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
    public string OutputDir { set; get; } = "capture";
    public int AverageNum { set; get; } = 5;

    public void Configure(MyEpiscan episcan)
    {
        Debug.Write("hello");
    }

    public async Task Run(MyEpiscan episcan)
    {
        float delay = 1f;
        float exposure = 0.05f;
        int pc2 = 60;
        int gain = 0;
        
        episcan.Sensor.Delay = (int)(delay - 0.5f * exposure * 1e3f);
        episcan.Sensor.Exposure = exposure;
        episcan.Sensor.PixelClock = pc2;
        episcan.Sensor.MasterGain = gain;
       
        episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Rolling;
        var patternFiles = Directory.GetFiles(PatternDir1, "*.png").ToArray();
        var patterns = patternFiles.Select(src => Cv2.ImRead(src, ImreadModes.Unchanged)).ToArray();

        while (delay <100f)
        {   
            if (!Directory.Exists(OutputDir)) Directory.CreateDirectory(OutputDir);

            for (int i = 0; i < patterns.Count(); i++)
            {
                episcan.Screen.BackgroundMat = patterns[i];
                await Task.Delay(100);
                var capture = await episcan.CaptureAverage(AverageNum);
                var filename = Path.GetFileName(patternFiles[i]);
                Cv2.ImWrite($"{OutputDir}\\{filename}", capture);
            }
            delay = delay + 10f;

        }
        
    }
}
