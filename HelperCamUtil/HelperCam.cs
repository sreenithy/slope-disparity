using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace HelperCamUtil
{
    public class HelperCam
    {
        public HelperSensor Sensor { private set; get; }
        
        //public int[] ImageRoi { set; get; }


        public void InitializeSensor(IntPtr handle, int deviceId)
        {
            Sensor = new HelperSensor(handle, deviceId);
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

        public async Task<Mat> CaptureAverage(int averageNum = 4)
        {
            int height = Sensor.Height, width = Sensor.Width;

            var sum = new Mat(height, width, Sensor.IsMono ? MatType.CV_32SC1 : MatType.CV_32SC3);
            var floatImage = new Mat();
            MatType type = MatType.CV_8UC1;
            for (int i = 0; i < averageNum; i++)
            {
                await Task.Delay(1);
                var frame = Sensor.CaptureFrame();
                type = frame.Type();
                frame.ConvertTo(floatImage, Sensor.IsMono ? MatType.CV_32SC1 : MatType.CV_32SC3);
                sum += floatImage;
            }
            sum = sum / (float)averageNum;
            var averaged = new Mat();
            sum.ConvertTo(averaged, type);

            return averaged;
        }
        
    }
}
