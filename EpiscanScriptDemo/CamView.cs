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
    public partial class CamView : Form
    {
        public EpiscanUtil.MySensor Sensor = null;
        Size cachedSize = new Size();

        public CamView()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.BackgroundImageLayout = ImageLayout.Zoom;
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            this.Sensor.ExitCamera();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            this.Sensor.ExitCamera();
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            UpdateSize();
        }

        public void UpdateSize()
        {
            if (Sensor == null) return;

            var ratio = Sensor.Height / (float)Sensor.Width;

            if (cachedSize.Width != Size.Width)
                ClientSize = new Size(ClientSize.Width, (int)(ratio * ClientSize.Width));
            else
                ClientSize = new Size((int)(ClientSize.Height / ratio), ClientSize.Height);
            cachedSize = ClientSize;
        }

    }
}
