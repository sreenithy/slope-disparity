using System;
using System.Diagnostics;
using UserScriptHost;
using System.Collections.Generic;
using System.Linq;
using EpiscanUtil;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

public class DelayPCSweepScript : IEpiscanScript
{
    public int DelayMin { set; get; } = -500;
    public int DelayMax { set; get; } = 500;
    public int Shift { set; get; } = 25;
    public float Exposure { set; get; } = 0.3f;
    public int AverageNum { set; get; } = 4;
    public int PCMin { set; get; } = 80;
    public int PCMax { set; get; } = 80;
    public int PCstep { set; get; } = 0;

    public void Configure(MyEpiscan episcan)
    {
        Debug.Write("hello");
    }

    public async Task Run(MyEpiscan episcan)
    {
        // generate patterns


        // Switch to synchronized mode
        episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Rolling;

        episcan.SetDelayExposure(0, Exposure);
        episcan.Screen.BackColor = System.Drawing.Color.Black;
        await Task.Delay(100);
        var black = await episcan.CaptureAverage(AverageNum);
        Cv2.ImWrite("capture_black.png", black);


        episcan.Screen.BackColor = System.Drawing.Color.White;
        await Task.Delay(100);

        var sfd = new SaveFileDialog()
        {
            Filter = "PNG file (*.png)|*.png|All files(*.*)|*.*",
            FileName = "capture.png"
        };

        var basename = Path.GetDirectoryName(sfd.FileName);

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            int i = 0;
            for (int pc = PCMin; pc <= PCMax; pc += PCstep)
            {
                var pc_dir = basename + "/" + pc.ToString();
                //Check if directory exists or not
                if (Directory.Exists(pc_dir))
                {
                    throw new System.ArgumentException("Already the directory exists", "pc_dir");
                }
                else
                {//If not create one
                    Directory.CreateDirectory(pc_dir);
                }

                for (int delay = DelayMin; delay <= DelayMax; delay += Shift)
                {
                    episcan.SetDelayExposurePC(delay, Exposure, pc);
                    await Task.Delay(1);

                    var captured = await episcan.CaptureAverage(AverageNum);
                    var filename = basename + "/capture_{i:D5}_{(int)delay:D5}.png";
                    Cv2.ImWrite(filename, captured);
                    i++;
                }
            }
        }
    }
}
/*
 * 
 * using EpiscanUtil;
using HelperCamUtil;
using UserScriptHost;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using OpenCvSharp;
using Size = System.Drawing.Size;
using Point = System.Drawing.Point;

namespace EpiscanScriptDemo
{
    public partial class Form1 : Form
    {
        private MyEpiscan episcan = null;
        private HelperCam helpercam = null;
        private CamView camView = null;
        private CamView_h camView_h = null;

        ///// <summary>
        ///// Episcan Sensor
        ///// </summary>
        //private EpiscanUtil.MySensor episcan.Sensor = null;

        ///// <summary>
        ///// screen to show on projector
        ///// </summary>
        //Form episcan.Screen = null;

        /// <summary>
        /// Camera parameter
        /// </summary>
        public class CameraParameter
        {
            //public int Delay = 0;
            // gray
            // public int DelayOffset = 10975 + 250;
            // color
            //public int DelayOffset = 6450;

            // sony-gray
            public int DelayOffset = 7000;
            public float Exposure = 0.5f;
            // celluon color, gray
            public int PixelClock = 60;
            

            public EpiscanUtil.MySensor.ShutterModeList ShutterMode = EpiscanUtil.MySensor.ShutterModeList.Global;
            public EpiscanUtil.MySensor.TriggerModeList TriggerMode = EpiscanUtil.MySensor.TriggerModeList.RisingEdge;

            public HelperCamUtil.HelperSensor.ShutterModeList_h ShutterMode_h = HelperCamUtil.HelperSensor.ShutterModeList_h.Global;
            //public HelperCamUtil.HelperSensor.TriggerModeList_h TriggerMode_h = HelperCamUtil.HelperSensor.TriggerModeList_h.RisingEdge;


            //public int Width = 1280;
            //public int Height = 1024;
            public int Left = 0;
            public int Top = 0;
            public int Width = 1600;
            public int Height = 1200;
            public bool EnableGainBoost = false;
            public int MasterGain = 20;
            public int Scaler = 100;
            public float FrameRate = 30f;
            public EpiscanUtil.MySensor.PixelFormatList PixelFormat = EpiscanUtil.MySensor.PixelFormatList.Mono12;
            public HelperCamUtil.HelperSensor.PixelFormatList_h PixelFormat_h = HelperCamUtil.HelperSensor.PixelFormatList_h.Mono12;

            public void SaveAsXml(string path)
            {
                System.Xml.Serialization.XmlSerializer serializer =
                    new System.Xml.Serialization.XmlSerializer(typeof(CameraParameter));
                System.IO.StreamWriter sw = new System.IO.StreamWriter(path, false, new System.Text.UTF8Encoding(false));
                serializer.Serialize(sw, this);
                sw.Close();
            }
            public static CameraParameter FromXml(string path)
            {
                System.Xml.Serialization.XmlSerializer serializer =
                    new System.Xml.Serialization.XmlSerializer(typeof(CameraParameter));
                System.IO.StreamReader sr = new System.IO.StreamReader(path, new System.Text.UTF8Encoding(false));
                CameraParameter cp = (CameraParameter)serializer.Deserialize(sr);
                //ファイルを閉じる
                sr.Close();

                return cp;
            }
        };

        public CameraParameter _preset = new CameraParameter();
        public CameraParameter _preset_h = new CameraParameter();


        public Form1()
        {
            InitializeComponent();

            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));

            episcan = new MyEpiscan();
            helpercam = new HelperCam();

            UpdateCameraProfileList();

            UpdateCameraList();
            InitUISyncboard();
            InitUIScreen();
            InitUIScript();

            // camera groupbox disable
            this._cameraGb.Enabled = false;

            // episcan groupbox disable
            this._episcanGb.Enabled = false;

            //
            this._displayColorCb.SelectedIndex = 0;

            // Set event to receive event notification when display settings change.
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingChanged);

        }

        private void SystemEvents_DisplaySettingChanged(object sender, EventArgs e)
        {
            InitUIScreen();
        }

        void InitUISyncboard()
        {
            // Register available com port to dropdown list
            this.serialsCb.Items.Clear();
            this.serialsCb.BeginUpdate();
            foreach (var port in SyncBoard.AvailablePortNames) this.serialsCb.Items.Add(port);
            if(this.serialsCb.Items.Count >0) this.serialsCb.SelectedIndex = 0;
            this.serialsCb.EndUpdate();

            DetectSyncBoard();
        }

        void InitUIScreen()
        {
            // celluon pico projector width supposed to be 1280
            int supposedWidth = 1280;

            int projIndex = -1;

            // Register Screens
            this._screenCb.Items.Clear();
            this._screenCb.BeginUpdate();
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                Screen scr = Screen.AllScreens[i];
                this._screenCb.Items.Add("[" + i.ToString() + "]" + scr.Bounds.Width.ToString() + "x" + scr.Bounds.Height.ToString());

                if(scr.Bounds.Width == supposedWidth)
                {
                    projIndex = i;
                }
            }
            this._screenCb.EndUpdate();

            // set default
            this._screenCb.SelectedIndex = projIndex != -1 ? projIndex : Screen.AllScreens.Length - 1;
            episcan.Screen.Create(_screenCb.SelectedIndex);
        }
        private void UpdateCameraList()
        {
            string[] models;
            long[] devids;
            EpiscanUtil.MySensor.GetCameraList(out models, out devids);

            // Register available cameras
            this._cameraCb.Items.Clear();
            this._cameraCb.BeginUpdate();
            for(int i = 0; i < models.Length; i++)
            {
                this._cameraCb.Items.Add(models[i] + "(" + devids[i].ToString() + ")");
            }
            if (this._cameraCb.Items.Count > 0) this._cameraCb.SelectedIndex = 0;
            this._cameraCb.EndUpdate();

        }
        void InitUI()
        {
            // Register Color Mode
            this._cmodeCb.Items.Clear();
            this._cmodeCb.BeginUpdate();
            foreach (EpiscanUtil.MySensor.PixelFormatList p in Enum.GetValues(typeof(EpiscanUtil.MySensor.PixelFormatList)))
                this._cmodeCb.Items.Add(p.ToString());
            this._cmodeCb.EndUpdate();
        }
        void InitUIfromCamera()
        {
            // Register available pixel clocks to dropdown list
            this._pixelClockCb.Items.Clear();
            this._pixelClockCb.BeginUpdate();
            foreach (var pc in episcan.Sensor.AvailablePixelClock) this._pixelClockCb.Items.Add(pc.ToString());
            this._pixelClockCb.EndUpdate();

            // Registar available shutter mode to dropdown list
            this._shutterModeCb.Items.Clear();
            this._shutterModeCb.BeginUpdate();
            foreach (var ms in episcan.Sensor.AvailableShutterModes) this._shutterModeCb.Items.Add(ms);
            this._shutterModeCb.EndUpdate();

            // Registar available trigger mode to dropdown list
            this._triggerModeCb.Items.Clear();
            this._triggerModeCb.BeginUpdate();
            foreach (var tm in episcan.Sensor.AvailableTriggerModes) this._triggerModeCb.Items.Add(tm);
            this._triggerModeCb.EndUpdate();

        }
        void UpdateUIfromCamera()
        {
            try
            {
                this._gainBoostCb.Checked = episcan.Sensor.EnableGainBoost;
                this._masterGainNud.Value = episcan.Sensor.MasterGain;

                this._syncDelayNud.ValueChanged -= SyncDelayExpNudOnValueChanged;
                
                this._syncDelayNud.Value = (int)Math.Round(episcan.Sensor.Delay + 0.5f * episcan.Sensor.Exposure * 1e3f);
                this._syncDelayNud.ValueChanged += SyncDelayExpNudOnValueChanged;
                this._syncExpNud.Value = (decimal)episcan.Sensor.Exposure;
                Console.WriteLine("{0},{1}","Exposure",episcan.Sensor.Exposure.ToString());
                this._pixelClockCb.SelectedIndex = this._pixelClockCb.FindString(episcan.Sensor.PixelClock.ToString());
                this._shutterModeCb.SelectedIndex = this._shutterModeCb.FindString(episcan.Sensor.ShutterMode.ToString());
                this._triggerModeCb.SelectedIndex = this._triggerModeCb.FindString(episcan.Sensor.TrigerMode.ToString());

                _camWidthNud.ValueChanged -= CameraAOIOnValueChanged;
                _camHeightNud.ValueChanged -= CameraAOIOnValueChanged;
                _camLeftNud.ValueChanged -= CameraAOIOnValueChanged;
                _camTopNud.ValueChanged -= CameraAOIOnValueChanged;

                this._camWidthNud.Value = episcan.Sensor.Width;
                this._camHeightNud.Value = episcan.Sensor.Height;
                this._camLeftNud.Value = episcan.Sensor.Left;
                this._camTopNud.Value = episcan.Sensor.Top;

                _camWidthNud.ValueChanged += CameraAOIOnValueChanged;
                _camHeightNud.ValueChanged += CameraAOIOnValueChanged;
                _camLeftNud.ValueChanged += CameraAOIOnValueChanged;
                _camTopNud.ValueChanged += CameraAOIOnValueChanged;


                this._cmodeCb.SelectedIndex = this._cmodeCb.FindString(episcan.Sensor.PixelFormat.ToString());

                this._delayOffsetNud.Value = episcan.Sensor.DelayOffset;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void ExitBtnOnClick(object sender, EventArgs e)
        {
            if(episcan.Sensor != null) episcan.Sensor.Close();
            Close();
        }

        private void GainBoostCbOnCheckedChanged(object sender, EventArgs e)
        {
            episcan.Sensor.EnableGainBoost = ((CheckBox)sender).Checked;
        }

        private void MasterGainOnValueChanged(object sender, EventArgs e)
        {
            episcan.Sensor.MasterGain = (int)((NumericUpDown)sender).Value;
        }


        private void PixelClockCbOnSelectedIndexChanged(object sender, EventArgs e)
        {
            episcan.Sensor.PixelClock = int.Parse(((ComboBox)sender).SelectedItem.ToString());
        }
        

        private void ToggleProjectorPower(string port)
        {
            _projectorPowerBtn.Invoke(new Action(() => _projectorPowerBtn.Enabled = false));
            
            try
            {
                SyncBoard sb = new SyncBoard(port);

                sb.Open();
                sb.TogglePower();

                sb.ToggleProjectorMode();
                sb.Close();
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                _projectorPowerBtn.Invoke(new Action(() => _projectorPowerBtn.Enabled = true));
            }
        }
        private async void ProjectorPowerToggleBtnOnClick(object sender, EventArgs e)
        {
            try
            {
                string port = this.serialsCb.SelectedItem.ToString();
                await Task.Run(() => ToggleProjectorPower(port));
            }
            catch(Exception ex)
            {
                this._toolStripStatusLabel1.Text = ex.Message;
            }
        }

        private void CameraAOIOnValueChanged(object sender, EventArgs e)
        {
            if (sender.GetType() != typeof(NumericUpDown)) return;
            episcan.Sensor.AOI = new Rectangle(
                (int)_camLeftNud.Value, (int)_camTopNud.Value,
                (int)_camWidthNud.Value, (int)_camHeightNud.Value);
            camView.UpdateSize();
            camView_h.UpdateSize();
        }

        private void CamWidthNudOnValueChanged(object sender, EventArgs e)
        {
            episcan.Sensor.Width = (int)((NumericUpDown)sender).Value;
        }

        private void CamHeightNud_ValueChanged(object sender, EventArgs e)
        {
            episcan.Sensor.Height = (int)((NumericUpDown)sender).Value;
        }


        private async void SingleCapBtnOnClick(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Png file (*.png)|*.png|All files (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {

                var img = await episcan.CaptureAverage();

                string fname = sfd.FileName;

                Cv2.ImWrite(fname, img);
                
                await Task.Delay(100);
                episcan.Sensor.StartCapture();
            }
        }

        private async void DoubleCapBtnOnClick(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Png file (*.png)|*.png|All files (*.*)|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {



                string fname = sfd.FileName;

                string[] fname_s = fname.Split('.');

                string fname_h = fname_s[0] + "_h.png";

                string fname_black = fname_s[0] + "_black.png";

                string fname_black_h = fname_s[0] + "_black_h.png";

                episcan.Screen.BackColor = System.Drawing.Color.Black;
                await Task.Delay(100);

                var black = await episcan.CaptureAverage();
                var black_h = await helpercam.CaptureAverage();

                episcan.Screen.BackColor = System.Drawing.Color.White;
                await Task.Delay(100);

                var img = await episcan.CaptureAverage();

                var img_h = await helpercam.CaptureAverage();

                Cv2.ImWrite(fname_black, black);
                Cv2.ImWrite(fname_black_h, black_h);
                Cv2.ImWrite(fname, img);
                Cv2.ImWrite(fname_h, img_h);

                await Task.Delay(100);
                episcan.Sensor.StartCapture();
                helpercam.Sensor.StartCapture();
            }
        }


        private void ScalerNudOnValueChanged(object sender, EventArgs e)
        {
            episcan.Sensor.Scaler = (float)((NumericUpDown)sender).Value;

        }


        private void TriggerModeCbOnSelectedIndexChanged(object sender, EventArgs e)
        {
            episcan.Sensor.TrigerMode = (EpiscanUtil.MySensor.TriggerModeList)Enum.Parse(typeof(EpiscanUtil.MySensor.TriggerModeList), ((ComboBox)sender).SelectedItem.ToString());
        }

        private void RestartBtnOnClick(object sender, EventArgs e)
        {
            episcan.Sensor.StartCapture();
        }

        private void UpdateDispString()
        {

            if (this._regularModeRb.Checked)
            {
                episcan.Sensor.DispText = "Regular";
            }
            else if (this._directModeRb.Checked)
            {
                episcan.Sensor.DispText = "Direct";
            }
            else if (this._indirectModeRb.Checked)
            {
                episcan.Sensor.DispText = "Indirect";
            }
            else
            {
                episcan.Sensor.DispText = "delay =   " + ((float)this._syncDelayNud.Value).ToString("0") + "us\n" + "exposure = " + (((float)this._syncExpNud.Value) * 1e3f).ToString("0") + "us";
            }

        }

        private void ConnectBtnOnClick(object sender, EventArgs e)
        {
            try
            {
                int device_id = int.Parse(this._cameraCb.SelectedItem.ToString().Split(new char[] { '(', ')' })[1]);

                this._preset = CameraParameter.FromXml("../../EpiscanScriptDemo/cp_color.xml");

                camView = new CamView();

                episcan.InitializeSensor(camView.Handle, device_id);

                camView.Sensor = episcan.Sensor;

                
                camView.Show();

                camView.Width = (int)(0.5 * episcan.Sensor.Width);
                camView.Height = (int)(0.5 * episcan.Sensor.Height);

                InitUIfromCamera();
                InitUI();

                //UpdateUIfromCamera();

                this._cameraGb.Enabled = true;
                this._episcanGb.Enabled = true;

                // apply camera parameters
                try
                {
                    ApplyCameraParameters(this._preset);
                }
                catch(Exception ex)
                {
                    throw ex;
                }

                // update UI
                UpdateUIfromCamera();

                UpdateDelayExposure();

                UpdateDispString();

                episcan.Sensor.StartCapture();
                episcan.Sensor.OnSensorParameterChanged = UpdateUIfromCamera;
                camView.UpdateSize();
                episcan.Sensor.AutoWhitebalanceOn();
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        private void HelperBtnOnClick(object sender, EventArgs e)
        {
            try
            {
                int device_id_sub = int.Parse(this._cameraCb.SelectedItem.ToString().Split(new char[] { '(', ')' })[1]);

                if (device_id_sub == 1) { device_id_sub = 2; }
                else if (device_id_sub == 2) { device_id_sub = 1; }


                this._preset_h = CameraParameter.FromXml("../../EpiscanScriptDemo/cp_sub_color.xml");

                camView_h = new CamView_h();

                helpercam.InitializeSensor(camView_h.Handle, device_id_sub);
                //helpercam.ImageRoi = Roi;
                //Roi = episcan.ImageRoi;
                //SetUIImageRoi();

                camView_h.Sensor = helpercam.Sensor;


                camView_h.Show();

                camView_h.Width = (int)(0.625 * helpercam.Sensor.Width);
                camView_h.Height = (int)(0.625 * helpercam.Sensor.Height);

                

                // InitUIfromCamera();
                // InitUI();

                //UpdateUIfromCamera();

                this._cameraGb.Enabled = true;
                this._episcanGb.Enabled = true;

                try
                {
                    ApplyCameraParameters_h(this._preset_h);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                
                // update UI
                //UpdateUIfromCamera();

                //UpdateDelayExposure();

                //UpdateDispString();

                helpercam.Sensor.StartCapture();
                camView_h.UpdateSize();
                helpercam.Sensor.AutoWhitebalanceOn();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        private void ApplyCameraParameters(CameraParameter param)
        {
            //episcan.Sensor.Delay = param.Delay;
            episcan.Sensor.DelayOffset = param.DelayOffset;
            episcan.Sensor.PixelClock = param.PixelClock;
            episcan.Sensor.Exposure = param.Exposure;
            episcan.Sensor.ShutterMode = param.ShutterMode;
            episcan.Sensor.TrigerMode = param.TriggerMode;
            episcan.Sensor.AOI = new Rectangle(
                param.Left, param.Top, param.Width, param.Height);
            episcan.Sensor.Width = param.Width;
            episcan.Sensor.Height = param.Height;
            //episcan.Sensor.Left = param.Left;
            //episcan.Sensor.Top = param.Top;
            episcan.Sensor.EnableGainBoost = param.EnableGainBoost;
            episcan.Sensor.MasterGain = param.MasterGain;
            episcan.Sensor.Scaler = param.Scaler;
            episcan.Sensor.PixelFormat = param.PixelFormat;
        }


        private void ApplyCameraParameters_h(CameraParameter param)
        {
            //episcan.Sensor.Delay = param.Delay;
            //helpercam.Sensor.DelayOffset = param.DelayOffset;
            helpercam.Sensor.PixelClock = param.PixelClock;
            helpercam.Sensor.FrameRate = param.FrameRate;
            helpercam.Sensor.Exposure = param.Exposure;
            helpercam.Sensor.ShutterMode = param.ShutterMode_h;
            //helpercam.Sensor.TrigerMode = param.TriggerMode_h;
            helpercam.Sensor.AOI = new Rectangle(
                param.Left, param.Top, param.Width, param.Height);
            helpercam.Sensor.Width = param.Width;
            helpercam.Sensor.Height = param.Height;
            helpercam.Sensor.EnableGainBoost = param.EnableGainBoost;
            helpercam.Sensor.MasterGain = param.MasterGain;
            helpercam.Sensor.Scaler = param.Scaler;
            helpercam.Sensor.PixelFormat = param.PixelFormat_h;
        }


        void UpdateDelayExposure()
        {
            if (this._directModeRb.Checked)
            {
                float exposure = 0.5f; // 0.5[ms]
                episcan.Sensor.ShutterMode = EpiscanUtil.MySensor.ShutterModeList.Rolling;
                episcan.Sensor.Delay = (int)(-0.5f * exposure * 1e3f);
                //episcan.Sensor.PixelClock = (int)(68);
                episcan.Sensor.Exposure = exposure;

                //
                this._syncDelayNud.Value = (decimal)0.0;
                this._syncExpNud.Value = (decimal)exposure;
                this._syncDelayNud.Enabled = false;
                this._syncExpNud.Enabled = false;
            }
            else if (this._indirectModeRb.Checked)
            {
                float exposure = 0.5f; // 0.5[ms]
                float gap = 0.3f;

                episcan.Sensor.ShutterMode = EpiscanUtil.MySensor.ShutterModeList.Rolling;
                episcan.Sensor.Delay = (int)((2.0f * gap) * 1e3f * exposure);
                episcan.Sensor.Exposure = 16.67f - exposure - 2.0f * gap;
                //episcan.Sensor.PixelClock = (int)(68);
                //
                this._syncDelayNud.Value = (decimal)0.0;
                this._syncExpNud.Value = (decimal)episcan.Sensor.Exposure;
                this._syncDelayNud.Enabled = false;
                this._syncExpNud.Enabled = false;
            }
            else if (this._regularModeRb.Checked)
            {
                float gap = 0.3f;
                episcan.Sensor.ShutterMode = EpiscanUtil.MySensor.ShutterModeList.Global;
                episcan.Sensor.Delay = -(int)this._delayOffsetNud.Value;
                episcan.Sensor.Exposure = 16.67f - gap;

                //
                this._syncDelayNud.Value = (decimal)0.0;
                this._syncExpNud.Value = (decimal)episcan.Sensor.Exposure;
                this._syncDelayNud.Enabled = false;
                this._syncExpNud.Enabled = false;
            }
            else if (this._veinRb.Checked)
            {
                episcan.Sensor.ShutterMode = EpiscanUtil.MySensor.ShutterModeList.Rolling;

                float delay = 600f;
                float exposure = 0.5f;

                episcan.Sensor.Delay = (int)(delay - 0.5f * exposure * 1e3f);
                episcan.Sensor.Exposure = exposure;

                this._syncDelayNud.Value = (decimal)delay;
                this._syncExpNud.Value = (decimal)exposure;

                //this._masterGainNud.Value = 100;
            }
            else if (this._deskModeRb.Checked)
            {
                episcan.Sensor.ShutterMode = EpiscanUtil.MySensor.ShutterModeList.Rolling;

                float delay = 6000f;
                float exposure = 0.5f;
                int pc = 60;
                int gain = 100;

                episcan.Sensor.Delay = (int)(delay - 0.5f * exposure * 1e3f);
                episcan.Sensor.Exposure = exposure;
                episcan.Sensor.PixelClock = pc;
                episcan.Sensor.MasterGain = gain;

                this._syncDelayNud.Value = (decimal)delay;
                this._syncExpNud.Value = (decimal)exposure;

                episcan.Sensor.AutoWhitebalanceOn();
            }
            else if (this._excdeskModeRb.Checked)
            {
                episcan.Sensor.ShutterMode = EpiscanUtil.MySensor.ShutterModeList.Rolling;

                float delay = 7800f; 
                float exposure = 3.0f;
                int pc = 60;
                int gain = 0;
                int scaler = 150;

                episcan.Sensor.Delay = (int)(delay - 0.5f * exposure * 1e3f);
                episcan.Sensor.Exposure = exposure;
                episcan.Sensor.PixelClock = pc;
                episcan.Sensor.MasterGain = gain;
                episcan.Sensor.Scaler = scaler;

                this._syncDelayNud.Value = (decimal)delay;
                this._syncExpNud.Value = (decimal)exposure;

                episcan.Sensor.AutoWhitebalanceOn();
            }
            else if (this._manualModeRb.Checked)
            {
                episcan.Sensor.ShutterMode = EpiscanUtil.MySensor.ShutterModeList.Rolling;
                float exposure = (float)this._syncExpNud.Value;

                try
                {
                    episcan.Sensor.Delay = (int)((float)this._syncDelayNud.Value - 0.5f * exposure * 1e3f);
                    episcan.Sensor.Exposure = exposure;
                }
                catch (Exception)
                {
                    this._toolStripStatusLabel1.Text = "failed to set delay/exposure";

                    this._syncDelayNud.Value = 0;
                    this._syncExpNud.Value = (decimal)0.5;
                    UpdateDelayExposure();
                }



                //
                this._syncDelayNud.Enabled = true;
                this._syncExpNud.Enabled = true;
            }

            else if (this._delayModeRb.Checked)
            {
                episcan.Sensor.ShutterMode = EpiscanUtil.MySensor.ShutterModeList.Rolling;
                float exposure = 0.5f;
                int pc = 60;
                int gain = 100;
                this._toolStripStatusLabel1.Text = "Delay:";
                for (float delayvalue = 5000f; delayvalue <= 9000f; delayvalue += 10f)
                {
                    episcan.Sensor.Exposure = exposure;
                    //Console.WriteLine("{0},{1}", "Synchronisation Delay", episcan.Sensor.Delay.ToString());
                    //Console.WriteLine((int)(delayvalue - 0.5f * exposure * 1e3f));
                    this._toolStripStatusLabel1.Text = delayvalue.ToString();

                    episcan.Sensor.Delay = (int)(delayvalue - 0.5f * exposure * 1e3f);
                    episcan.Sensor.PixelClock = pc;
                    Console.WriteLine("{0},{1}", "Synchronisation Delay", episcan.Sensor.Delay.ToString());


                }
            }
            else if (this._trackModeRb.Checked)
            {
                episcan.Sensor.ShutterMode = EpiscanUtil.MySensor.ShutterModeList.Rolling;

                float delay = 7800f;
                float exposure = 3.0f;
                int pc = 60;
                int gain = 0;
                int scaler = 150;

                episcan.Sensor.Delay = (int)(delay - 0.5f * exposure * 1e3f);
                episcan.Sensor.Exposure = exposure;
                episcan.Sensor.PixelClock = pc;
                episcan.Sensor.MasterGain = gain;
                episcan.Sensor.Scaler = scaler;

                this._syncDelayNud.Value = (decimal)delay;
                this._syncExpNud.Value = (decimal)exposure;

                episcan.Sensor.AutoWhitebalanceOn();
                //var captured = await episcan.CaptureAverage(AverageNum);

                VideoCapture capture = new VideoCapture(0);
                using (Window window = new Window("Camera"))
                using (Mat image = new Mat()) // Frame image buffer
                {
                    // When the movie playback reaches end, Mat.data becomes NULL.
                    while (true)
                    {
                        capture.Read(image); // same as cvQueryFrame
                        if (image.Empty()) break;
                        window.ShowImage(image);
                        Cv2.WaitKey(30);
                    }
                }
            }

        }

        private void ImagingModeRbOnCheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Focused)
            {
                UpdateDelayExposure();

                UpdateDispString();

                UpdateUIfromCamera();
            }


        }

        private void SyncDelayExpNudOnValueChanged(object sender, EventArgs e)
        {
            if (this._syncDelayNud.Focused || this._syncExpNud.Focused)
            {
                float syncDelay = (float)this._syncDelayNud.Value;
                float syncExposure = (float)this._syncExpNud.Value;

                episcan.Sensor.Delay = (int)(syncDelay - 0.5f * syncExposure * 1e3f);
                episcan.Sensor.Exposure = syncExposure;

                this._manualModeRb.Checked = true;

                UpdateDispString();
                UpdateUIfromCamera();
            }
        }


        private void DelayOffsetNudOnValueChanged(object sender, EventArgs e)
        {
            episcan.Sensor.DelayOffset = (int)this._delayOffsetNud.Value;
        }

        private void ShowProjectorScreenBtnOnClick(object sender, EventArgs e)
        {
            //if (episcan.Screen != null) episcan.Screen.Dispose();

            //int index = this._screenCb.SelectedIndex;
            //if (index == -1) return;

            //episcan.Screen = new Form();

            //episcan.Screen.Show();

            //Screen scr = Screen.AllScreens[index];

            //episcan.Screen.Location = new Point(scr.WorkingArea.X, scr.WorkingArea.Y);
            //episcan.Screen.FormBorderStyle = FormBorderStyle.None;
            //episcan.Screen.WindowState = FormWindowState.Maximized;


            switch(this._displayColorCb.SelectedItem.ToString())
            {
                case "White":
                    episcan.Screen.BackColor = Color.White;
                    break;
                case "Black":
                    episcan.Screen.BackColor = Color.Black;
                    break;
                case "Red":
                    episcan.Screen.BackColor = Color.FromArgb(255, 0, 0);
                    break;
                case "Green":
                    episcan.Screen.BackColor = Color.FromArgb(0, 255, 0);
                    break;
                case "Blue":
                    episcan.Screen.BackColor = Color.FromArgb(0, 0, 255);
                    break;
                
            }


        }

        private void CloseProjectorScreenBtnOnClick(object sender, EventArgs e)
        {
            if (episcan.Screen != null) episcan.Screen.Close();
        }

        private void RefreshScreenBtnOnClick(object sender, EventArgs e)
        {
            InitUIScreen();
        }
        private void ResetBtnOnClick(object sender, EventArgs e)
        {
            this._syncDelayNud.Value = 0;
            this._syncExpNud.Value = (decimal)0.5;

            UpdateDelayExposure();
            UpdateDispString();
            UpdateUIfromCamera();
            UpdateDispString();
        }

        private void ColorModeCbOnSelectedIndexCommitted(object sender, EventArgs e)
        {
            episcan.Sensor.PixelFormat = (EpiscanUtil.MySensor.PixelFormatList)Enum.Parse(typeof(EpiscanUtil.MySensor.PixelFormatList), ((ComboBox)sender).SelectedItem.ToString());
        }


        private void ShutterModeCbOnSelectionChangeCommitted(object sender, EventArgs e)
        {
            episcan.Sensor.ShutterMode = (EpiscanUtil.MySensor.ShutterModeList)Enum.Parse(typeof(EpiscanUtil.MySensor.ShutterModeList), ((ComboBox)sender).SelectedItem.ToString());
        }

        private void AutoWbBtnOnClick(object sender, EventArgs e)
        {
            episcan.Sensor.AutoWhitebalanceOn();
        }

        //
        // Modified by Iwaguchi
        //

        private bool CheckConnection()
        {
            if (episcan.Sensor != null) return true;
            MessageBox.Show("Camera is not connected");
            return false;
        }

        

        public void DetectSyncBoard()
        {
            var portFound = SyncBoard.FindSyncboard();
            if (portFound == "NOT_FOUND")
            {
                _toolStripStatusLabel1.Text = "Sync board not found";
                serialsCb.Enabled = true;
                return;
            }
            else
            {
                _toolStripStatusLabel1.Text = $"Sync board found at {portFound}";
            }

            var cbIdx = serialsCb.Items.IndexOf(portFound);
            if (cbIdx != -1)
            {
                serialsCb.SelectedIndex = cbIdx;
                serialsCb.Enabled = false;
            }
            else
            {
                serialsCb.Enabled = true;
            }
        }

        private void _projectorDetectBtn_Click(object sender, EventArgs e)
        {
            DetectSyncBoard();
        }

        dynamic currentScript = null;
        string[] scriptFiles = null;
        private void InitUIScript()
        {
            var scriptDir = @"scripts";
            scriptFiles = Directory.GetFiles(scriptDir, "*.cs")
                .Where(p => !Path.GetFileName(p).StartsWith("_"))
                .ToArray();
            ScriptListBox.Items.Clear();
            foreach (var scriptFile in scriptFiles)
            {
                ScriptListBox.Items.Add(scriptFile.Replace($@"{scriptDir}\", ""));
            }
        }
        
        private void ScriptListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ScriptListBox.SelectedIndex == -1) return;

            ScriptRunButton.Enabled = false;

            currentScript = ScriptLoader.Load(scriptFiles[ScriptListBox.SelectedIndex]);
            currentScript.Configure(episcan);

            PropertyInfo[] props = currentScript.GetType().GetProperties();
            int count = 0;
            ScriptOptionContainer.Controls.Clear();
            foreach (var prop in props)
            {
                if (prop.PropertyType == typeof(int) ||
                    prop.PropertyType == typeof(float) ||
                    prop.PropertyType == typeof(double))
                {
                    ScriptOptionContainer.Controls.Add(new Label
                    {
                        Size = new Size(77, 12),
                        Text = prop.Name,
                        Location = new Point(0, 2 + 26 * count),
                    });
                    var numeric = new NumericUpDown
                    {
                        Size = new Size(114, 19),
                        Name = prop.Name,
                        Location = new Point(80, 0 + 26 * count),
                        Maximum = Decimal.MaxValue,
                        Minimum = Decimal.MinValue,
                        Value = Decimal.Parse(prop.GetValue(currentScript).ToString()),
                        DecimalPlaces = prop.PropertyType == typeof(int) ? 0 : 3
                    };
                    numeric.ValueChanged += (object _sender, EventArgs _e) =>
                    {
                        if (prop.PropertyType == typeof(int))
                            prop.SetValue(currentScript, (int)numeric.Value);
                        else if (prop.PropertyType == typeof(float))
                            prop.SetValue(currentScript, (float)numeric.Value);
                        else if (prop.PropertyType == typeof(double))
                            prop.SetValue(currentScript, (double)numeric.Value);
                    };
                    ScriptOptionContainer.Controls.Add(numeric);
                }
                else if (prop.PropertyType == typeof(string))
                {
                    ScriptOptionContainer.Controls.Add(new Label
                    {
                        Size = new Size(77, 12),
                        Text = prop.Name,
                        Location = new Point(0, 2 + 26 * count),
                    });
                    var textbox = new TextBox
                    {
                        Size = new Size(114, 19),
                        Name = prop.Name,
                        Location = new Point(80, 0 + 26 * count),
                        Text = prop.GetValue(currentScript),
                        Multiline = false
                    };
                    textbox.TextChanged += (object _sender, EventArgs _e)
                        => prop.SetValue(currentScript, textbox.Text);
                    ScriptOptionContainer.Controls.Add(textbox);
                }
                count++;
            }

            ScriptRunButton.Enabled = true;
        }

        private async void ScriptRunButton_Click(object sender, EventArgs e)
        {
            currentScript.Configure(episcan);
            await currentScript?.Run(episcan);
            if (_autoTurnOffBox.Checked)
                ProjectorPowerToggleBtnOnClick(null, null);
        }

        private void _screenCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            episcan.Screen.ScreenIndex = _screenCb.SelectedIndex;
        }

        private string[] presetFiles;
        private void UpdateCameraProfileList()
        {
            PresetListCombo.Items.Clear();

            var profileDir = @"presets";
            presetFiles = Directory.GetFiles(profileDir, "*.xml");

            foreach (var profFile in presetFiles)
            {
                PresetListCombo.Items.Add(profFile.Replace($@"{profileDir}\", ""));
            }

            PresetListCombo.SelectedIndex = presetFiles.ToList()
                .FindIndex(s => s == Properties.Settings.Default.LastSelectedPreset);

        }

        private void PresetListCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            var presetFile = presetFiles[PresetListCombo.SelectedIndex];
            Properties.Settings.Default.LastSelectedPreset = presetFile;
            Properties.Settings.Default.Save();

            _preset = CameraParameter.FromXml(presetFile);
            if (episcan.Sensor == null) return;
            try
            {
                ApplyCameraParameters(_preset);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }


        public CameraParameter GenerateFromCurrentParameters()
        {
            var parameters = new CameraParameter()
            {
                DelayOffset = (int)_delayOffsetNud.Value,
                Exposure = (float)_syncExpNud.Value,
                PixelClock = int.Parse((string)_pixelClockCb.Items[_pixelClockCb.SelectedIndex]),
                ShutterMode = (MySensor.ShutterModeList)Enum.Parse(
                    typeof(MySensor.ShutterModeList),
                    _shutterModeCb.Items[_shutterModeCb.SelectedIndex].ToString()),
                TriggerMode = (MySensor.TriggerModeList)Enum.Parse(
                    typeof(MySensor.TriggerModeList),
                    _triggerModeCb.Items[_triggerModeCb.SelectedIndex].ToString()),
                Width = (int)_camWidthNud.Value,
                Height = (int)_camHeightNud.Value,
                Left = (int)_camLeftNud.Value,
                Top = (int)_camTopNud.Value,
                EnableGainBoost = _gainBoostCb.Checked,
                MasterGain = (int)_masterGainNud.Value,
                Scaler = (int)_scalerNud.Value,
                PixelFormat = (MySensor.PixelFormatList)Enum.Parse(
                    typeof(MySensor.PixelFormatList),
                    _cmodeCb.Items[_cmodeCb.SelectedIndex].ToString())
            };
            return parameters;
        }

        private void ProfileSaveButton_Click(object sender, EventArgs e)
        {
            var profileDir = @"presets";
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "XML file (*.xml)|*.xml|All files (*.*)|*.*",
                InitialDirectory = Directory.GetCurrentDirectory() + "\\" + profileDir
            };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                GenerateFromCurrentParameters().SaveAsXml(sfd.FileName);
            }
        }

        private void _episcanGb_Enter(object sender, EventArgs e)
        {

        }
    }
}
*/