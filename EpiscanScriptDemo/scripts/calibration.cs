using System;
using System.Diagnostics;
using UserScriptHost;
using System.Linq;
using EpiscanUtil;
using OpenCvSharp;
using System.Threading.Tasks;
using System.IO;

public class calibration : IEpiscanScript
{
    public string PatternDir1 { set; get; } = "pos";
    public string PatternDir2 { set; get; } = "neg";
    public string OutputDir { set; get; } = "newcapture";


    public void Configure(MyEpiscan episcan)
    {
        Debug.Write("hello");
    }

    public async Task Run(MyEpiscan episcan)
    {
        float delay = 1f;
        float exposure = 0.10f;
        int pc2 = 60;
        int gain = 0;
        int AverageNum = 5;
        episcan.Sensor.Delay = (int)(delay - 0.5f * exposure * 1e3f);
        episcan.Sensor.Exposure = exposure;
        episcan.Sensor.PixelClock = pc2;
        episcan.Sensor.MasterGain = gain;
        episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Rolling;

        var patternFiles = Directory.GetFiles(PatternDir1, "*.png").ToArray();
        var patterns = patternFiles.Select(src => Cv2.ImRead(src, ImreadModes.Unchanged)).ToArray();


        while (delay < 10000f)
        {
            Console.WriteLine(delay);
            string OutputDir1 = OutputDir + "\\pos_" + delay.ToString();
            if (!Directory.Exists(OutputDir1)) Directory.CreateDirectory(OutputDir1);

            for (int i = 0; i < patterns.Count(); i++)
            {
                episcan.Screen.BackgroundMat = patterns[i];
                await Task.Delay(100);
                var capture = await episcan.CaptureAverage(AverageNum);
                var filename = Path.GetFileName(patternFiles[i]);
                Cv2.ImWrite($"{OutputDir1}\\{filename}", capture);
            }
            delay = delay + 100f;
        }

        //Negative Patterns
        delay = 1f;

        episcan.Sensor.Delay = (int)(delay - 0.5f * exposure * 1e3f);
        episcan.Sensor.Exposure = exposure;
        episcan.Sensor.PixelClock = pc2;
        episcan.Sensor.MasterGain = gain;
        episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Rolling;

        var patternFilesNeg = Directory.GetFiles(PatternDir2, "*.png").ToArray();
        var patternsNeg = patternFilesNeg.Select(src => Cv2.ImRead(src, ImreadModes.Unchanged)).ToArray();


        while (delay < 10000f)
        {
            Console.WriteLine(delay);
            string OutputDir1 = OutputDir + "\\neg_" + delay.ToString();
            if (!Directory.Exists(OutputDir1)) Directory.CreateDirectory(OutputDir1);

            for (int i = 0; i < patternsNeg.Count(); i++)
            {
                episcan.Screen.BackgroundMat = patternsNeg[i];
                await Task.Delay(100);
                var capture = await episcan.CaptureAverage(AverageNum);
                var filename = Path.GetFileName(patternFilesNeg[i]);
                Cv2.ImWrite($"{OutputDir1}\\{filename}", capture);
            }
            delay = delay + 100f;
        }

    }
}
