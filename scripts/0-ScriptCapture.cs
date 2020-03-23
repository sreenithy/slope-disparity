using System;
using System.Diagnostics;
using UserScriptHost;
using System.Linq;
using EpiscanUtil;
using OpenCvSharp;
using System.Threading.Tasks;
using System.IO;

public class ScriptCapture : IEpiscanScript
{
    public string PatternDir { set; get; } = "patterns";
    public string OutputDir { set; get; } = "capture";
    public int AverageNum { set; get; } = 50;

    public void Configure(MyEpiscan episcan)
    {
        Debug.Write("hello");
    }

    public async Task Run(MyEpiscan episcan)
    {
        var patternFiles = Directory.GetFiles(PatternDir, "*.png").ToArray();
        var patterns = patternFiles.Select(src => Cv2.ImRead(src, ImreadModes.Unchanged)).ToArray();

        if (!Directory.Exists(OutputDir)) Directory.CreateDirectory(OutputDir);

        for (int i = 0; i < patterns.Count(); i++)
        {
            episcan.Screen.BackgroundMat = patterns[i];
            await Task.Delay(100);
            var capture = await episcan.CaptureAverage(AverageNum);
            var filename = Path.GetFileName(patternFiles[i]);
            Cv2.ImWrite($"{OutputDir}\\{filename}", capture);
        }
    }
}
