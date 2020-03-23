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

public class Class1Script : IEpiscanScript
{
    public float Delay { set; get; } = 7037f;
    public string OutputDir { set; get; } = "data";
    public float Exposure { set; get; } = 4.5f;
    public float pixelclock { set; get; } = 0;
    public int AverageNum { set; get; } = 4;


    public void Configure(MyEpiscan episcan)
    {
        Debug.Write("hello");
    }

    public async Task Run(MyEpiscan episcan)
    {
        if (!Directory.Exists(OutputDir)) Directory.CreateDirectory(OutputDir);
        for (int i = 50; i < 100; i++)
        {
            pixelclock = i;

            episcan.SetDelayExposurePC(Delay, Exposure, pixelclock);
            // Switch to regular mode
            episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Rolling;

            episcan.SetDelayExposure(Delay, Exposure);
            
            Console.WriteLine("Details");
            Console.WriteLine(episcan.Sensor);


            await Task.Delay(80);
            //episcan.Sensor.StartCapture();
            var capture = await episcan.CaptureAverage(AverageNum);
            //var filename = Path.GetFileName(patternFiles[i]);
            Cv2.ImWrite($"{OutputDir}\\"+i.ToString()+".png", capture);


        }
    }
}

/*
using System;
using System.Diagnostics;
using UserScriptHost;
using System.Collections.Generic;
using System.Linq;
using EpiscanUtil;
using OpenCvSharp;
using System.Threading.Tasks;
using System.IO;

public class Class1 : IEpiscanScript
{
    public float Delay { set; get; } = 0;
    public float Exposure { set; get; } = 0.5f;
    public float pixelclock { set; get; } = 0;

    public void Configure(MyEpiscan episcan)
    {
        Debug.Write("hello");
    }

    public async Task Run(MyEpiscan episcan)
    {   for (int i = 50; i < 100; i=i+10)
        {
            pixelclock = i;

            episcan.SetDelayExposurePC(Delay, Exposure, pixelclock);
            // Switch to regular mode
            episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Rolling;

            episcan.SetDelayExposure(Delay, Exposure);
            await Task.Delay(100);

            await Task.Delay(100);
            episcan.Sensor.StartCapture();

            Cv2.WaitKey();

        }
         


    }
}
*/
