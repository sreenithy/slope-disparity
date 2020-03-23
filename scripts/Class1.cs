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
    public int DelayMin { set; get; } = -1000;
    public int DelayMax { set; get; } = 1000;
    public int Shift { set; get; } = 25;
    public string OutputDir { set; get; } = "data";
    public float Exposure { set; get; } = 8.748f;
    public int AverageNum { set; get; } = 4;


    public void Configure(MyEpiscan episcan)
    {
        Debug.Write("hello");
    }

    public async Task Run(MyEpiscan episcan)
    {
        if (!Directory.Exists(OutputDir)) Directory.CreateDirectory(OutputDir);
        int PCMin = 60;
        int PCMax = 80;
        int PCstep = 1;

        for (int pc = PCMin; pc <= PCMax; pc += PCstep)
        {
            for (int delay = DelayMin; delay <= DelayMax; delay += Shift)
            {
                
                episcan.SetDelayExposurePC(delay, Exposure, pc);


                // Switch to regular mode
                episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Rolling;

                //episcan.SetDelayExposure(Delay, Exposure);

                Console.WriteLine("Details");
                Console.WriteLine(episcan.Sensor);


                await Task.Delay(80);
                //episcan.Sensor.StartCapture();
                var capture = await episcan.CaptureAverage(AverageNum);
                //var filename = Path.GetFileName(patternFiles[i]);
                Cv2.ImWrite($"{OutputDir}\\" + delay.ToString()+pc.ToString()+ ".png", capture);


            }
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
using OpenCvSharp.Extensions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

public class Class1Script : IEpiscanScript
{
    public int DelayMin { set; get; } = -500;
    public int DelayMax { set; get; } = 500;
    public int Shift { set; get; } = 25;
    public float Exposure { set; get; } = 0.3f;
    public int AverageNum { set; get; } = 4;
    public int PCMin { set; get; } = 60;
    public int PCMax { set; get; } = 80;
    public int PCstep { set; get; } = 1;

    public void Configure(MyEpiscan episcan)
    {
        Debug.Write("hello");
    }

    public async Task Run(MyEpiscan episcan)
    {
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

                if (Directory.Exists(pc_dir))
                {
                    throw new System.ArgumentException("Already the directory exists", "pc_dir");
                }
                else
                {
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
