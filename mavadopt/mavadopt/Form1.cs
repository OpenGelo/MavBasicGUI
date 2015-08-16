using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using ITMS.Network;
using ITMS.Logic;
using ITMS.Logic.MP_Utilities;
using ITMS.Logic.AM_Data;
using log4net;

using AForge;
using AForge.Controls;
using AForge.Video;
using AForge.Video.DirectShow;
//using AForge.Video.VFW;
//using AForge.Video.FFMPEG;
using AForge.Math;
using System.Reflection;

namespace mavadopt
{
    public partial class Form1 : Form
    {

        #region From MAVLINK
        private static readonly ILog log =
                LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Active Comport interface
        /// </summary>
        public static MAVLinkInterface comPort = new MAVLinkInterface();

        /// <summary>
        /// passive comports
        /// </summary>
        public static List<MAVLinkInterface> Comports = new List<MAVLinkInterface>();

        /// <summary>
        /// Comport namea
        /// </summary>
        public static string comPortName = "";

        /// <summary>
        /// use to store all internal config
        /// </summary>
        public static Hashtable config = new Hashtable();


        //public static Form1 instance = null;

        /// <summary>
        /// store the time we first connect
        /// </summary>
        DateTime connecttime = DateTime.Now;
        DateTime nodatawarning = DateTime.Now;
        DateTime OpenTime = DateTime.Now;

        /// <summary>
        /// controls the main serial reader thread
        /// </summary>
        bool serialThread = false;

        bool pluginthreadrun = false;

        bool joystickthreadrun = false;

        Thread serialreaderthread;

        ManualResetEvent SerialThreadrunner = new ManualResetEvent(false);

        /// <summary>
        /// track the last heartbeat sent
        /// </summary>
        private DateTime heatbeatSend = DateTime.Now;

        public static string LogDir
        {
            get
            {
                if (config["logdirectory"] == null)
                    return _logdir;
                return config["logdirectory"].ToString();
            }
            set
            {
                _logdir = value;
                config["logdirectory"] = value;
            }
        }
        static string _logdir = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + @"logs";
        #endregion

        static Form1 _mainInstance;
        public static Form1 GetInstance()
        {
            while (_mainInstance == null)
            {

            }

            return _mainInstance;
        }

        public Form1()
        {
            InitializeComponent();

            //Serial List
            PopulateSerialportList();
            //Video List
            //EnumerateVideoDevices();

            _mainInstance = this;

            #region MAV Adopt
            // create one here - but override on load
            Form1.config["guid"] = Guid.NewGuid().ToString();

            // load config
            xmlconfig(false);

            // setup guids for droneshare
            if (!Form1.config.ContainsKey("plane_guid"))
                Form1.config["plane_guid"] = Guid.NewGuid().ToString();

            if (!Form1.config.ContainsKey("copter_guid"))
                Form1.config["copter_guid"] = Guid.NewGuid().ToString();

            if (!Form1.config.ContainsKey("rover_guid"))
                Form1.config["rover_guid"] = Guid.NewGuid().ToString();

            // setup main serial reader
            serialreaderthread = new Thread(SerialReader)
            {
                IsBackground = true,
                Name = "Main Serial reader",
                Priority = ThreadPriority.AboveNormal
            };
            serialreaderthread.Start();
            #endregion
        }
        #region MAVLINK Adopt
        /// <summary>
        /// main serial reader thread
        /// controls
        /// serial reading
        /// link quality stats
        /// speech voltage - custom - alt warning - data lost
        /// heartbeat packet sending
        /// 
        /// and can't fall out
        /// </summary>
        private void SerialReader()
        {
            if (serialThread == true)
                return;
            serialThread = true;

            SerialThreadrunner.Reset();

            int minbytes = 0;

            int altwarningmax = 0;

            bool armedstatus = false;

            string lastmessagehigh = "";

            DateTime speechcustomtime = DateTime.Now;

            DateTime speechbatterytime = DateTime.Now;
            DateTime speechlowspeedtime = DateTime.Now;

            DateTime linkqualitytime = DateTime.Now;

            while (serialThread)
            {
                try
                {
                    Thread.Sleep(1); // was 5                

                    //// get home point on armed status change.
                    //if (armedstatus != Form1.comPort.MAV.cs.armed && comPort.BaseStream.IsOpen)
                    //{
                    //    armedstatus = Form1.comPort.MAV.cs.armed;
                    //    // status just changed to armed
                    //    if (Form1.comPort.MAV.cs.armed == true)
                    //    {
                    //        try
                    //        {
                    //            //Form1.comPort.MAV.cs.HomeLocation = new PointLatLngAlt(Form1.comPort.getWP(0));
                    //            //if (MyView.current != null && MyView.current.Name == "FlightPlanner")
                    //            //{
                    //            //    // update home if we are on flight data tab
                    //            //    FlightPlanner.updateHome();
                    //            //}
                    //        }
                    //        catch
                    //        {
                    //            // dont hang this loop
                    //            this.BeginInvoke((MethodInvoker)delegate { MessageBox.Show("Failed to update home location"); });
                    //        }
                    //    }
                    //}

                    // send a hb every seconds from gcs to ap
                    if (heatbeatSend.Second != DateTime.Now.Second)
                    {
                        MAVLink.mavlink_heartbeat_t htb = new MAVLink.mavlink_heartbeat_t()
                        {
                            type = (byte)MAVLink.MAV_TYPE.GCS,
                            autopilot = (byte)MAVLink.MAV_AUTOPILOT.INVALID,
                            mavlink_version = 3,
                        };

                        foreach (var port in Form1.Comports)
                        {
                            try
                            {
                                port.sendPacket(htb);
                            }
                            catch { }
                        }

                        heatbeatSend = DateTime.Now;
                    }

                    // if not connected or busy, sleep and loop
                    if (!comPort.BaseStream.IsOpen || comPort.giveComport == true)
                    {
                        if (!comPort.BaseStream.IsOpen)
                        {
                            // check if other ports are still open
                            foreach (var port in Comports)
                            {
                                if (port.BaseStream.IsOpen)
                                {
                                    Console.WriteLine("Main comport shut, swapping to other mav");
                                    comPort = port;
                                    break;
                                }
                            }
                        }

                        System.Threading.Thread.Sleep(100);
                        //continue;
                    }

                    // actualy read the packets
                    while (comPort.BaseStream.IsOpen && comPort.BaseStream.BytesToRead > minbytes && comPort.giveComport == false)
                    {
                        try
                        {
                            comPort.readPacket();
                        }
                        catch { }
                    }

                    // update currentstate of sysids on main port
                    foreach (var sysid in comPort.sysidseen)
                    {
                        try
                        {
                            comPort.MAVlist[sysid].cs.UpdateCurrentSettings(null, false, comPort, comPort.MAVlist[sysid]);
                        }
                        catch { }
                    }

                    //C_Log.writeLog(comPort.MAV.cs.pitch + " " + comPort.MAV.cs.roll + " " + comPort.MAV.cs.yaw + "\n\r");

                    this.Invoke((MethodInvoker)delegate
                    {
                        //update teks
                        //lbl_test.Visible = true;
                        //lbl_test.Text = "YAW :" + comPort.MAV.cs.yaw.ToString() + "\nAltitude :" + comPort.MAV.cs.alt.ToString();
                        //lbl_test.Text = "YAW :" + loop1.ToString() + "\nAltitude :" + loop2.ToString();
                        label1.Text = comPort.MAV.cs.yaw.ToString();
                        label2.Text = comPort.MAV.cs.pitch.ToString();
                        label3.Text = comPort.MAV.cs.roll.ToString();
                    });

                    ////Ngirim Data
                    //if (USock != null)
                    //{
                    //    if (USock.isConnected())
                    //    {
                    //        this.Invoke(sdtc);
                    //    }
                    //}

                    //// read the other interfaces
                    //foreach (var port in Comports)
                    //{
                    //    // skip primary interface
                    //    if (port == comPort)
                    //        continue;

                    //    if (!port.BaseStream.IsOpen)
                    //    {
                    //        // modify array and drop out
                    //        Comports.Remove(port);
                    //        break;
                    //    }

                    //    while (port.BaseStream.IsOpen && port.BaseStream.BytesToRead > minbytes)
                    //    {
                    //        try
                    //        {
                    //            port.readPacket();
                    //        }
                    //        catch { }
                    //    }
                    //    // update currentstate of sysids on the port
                    //    foreach (var sysid in port.sysidseen)
                    //    {
                    //        try
                    //        {
                    //            port.MAVlist[sysid].cs.UpdateCurrentSettings(null, false, port, port.MAVlist[sysid]);
                    //        }
                    //        catch { }
                    //    }
                    //}

                    //UPDATE JOYSTICK OUTPUT FROM SERIAL
                    //updateProgressJoystickInputSERIAL();

                    //COMPARE DELAY
                    //JoystickDelayCalculation();
                }
                catch (Exception e)
                {
                    log.Error("Serial Reader fail :" + e.ToString());
                    MessageBox.Show(e.ToString());
                    try
                    {
                        comPort.Close();
                    }
                    catch { }
                }
            }

            Console.WriteLine("SerialReader Done");
            SerialThreadrunner.Set();
        }


        public void xmlconfig(bool write)
        {
            if (write || !File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + @"config.xml"))
            {
                try
                {
                    log.Info("Saving config");

                    XmlTextWriter xmlwriter = new XmlTextWriter(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + @"config.xml", Encoding.ASCII);
                    xmlwriter.Formatting = Formatting.Indented;

                    xmlwriter.WriteStartDocument();

                    xmlwriter.WriteStartElement("Config");

                    xmlwriter.WriteElementString("comport", comPortName);

                    xmlwriter.WriteElementString("baudrate", cb_baudrate.Text);

                    xmlwriter.WriteElementString("APMFirmware", Form1.comPort.MAV.cs.firmware.ToString());

                    foreach (string key in config.Keys)
                    {
                        try
                        {
                            if (key == "" || key.Contains("/")) // "/dev/blah"
                                continue;
                            xmlwriter.WriteElementString(key, config[key].ToString());
                        }
                        catch { }
                    }

                    xmlwriter.WriteEndElement();

                    xmlwriter.WriteEndDocument();
                    xmlwriter.Close();
                }
                catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            }
            else
            {
                try
                {
                    using (XmlTextReader xmlreader = new XmlTextReader(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + @"config.xml"))
                    {
                        log.Info("Loading config");

                        while (xmlreader.Read())
                        {
                            xmlreader.MoveToElement();
                            try
                            {
                                switch (xmlreader.Name)
                                {
                                    case "comport":
                                        string temp = xmlreader.ReadString();

                                        cb_source.SelectedIndex = cb_source.FindString(temp);
                                        if (cb_source.SelectedIndex == -1)
                                        {
                                            cb_source.Text = temp; // allows ports that dont exist - yet
                                        }
                                        comPort.BaseStream.PortName = temp;
                                        comPortName = temp;
                                        break;
                                    case "baudrate":
                                        string temp2 = xmlreader.ReadString();

                                        cb_baudrate.SelectedIndex = cb_baudrate.FindString(temp2);
                                        if (cb_baudrate.SelectedIndex == -1)
                                        {
                                            cb_baudrate.Text = temp2;
                                            //CMB_baudrate.SelectedIndex = CMB_baudrate.FindString("57600"); ; // must exist
                                        }
                                        //bau = int.Parse(CMB_baudrate.Text);
                                        break;
                                    case "APMFirmware":
                                        //string temp3 = xmlreader.ReadString();
                                        //_connectionControl.TOOL_APMFirmware.SelectedIndex = _connectionControl.TOOL_APMFirmware.FindStringExact(temp3);
                                        //if (_connectionControl.TOOL_APMFirmware.SelectedIndex == -1)
                                        //    _connectionControl.TOOL_APMFirmware.SelectedIndex = 0;
                                        //Form1.comPort.MAV.cs.firmware = (Form1.Firmwares)Enum.Parse(typeof(Form1.Firmwares), _connectionControl.TOOL_APMFirmware.Text);
                                        break;
                                    case "Config":
                                        break;
                                    case "xml":
                                        break;
                                    default:
                                        if (xmlreader.Name == "") // line feeds
                                            break;
                                        config[xmlreader.Name] = xmlreader.ReadString();
                                        break;
                                }
                            }
                            // silent fail on bad entry
                            catch (Exception ee)
                            {
                                log.Error(ee);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Bad Config File", ex);
                }
            }
        }

        private void pb_connect_Click(object sender, EventArgs e)
        {
            //pb_connect.BackgroundImage = global::GCSServer.Properties.Resources.disconnect;
            comPort.giveComport = false;

            log.Info("MenuConnect Start");
            //lb_log.Items.Add("MenuConnect Start");

            // sanity check
            if (comPort.BaseStream.IsOpen && comPort.MAV.cs.groundspeed > 4)
            {
                if (DialogResult.No == MessageBox.Show(Strings.Stillmoving, Strings.Disconnect, MessageBoxButtons.YesNo))
                {
                    return;
                }
            }

            try
            {
                log.Info("Cleanup last logfiles");
                //lb_log.Items.Add("Cleanup last logfiles");
                // cleanup from any previous sessions
                if (comPort.logfile != null)
                    comPort.logfile.Close();

                if (comPort.rawlogfile != null)
                    comPort.rawlogfile.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Strings.ErrorClosingLogFile + ex.Message, Strings.ERROR);
            }

            comPort.logfile = null;
            comPort.rawlogfile = null;

            // decide if this is a connect or disconnect
            #region DISCONNECT
            // IF BUAT DISCONNECT, dont bother
            if (comPort.BaseStream.IsOpen)
            {
                log.Info("We are disconnecting");
                //lb_log.Items.Add("Disconnecting...");
                try
                {
                    //if (speechEngine != null) // cancel all pending speech
                    //    speechEngine.SpeakAsyncCancelAll();

                    comPort.BaseStream.DtrEnable = false;
                    comPort.Close();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }

                //// now that we have closed the connection, cancel the connection stats
                //// so that the 'time connected' etc does not grow, but the user can still
                //// look at the now frozen stats on the still open form
                //try
                //{
                //    // if terminal is used, then closed using this button.... exception
                //    if (this.connectionStatsForm != null)
                //        ((ConnectionStats)this.connectionStatsForm.Controls[0]).StopUpdates();
                //}
                //catch { }

                //// refresh config window if needed
                //if (MyView.current != null)
                //{
                //    if (MyView.current.Name == "HWConfig")
                //        MyView.ShowScreen("HWConfig");
                //    if (MyView.current.Name == "SWConfig")
                //        MyView.ShowScreen("SWConfig");
                //}

                //try
                //{
                //    System.Threading.ThreadPool.QueueUserWorkItem((WaitCallback)delegate
                //    {
                //        try
                //        {
                //            MissionPlanner.Log.LogSort.SortLogs(Directory.GetFiles(Form1.LogDir, "*.tlog"));
                //        }
                //        catch { }
                //    }
                //    );
                //}
                //catch { }

                //ganti gambar connect
                //this.btn_Start.Text = "CONNECT";
                changeConnectStatusDisplay("CONNECT");
            }
            #endregion
            #region CONNECT
            else
            {
                log.Info("We are connecting");
                //lb_log.Items.Add("Connecting....");
                switch (cb_source.Text)
                {
                    //case "TCP":
                    //    comPort.BaseStream = new TcpSerial();
                    //    break;
                    //case "UDP":
                    //    comPort.BaseStream = new UdpSerial();
                    //    break;
                    //case "UDPCl":
                    //    comPort.BaseStream = new UdpSerialConnect();
                    //    break;
                    case "AUTO"://kalau auto defaultnya pake serial port
                    default:
                        comPort.BaseStream = new SerialPort();
                        break;
                }

                //RESET current state ,dont bother
                comPort.MAV.cs.ResetInternals();

                //cleanup any log being played, dont bother
                comPort.logreadmode = false;
                if (comPort.logplaybackfile != null)
                    comPort.logplaybackfile.Close();
                comPort.logplaybackfile = null;

                try
                {
                    // do autoscan
                    if (cb_source.Text == "AUTO")
                    {
                        //Panggil scan supaya ngisi variable portinterface
                        CommsSerialScan.Scan(false);

                        DateTime deadline = DateTime.Now.AddSeconds(50);

                        //klo portnya ga ketemu2 return dari fungsi dan say fail
                        while (CommsSerialScan.foundport == false)
                        {
                            System.Threading.Thread.Sleep(100);

                            if (DateTime.Now > deadline)
                            {
                                MessageBox.Show(Strings.Timeout);
                                //_connectionControl.IsConnected(false);
                                return;
                            }
                        }

                        //ambil portname dan baud lgs dari variable portinterface
                        //_connectionControl.CMB_serialport.Text = Comms.CommsSerialScan.portinterface.PortName;
                        //_connectionControl.CMB_baudrate.Text = Comms.CommsSerialScan.portinterface.BaudRate.ToString();
                        cb_source.Text = CommsSerialScan.portinterface.PortName;
                        cb_baudrate.Text = CommsSerialScan.portinterface.BaudRate.ToString();
                    }

                    log.Info("Set Portname");
                    // set port, then options
                    comPort.BaseStream.PortName = cb_source.Text;

                    log.Info("Set Baudrate");
                    try
                    {
                        comPort.BaseStream.BaudRate = int.Parse(cb_baudrate.Text);
                    }
                    catch (Exception exp)
                    {
                        log.Error(exp);
                    }
                    //lb_log.Items.Add("Port Name : " + cb_source.Text + ",Baudrate : " + cb_baudrate.Text);

                    // prevent serialreader from doing anything
                    comPort.giveComport = true;//jika true maka exclusive use

                    log.Info("About to do dtr if needed");
                    // reset on connect logic. ,buat DTR - pentingkah??
                    if (config["CHK_resetapmonconnect"] == null || bool.Parse(config["CHK_resetapmonconnect"].ToString()) == true)
                    {
                        log.Info("set dtr rts to false");
                        comPort.BaseStream.DtrEnable = false;
                        comPort.BaseStream.RtsEnable = false;

                        comPort.BaseStream.toggleDTR();
                    }

                    comPort.giveComport = false;

                    // setup to record new logs , dont bother
                    try
                    {
                        Directory.CreateDirectory(Form1.LogDir);
                        comPort.logfile = new BufferedStream(File.Open(Form1.LogDir + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".tlog", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None));

                        comPort.rawlogfile = new BufferedStream(File.Open(Form1.LogDir + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".rlog", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None));

                        log.Info("creating logfile " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".tlog");
                    }
                    catch (Exception exp2) { log.Error(exp2); MessageBox.Show(Strings.Failclog); } // soft fail

                    // reset connect time - for timeout functions
                    connecttime = DateTime.Now;

                    #region Connectnya
                    // do the connect - CONNECT pake mavlink
                    comPort.Open(false);

                    if (!comPort.BaseStream.IsOpen)//klo g open maka close yg exisitng, mungkin gara2 ngeunplug tanpa disconnect
                    {
                        log.Info("comport is closed. existing connect");
                        try
                        {
                            //_connectionControl.IsConnected(false);
                            //UpdateConnectIcon();
                            changeConnectStatusDisplay("CONNECT");
                            comPort.Close();
                        }
                        catch { }
                        return;
                    }

                    //-------------------------NANTI AJA
                    //// KALAU ADA ID lebih dari satu maka bisa pilih yg mana, masukin ke sysidcurrent
                    //// 3dr radio is hidden as no hb packet is ever emitted
                    //if (comPort.sysidseen.Count > 1)
                    //{
                    //    // we have more than one mav
                    //    // user selection of sysid
                    //    MissionPlanner.Controls.SysidSelector id = new SysidSelector();

                    //    id.ShowDialog();
                    //}
                    #endregion

                    comPort.getParamList();

                    #region masalah firmware
                    //// detect firmware we are conected to.
                    //if (comPort.MAV.cs.firmware == Firmwares.ArduCopter2)
                    //{
                    //    _connectionControl.TOOL_APMFirmware.SelectedIndex = _connectionControl.TOOL_APMFirmware.Items.IndexOf(Firmwares.ArduCopter2);
                    //}
                    //else if (comPort.MAV.cs.firmware == Firmwares.Ateryx)
                    //{
                    //    _connectionControl.TOOL_APMFirmware.SelectedIndex = _connectionControl.TOOL_APMFirmware.Items.IndexOf(Firmwares.Ateryx);
                    //}
                    //else if (comPort.MAV.cs.firmware == Firmwares.ArduRover)
                    //{
                    //    _connectionControl.TOOL_APMFirmware.SelectedIndex = _connectionControl.TOOL_APMFirmware.Items.IndexOf(Firmwares.ArduRover);
                    //}
                    //else if (comPort.MAV.cs.firmware == Firmwares.ArduPlane)
                    //{
                    //    _connectionControl.TOOL_APMFirmware.SelectedIndex = _connectionControl.TOOL_APMFirmware.Items.IndexOf(Firmwares.ArduPlane);
                    //}

                    //// check for newer firmware
                    //var softwares = Firmware.LoadSoftwares();

                    //if (softwares.Count > 0)
                    //{
                    //    try
                    //    {
                    //        string[] fields1 = comPort.MAV.VersionString.Split(' ');

                    //        foreach (Firmware.software item in softwares)
                    //        {
                    //            string[] fields2 = item.name.Split(' ');

                    //            // check primare firmware type. ie arudplane, arducopter
                    //            if (fields1[0] == fields2[0])
                    //            {
                    //                Version ver1 = VersionDetection.GetVersion(comPort.MAV.VersionString);
                    //                Version ver2 = VersionDetection.GetVersion(item.name);

                    //                if (ver2 > ver1)
                    //                {
                    //                    Common.MessageShowAgain(Strings.NewFirmware, Strings.NewFirmwareA + item.name + Strings.Pleaseup);
                    //                    break;
                    //                }

                    //                // check the first hit only
                    //                break;
                    //            }
                    //        }
                    //    }
                    //    catch (Exception ex) { log.Error(ex); }
                    //}
                    #endregion

                    #region Miscellaneous
                    //FlightData.CheckBatteryShow();

                    //MissionPlanner.Utilities.Tracking.AddEvent("Connect", "Connect", comPort.MAV.cs.firmware.ToString(), comPort.MAV.param.Count.ToString());
                    //MissionPlanner.Utilities.Tracking.AddTiming("Connect", "Connect Time", (DateTime.Now - connecttime).TotalMilliseconds, "");

                    //MissionPlanner.Utilities.Tracking.AddEvent("Connect", "Baud", comPort.BaseStream.BaudRate.ToString(), "");

                    // save the baudrate for this port
                    config[cb_baudrate.Text + "_BAUD"] = cb_baudrate.Text;

                    this.Text = "GCS Server " + comPort.MAV.VersionString;

                    //// refresh config window if needed
                    //if (MyView.current != null)
                    //{
                    //    if (MyView.current.Name == "HWConfig")
                    //        MyView.ShowScreen("HWConfig");
                    //    if (MyView.current.Name == "SWConfig")
                    //        MyView.ShowScreen("SWConfig");
                    //}


                    //// load wps on connect option.
                    //if (config["loadwpsonconnect"] != null && bool.Parse(config["loadwpsonconnect"].ToString()) == true)
                    //{
                    //    // only do it if we are connected.
                    //    if (comPort.BaseStream.IsOpen)
                    //    {
                    //        MenuFlightPlanner_Click(null, null);
                    //        FlightPlanner.BUT_read_Click(null, null);
                    //    }
                    //}
                    #endregion
                    //GANTI GAMBR DISCONNECT
                    // set connected icon
                    //this.MenuConnect.Image = displayicons.disconnect;
                    MessageBox.Show(Strings.Done);
                    //lb_log.Items.Add("Done!");
                    changeConnectStatusDisplay("DISCONNECT");
                }
                catch (Exception ex)
                {
                    log.Warn(ex);
                    try
                    {
                        //_connectionControl.IsConnected(false);
                        //UpdateConnectIcon();
                        comPort.Close();
                    }
                    catch { }
                    MessageBox.Show("Can not establish a connection\n\n" + ex.Message);
                    //lb_log.Items.Add("Connection failed");
                    return;
                }
            }
            #endregion
        }

        void changeConnectStatusDisplay(string conStat)
        {
            //if (_mainInstance != null)
            //{
            //    //jika bukan sedang di thread GUI
            //    if (Form1.GetInstance().InvokeRequired)
            //    {
            //        Form1.GetInstance().BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
            //        {
            //            //btn_Start.Text = conStat;
            //            if (conStat == "CONNECT")
            //            {
            //                pb_connect.BackgroundImage = global::GCSServer.Properties.Resources.connect;
            //                isSerialConnected = true;
            //            }
            //            else
            //            {
            //                pb_connect.BackgroundImage = global::GCSServer.Properties.Resources.disconnect;
            //                isSerialConnected = false;
            //            }
            //            lbl_connect.Text = conStat;
            //        });
            //    }
            //    else
            //    {
            //        //btn_Start.Text = conStat;
            //        if (conStat == "CONNECT")
            //        {
            //            pb_connect.BackgroundImage = global::GCSServer.Properties.Resources.connect;
            //            isSerialConnected = true;
            //        }
            //        else
            //        {
            //            pb_connect.BackgroundImage = global::GCSServer.Properties.Resources.disconnect;
            //            isSerialConnected = false;
            //        }
            //        lbl_connect.Text = conStat;
            //    }
            //}
        }

        private void PopulateSerialportList()
        {
            cb_source.Items.Clear();
            cb_source.Items.Add("AUTO");
            cb_source.Items.AddRange(SerialPort.GetPortNames());
            cb_source.Items.Add("TCP");
            cb_source.Items.Add("UDP");
            cb_source.Items.Add("UDPCl");

            cb_baudrate.Items.Clear();
            foreach (int _baud in CommsSerialScan.bauds)
            {
                cb_baudrate.Items.Add(_baud.ToString());
            }
            cb_baudrate.Text = CommsSerialScan.bauds[0].ToString();
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {

        }

    }
}
