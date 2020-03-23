using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenCvSharp.Extensions;
using Mat = OpenCvSharp.Mat;

namespace EpiscanUtil
{
    public class MyScreen
    {
        private Form screen;
        public int ScreenIndex {
            set
            {
                screenIndex = value;
                Create(screenIndex);
            }
            get => screenIndex;
        }
        public Size Size
        {
            get => screen.Size;
        }

        private int screenIndex;
        

        public Color BackColor
        {
            set
            {
                screen.BackColor = value;
                screen.Show();
                Task.Delay(100);
            }
            get => screen.BackColor;
        }

        public Image BackgroundImage
        {
            set
            {
                screen.BackgroundImage = value;
                screen.Show();
                Task.Delay(100);
            }
            get => screen.BackgroundImage;
        }

        public Mat BackgroundMat
        {
            set
            {
                if (value != null)
                {
                    var bmp = BitmapConverter.ToBitmap(value);
                    screen.BackgroundImage = bmp;
                }
                else
                {
                    screen.BackgroundImage = null;
                }
                Task.Delay(100);
                screen.Show();
            }
        }

        public void Create(int screenIndex)
        {
            this.screenIndex = screenIndex;

            screen = screen ?? new Form();
            screen.Show();

            Screen scr = Screen.AllScreens[ScreenIndex];

            screen.Location = new Point(scr.WorkingArea.X, scr.WorkingArea.Y);
            screen.FormBorderStyle = FormBorderStyle.None;
            screen.WindowState = FormWindowState.Maximized;
            screen.Hide();
        }

        public void Close()
        {
            if (screen == null) return;
            screen.Close();
            screen.Dispose();
            screen = null;
        }
    }
}
