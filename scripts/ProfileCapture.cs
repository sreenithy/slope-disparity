using System;
using System.Diagnostics;
using UserScriptHost;
using System.Collections.Generic;
using System.Linq;
using EpiscanUtil;
using OpenCvSharp;
using System.Threading.Tasks;

public class ProfileCaptureScript : IEpiscanScript
{
    public int Left { set; get; } = 0;
    public int Right { set; get; } = 1280;
    public int XShift { set; get; } = 1;
    public int LineWidth { set; get; } = 10;
    public int DelayMin { set; get; } = -340;
    public int DelayMax { set; get; } = 340;
    public int Shift { set; get; } = 17;
    public float Exposure { set; get; } = 0.170f;
    public int PatchSize { set; get; } = 50;
    public int AverageNum { set; get; } = 4;

    public void Configure(MyEpiscan episcan)
    {
        Debug.Write("hello");
    }

    public async Task Run(MyEpiscan episcan)
    {   
        // generate patterns
        var projSize = new Size(episcan.Screen.Size.Width, episcan.Screen.Size.Height);
        var patterns = new List<Mat>();

        var patchCount = (Right - Left) / PatchSize;

        for (int i = 0; i + LineWidth <= PatchSize; i += XShift)
        {
            var pattern = new Mat(projSize, MatType.CV_8UC1);
            pattern.ColRange(0, 1).SetTo(255);
            pattern.ColRange(projSize.Width - 1, projSize.Width).SetTo(255);
            for (int j = 0; j < patchCount; j++)
            {
                var patchLeft = j * PatchSize;
                pattern.ColRange(i + patchLeft, i + LineWidth + patchLeft).SetTo(255);
            }
            patterns.Add(pattern);
            Cv2.ImWrite($"pattern_{patterns.Count - 1}.png", pattern);
        }
        
        // Switch to synchronized mode
        episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Rolling;

        // capture with patterns
        for (int delay = DelayMin, i = 0; delay <= DelayMax; delay += Shift, i++)
        {
            await Task.Delay(50);
            episcan.SetDelayExposure(delay, Exposure);

            for (int j = 0; j < patterns.Count; j++)
            {
                episcan.Screen.BackgroundMat = patterns[j];
                await Task.Delay(100);

                var captured = await episcan.CaptureAverage(AverageNum);
                var filename = $"capture_{i:D5}_{(int)delay:D5}_{j:D5}.png";
                Cv2.ImWrite(filename, captured);
            }
        }

        // capture black
        episcan.SetDelayExposure(0, Exposure);
        episcan.Screen.BackgroundMat = null;
        episcan.Screen.BackColor = System.Drawing.Color.Black;
        await Task.Delay(100);
        var black = await episcan.CaptureAverage(AverageNum);
        Cv2.ImWrite("capture_black.png", black);

        // capture white
        episcan.Screen.BackColor = System.Drawing.Color.White;
        await Task.Delay(100);
        var white = await episcan.CaptureAverage(AverageNum);
        Cv2.ImWrite("capture_white.png", white);
    }
}
