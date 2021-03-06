﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;

using ITMS.Logic.AM_Attributes;
using ITMS.Logic.AM_Utilities;
using log4net;

namespace ITMS.Logic.AM_Data
{
    public enum Firmwares
    {
        ArduPlane,
        ArduCopter2,
        //ArduHeli,
        ArduRover,
        Ateryx,
        ArduTracker
    }

    public class Common
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public enum distances
        {
            Meters,
            Feet
        }

        public enum speeds
        {
            ms,
            fps,
            kph,
            mph,
            knots
        }


        /// <summary>
        /// from libraries\AP_Math\rotations.h
        /// </summary>
        public enum Rotation
        {
            ROTATION_NONE = 0,
            ROTATION_YAW_45,
            ROTATION_YAW_90,
            ROTATION_YAW_135,
            ROTATION_YAW_180,
            ROTATION_YAW_225,
            ROTATION_YAW_270,
            ROTATION_YAW_315,
            ROTATION_ROLL_180,
            ROTATION_ROLL_180_YAW_45,
            ROTATION_ROLL_180_YAW_90,
            ROTATION_ROLL_180_YAW_135,
            ROTATION_PITCH_180,
            ROTATION_ROLL_180_YAW_225,
            ROTATION_ROLL_180_YAW_270,
            ROTATION_ROLL_180_YAW_315,
            ROTATION_ROLL_90,
            ROTATION_ROLL_90_YAW_45,
            ROTATION_ROLL_90_YAW_90,
            ROTATION_ROLL_90_YAW_135,
            ROTATION_ROLL_270,
            ROTATION_ROLL_270_YAW_45,
            ROTATION_ROLL_270_YAW_90,
            ROTATION_ROLL_270_YAW_135,
            ROTATION_PITCH_90,
            ROTATION_PITCH_270,
            ROTATION_MAX
        }


        public enum ap_product
        {
            [DisplayText("HIL")]
            AP_PRODUCT_ID_NONE = 0x00,	// Hardware in the loop
            [DisplayText("APM1 1280")]
            AP_PRODUCT_ID_APM1_1280 = 0x01,// APM1 with 1280 CPUs
            [DisplayText("APM1 2560")]
            AP_PRODUCT_ID_APM1_2560 = 0x02,// APM1 with 2560 CPUs
            [DisplayText("SITL")]
            AP_PRODUCT_ID_SITL = 0x03,// Software in the loop
            [DisplayText("PX4")]
            AP_PRODUCT_ID_PX4 = 0x04,   // PX4 on NuttX
            [DisplayText("PX4 FMU 2")]
            AP_PRODUCT_ID_PX4_V2 = 0x05,   // PX4 FMU2 on NuttX
            [DisplayText("APM2 ES C4")]
            AP_PRODUCT_ID_APM2ES_REV_C4 = 0x14,// APM2 with MPU6000ES_REV_C4
            [DisplayText("APM2 ES C5")]
            AP_PRODUCT_ID_APM2ES_REV_C5 = 0x15,	// APM2 with MPU6000ES_REV_C5
            [DisplayText("APM2 ES D6")]
            AP_PRODUCT_ID_APM2ES_REV_D6 = 0x16,	// APM2 with MPU6000ES_REV_D6
            [DisplayText("APM2 ES D7")]
            AP_PRODUCT_ID_APM2ES_REV_D7 = 0x17,	// APM2 with MPU6000ES_REV_D7
            [DisplayText("APM2 ES D8")]
            AP_PRODUCT_ID_APM2ES_REV_D8 = 0x18,	// APM2 with MPU6000ES_REV_D8	
            [DisplayText("APM2 C4")]
            AP_PRODUCT_ID_APM2_REV_C4 = 0x54,// APM2 with MPU6000_REV_C4 	
            [DisplayText("APM2 C5")]
            AP_PRODUCT_ID_APM2_REV_C5 = 0x55,	// APM2 with MPU6000_REV_C5 	
            [DisplayText("APM2 D6")]
            AP_PRODUCT_ID_APM2_REV_D6 = 0x56,	// APM2 with MPU6000_REV_D6 		
            [DisplayText("APM2 D7")]
            AP_PRODUCT_ID_APM2_REV_D7 = 0x57,	// APM2 with MPU6000_REV_D7 	
            [DisplayText("APM2 D8")]
            AP_PRODUCT_ID_APM2_REV_D8 = 0x58,	// APM2 with MPU6000_REV_D8 	
            [DisplayText("APM2 D9")]
            AP_PRODUCT_ID_APM2_REV_D9 = 0x59,	// APM2 with MPU6000_REV_D9 
            [DisplayText("FlyMaple")]
            AP_PRODUCT_ID_FLYMAPLE = 0x100,   // Flymaple with ITG3205, ADXL345, HMC5883, BMP085
            [DisplayText("Linux")]
            AP_PRODUCT_ID_L3G4200D = 0x101,   // Linux with L3G4200D and ADXL345
        }

        //TUTUP - ADAM
    //    public static bool getFilefromNet(string url, string saveto)
    //    {
    //        try
    //        {
    //            // this is for mono to a ssl server
    //            //ServicePointManager.CertificatePolicy = new NoCheckCertificatePolicy(); 

    //            ServicePointManager.ServerCertificateValidationCallback =
    //new System.Net.Security.RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });

    //            log.Info(url);
    //            // Create a request using a URL that can receive a post. 
    //            WebRequest request = WebRequest.Create(url);
    //            request.Timeout = 10000;
    //            // Set the Method property of the request to POST.
    //            request.Method = "GET";
    //            // Get the response.
    //            WebResponse response = request.GetResponse();
    //            // Display the status.
    //            log.Info(((HttpWebResponse)response).StatusDescription);
    //            if (((HttpWebResponse)response).StatusCode != HttpStatusCode.OK)
    //                return false;
    //            // Get the stream containing content returned by the server.
    //            Stream dataStream = response.GetResponseStream();

    //            long bytes = response.ContentLength;
    //            long contlen = bytes;

    //            byte[] buf1 = new byte[1024];

    //            if (!Directory.Exists(Path.GetDirectoryName(saveto)))
    //                Directory.CreateDirectory(Path.GetDirectoryName(saveto));

    //            FileStream fs = new FileStream(saveto + ".new", FileMode.Create);

    //            DateTime dt = DateTime.Now;

    //            while (dataStream.CanRead && bytes > 0)
    //            {
    //                Application.DoEvents();
    //                log.Debug(saveto + " " + bytes);
    //                int len = dataStream.Read(buf1, 0, buf1.Length);
    //                bytes -= len;
    //                fs.Write(buf1, 0, len);
    //            }

    //            fs.Close();
    //            dataStream.Close();
    //            response.Close();

    //            File.Delete(saveto);
    //            File.Move(saveto + ".new", saveto);

    //            return true;
    //        }
    //        catch (Exception ex) { log.Info("getFilefromNet(): " + ex.ToString()); return false; }
        //    }

        #region TUTUP - ADAM
        public static List<KeyValuePair<int, string>> getModesList(CurrentState cs)
        {
            log.Info("getModesList Called");

            if (cs.firmware == Firmwares.ArduPlane)
            {
                var flightModes = ParameterMetaDataRepository.GetParameterOptionsInt("FLTMODE1", cs.firmware.ToString());
                flightModes.Add(new KeyValuePair<int, string>(16, "INITIALISING"));
                return flightModes;
            }
            else if (cs.firmware == Firmwares.Ateryx)
            {
                var flightModes = ParameterMetaDataRepository.GetParameterOptionsInt("FLTMODE1", cs.firmware.ToString()); //same as apm
                return flightModes;
            }
            else if (cs.firmware == Firmwares.ArduCopter2)
            {
                var flightModes = ParameterMetaDataRepository.GetParameterOptionsInt("FLTMODE1", cs.firmware.ToString());
                return flightModes;
            }
            else if (cs.firmware == Firmwares.ArduRover)
            {
                var flightModes = ParameterMetaDataRepository.GetParameterOptionsInt("MODE1", cs.firmware.ToString());
                return flightModes;
            }
            else if (cs.firmware == Firmwares.ArduTracker)
            {
                var temp = new List<KeyValuePair<int, string>>();
                temp.Add(new KeyValuePair<int, string>(0, "MANUAL"));
                temp.Add(new KeyValuePair<int, string>(1, "STOP"));
                temp.Add(new KeyValuePair<int, string>(2, "SCAN"));
                temp.Add(new KeyValuePair<int, string>(10, "AUTO"));
                temp.Add(new KeyValuePair<int, string>(16, "INITIALISING"));

                return temp;
            }

            return null;
        }
        #endregion

        #region TUTUP - ADAM
        //public static Form LoadingBox(string title, string promptText)
        //{
        //    Form form = new Form();
        //    System.Windows.Forms.Label label = new System.Windows.Forms.Label();
        //    System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainV2));
        //    form.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));

        //    form.Text = title;
        //    label.Text = promptText;

        //    label.SetBounds(9, 50, 372, 13);

        //    label.AutoSize = true;

        //    form.ClientSize = new Size(396, 107);
        //    form.Controls.AddRange(new Control[] { label });
        //    form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
        //    form.FormBorderStyle = FormBorderStyle.FixedDialog;
        //    form.StartPosition = FormStartPosition.CenterScreen;
        //    form.MinimizeBox = false;
        //    form.MaximizeBox = false;

        //    //ThemeManager.ApplyThemeTo(form);

        //    form.Show();
        //    form.Refresh();
        //    label.Refresh();
        //    Application.DoEvents();
        //    return form;
        //}
        #endregion

        #region TUTUP - ADAM
        //public static DialogResult MessageShowAgain(string title, string promptText)
        //{
        //    Form form = new Form();
        //    System.Windows.Forms.Label label = new System.Windows.Forms.Label();
        //    CheckBox chk = new CheckBox();
        //    Controls.MyButton buttonOk = new Controls.MyButton();
        //    System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainV2));
        //    form.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));

        //    form.Text = title;
        //    label.Text = promptText;

        //    chk.Tag = ("SHOWAGAIN_" + title.Replace(" ", "_"));
        //    chk.AutoSize = true;
        //    chk.Text = "Show me again?";
        //    chk.Checked = true;
        //    chk.Location = new Point(9, 80);

        //    if (MainV2.config[(string)chk.Tag] != null && (string)MainV2.config[(string)chk.Tag] == "False") // skip it
        //    {
        //        form.Dispose();
        //        chk.Dispose();
        //        buttonOk.Dispose();
        //        label.Dispose();
        //        return DialogResult.OK;
        //    }

        //    chk.CheckStateChanged += new EventHandler(chk_CheckStateChanged);

        //    buttonOk.Text = "OK";
        //    buttonOk.DialogResult = DialogResult.OK;
        //    buttonOk.Location = new Point(form.Right - 100, 80);

        //    label.SetBounds(9, 40, 372, 13);

        //    label.AutoSize = true;

        //    form.ClientSize = new Size(396, 107);
        //    form.Controls.AddRange(new Control[] { label, chk, buttonOk });
        //    form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
        //    form.FormBorderStyle = FormBorderStyle.FixedDialog;
        //    form.StartPosition = FormStartPosition.CenterScreen;
        //    form.MinimizeBox = false;
        //    form.MaximizeBox = false;

        //    ThemeManager.ApplyThemeTo(form);

        //    DialogResult dialogResult = form.ShowDialog();

        //    form.Dispose();

        //    form = null;

        //    return dialogResult;
        //}
        #endregion

        #region TUTUP - ADAM
        //static void chk_CheckStateChanged(object sender, EventArgs e)
        //{
        //    MainV2.config[(string)((CheckBox)(sender)).Tag] = ((CheckBox)(sender)).Checked.ToString();
        //}
        #endregion
        
        #region TUTUP - ADAM
        //public static string speechConversion(string input)
        //{
        //    if (MainV2.comPort.MAV.cs.wpno == 0)
        //    {
        //        input = input.Replace("{wpn}", "Home");
        //    }
        //    else
        //    {
        //        input = input.Replace("{wpn}", MainV2.comPort.MAV.cs.wpno.ToString());
        //    }

        //    input = input.Replace("{asp}", MainV2.comPort.MAV.cs.airspeed.ToString("0"));

        //    input = input.Replace("{alt}", MainV2.comPort.MAV.cs.alt.ToString("0"));

        //    input = input.Replace("{wpa}", MainV2.comPort.MAV.cs.targetalt.ToString("0"));

        //    input = input.Replace("{gsp}", MainV2.comPort.MAV.cs.groundspeed.ToString("0"));

        //    input = input.Replace("{mode}", MainV2.comPort.MAV.cs.mode.ToString());

        //    input = input.Replace("{batv}", MainV2.comPort.MAV.cs.battery_voltage.ToString("0.00"));

        //    input = input.Replace("{batp}", (MainV2.comPort.MAV.cs.battery_remaining).ToString("0"));

        //    input = input.Replace("{vsp}", (MainV2.comPort.MAV.cs.verticalspeed).ToString("0.0"));

        //    input = input.Replace("{curr}", (MainV2.comPort.MAV.cs.current).ToString("0.0"));

        //    return input;
        //}
        #endregion
    }

}