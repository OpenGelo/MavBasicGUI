using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ITMS.Logic.MP_Utilities;
using mavadopt;

namespace ITMS.Network
{
    public class CommsSerialScan
    {
        static public bool foundport = false;
        static public ICommsSerial portinterface;

        public static int run = 0;
        public static int running = 0;
        static bool connect = false;

        //change to public - adam
       static  public int[] bauds = new int[] { 115200, 57600, 38400, 19200, 9600 };

        //method scan auto port, output kopi instance ke form utama
        static public void Scan(bool connect = false)
        {
            foundport = false;
            portinterface = null;
            run = 0;
            running = 0;
            CommsSerialScan.connect = connect;

            List<MAVLinkInterface> scanports = new List<MAVLinkInterface>();

            string[] portlist = SerialPort.GetPortNames();

            foreach (string port in portlist)
            {
                scanports.Add(new MAVLinkInterface() { BaseStream = new SerialPort() { PortName = port, BaudRate = bauds[0] } });
            }

            foreach (MAVLinkInterface inter in scanports)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(doread, inter);
            }
        }

        static void doread(object o)
        {
            run++;
            running++;

            MAVLinkInterface port = (MAVLinkInterface)o;

            try
            {
                //coba open lalu cek packet length
                port.BaseStream.Open();//open serialport/udp/tcp dsb, bukan open di mavlink(beda method)

                int baud = 0;

            redo:

                DateTime deadline = DateTime.Now.AddSeconds(5);

                while (DateTime.Now < deadline)
                {
                    Console.WriteLine("Scan port {0} at {1}", port.BaseStream.PortName, port.BaseStream.BaudRate);

                    byte[] packet = new byte[0];

                    try
                    {
                        packet = port.readPacket();
                    }
                    catch { }

                    if (packet.Length > 0)
                    {
                        port.BaseStream.Close();//jika ketemu close dulu

                        Console.WriteLine("Found Mavlink on port {0} at {1}", port.BaseStream.PortName, port.BaseStream.BaudRate);

                        foundport = true;
                        portinterface = port.BaseStream;

                        //GA pernah ada yg true kan, jadi doconnect g akan pernah dipanggil, connect yg mavlink lgs dr form1
                        if (CommsSerialScan.connect)//jika connect true maka kopi ke instance form1
                        {
                            //ngopinya g dari sini tapi diambil lgs dari portinterface di form1
                            Form1.comPort.BaseStream = port.BaseStream;

                            doconnect();
                        }

                        running--;

                        return;
                    }

                    if (foundport)
                        break;
                }

                //jika ga ketemu looping while diatas maka ganti baudrate
                if (!foundport && port.BaseStream.BaudRate > 0)
                {
                    baud++;
                    if (baud < bauds.Length)
                    {
                        port.BaseStream.BaudRate = bauds[baud];
                        goto redo;
                    }
                }

                try
                {
                    port.BaseStream.Close();
                }
                catch { }
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }

            running--;

            Console.WriteLine("Scan port {0} Finished!!", port.BaseStream.PortName);
        }

        static void doconnect()
        {
           
            //jika MainV2 blm ada instancenya set false, otherwise true
            if (Form1.GetInstance() == null)//if (P_0601_Setup.instance == null)
            {
                Form1.comPort.Open(false);
            }
            else
            {
                //jika bukan sedang di thread GUI
                if (Form1.GetInstance().InvokeRequired)
                {
                    Form1.GetInstance().BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
                    {
                        Form1.comPort.Open(true);
                    });
                }
                else
                {
                    Form1.comPort.Open(true);
                }
            }
        }
    }
}
