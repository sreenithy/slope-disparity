using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EpiscanScriptDemo
{
    public partial class CamView_h : Form
    {
        //public HelperCamUtil.HelperSensor Sensor = null;
        //Size cachedSize = new Size();

        //public CamView_h()
        //{
        //    InitializeComponent();

        //    this.DoubleBuffered = true;
        //    this.BackgroundImageLayout = ImageLayout.Zoom;
        //}

        //private void OnFormClosed(object sender, FormClosedEventArgs e)
        //{
        //    this.Sensor.ExitCamera();
        //}

        //private void OnFormClosing(object sender, FormClosingEventArgs e)
        //{
        //    this.Sensor.ExitCamera();
        //}

        //protected override void OnResizeEnd(EventArgs e)
        //{
        //    base.OnResizeEnd(e);
        //    UpdateSize();
        //}

        //public void UpdateSize()
        //{
        //    if (Sensor == null) return;

        //    var ratio = Sensor.Height / (float)Sensor.Width;

        //    if (cachedSize.Width != Size.Width)
        //        ClientSize = new Size(ClientSize.Width, (int)(ratio * ClientSize.Width));
        //    else
        //        ClientSize = new Size((int)(ClientSize.Height / ratio), ClientSize.Height);
        //    cachedSize = ClientSize;
        //}

        //private void InitializeComponent()
        //{
        //    this.SuspendLayout();
        //    // 
        //    // CamView_h
        //    // 
        //    this.ClientSize = new System.Drawing.Size(359, 286);
        //    this.Name = "CamView_h";
        //    this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnFormClosing);
        //    this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
        //    this.ResumeLayout(false);

        //}
    }
}
