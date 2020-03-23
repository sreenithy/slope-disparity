using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace EpiscanUtil
{
    public class MyEpiscan
    {
        public MySensor Sensor { private set; get; }
        public MyScreen Screen
        {
            private set => screen = value;
            get
            {
                screen = screen ?? new MyScreen();
                return screen;
            }
        }
        public SyncBoard SyncBoard { private set; get; }
        //public int[] ImageRoi { set; get; }

        private MyScreen screen;

        public void InitializeSensor(IntPtr handle, int deviceId)
        {
            Sensor = new MySensor(handle, deviceId);
        }

        public void SetDelayExposure(float delay, float exposure)
        {
            try
            {
                Sensor.Delay = (int)(delay - 0.5f * exposure * 1e3f);
                Sensor.Exposure = exposure;
            }
            catch (Exception)
            {
                throw new Exception("failed to set delay / exposure.");
            }
        }

        public void SetDelayExposurePC(float delay, float exposure, float pixelclock)
        {
            try
            {
                Sensor.Delay = (int)(delay - 0.5f * exposure * 1e3f);
                Sensor.Exposure = exposure;
                Sensor.PixelClock = (int)pixelclock;
            }
            catch (Exception)
            {
                throw new Exception("failed to set delay / exposure.");
            }
        }


        public async Task<Mat> CaptureAverage(int averageNum = 1)
        {
            int height = Sensor.Height, width = Sensor.Width;

            //Inverted the ordering to capture Mono image
            var sum = new Mat(height, width, Sensor.IsMono ? MatType.CV_32SC1 : MatType.CV_32SC3);
            //var sum = new Mat(height, width, Sensor.IsMono ? MatType.CV_32SC3 : MatType.CV_32SC1);
            Console.WriteLine(Sensor);
            var floatImage = new Mat();
            MatType type = MatType.CV_8UC1;
            for (int i = 0; i < averageNum; i++)
            {
                await Task.Delay(1);
                var frame = Sensor.CaptureFrame();
                type = frame.Type();
                frame.ConvertTo(floatImage, Sensor.IsMono ? MatType.CV_32SC1 : MatType.CV_32SC3);
                //frame.ConvertTo(floatImage, Sensor.IsMono ? MatType.CV_32SC3 : MatType.CV_32SC1);
                sum += floatImage;
            }
            sum = sum / (float)averageNum;
            var averaged = new Mat();
            sum.ConvertTo(averaged, type);

            return averaged;
        }
        
    }
}
