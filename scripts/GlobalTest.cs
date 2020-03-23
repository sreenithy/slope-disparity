using System;
using System.Diagnostics;
using UserScriptHost;
using System.Collections.Generic;
using System.Linq;
using EpiscanUtil;
using OpenCvSharp;
using System.Threading.Tasks;

public class GlobalTestScript : IEpiscanScript
{
    public float Delay { set; get; } = 0;
    public float Exposure { set; get; } = 0.5f;
    public int AverageNum { set; get; } = 16;

    public void Configure(MyEpiscan episcan)
    {
        Debug.Write("hello");
    }

    public async Task Run(MyEpiscan episcan)
    {
        // generate patterns
        var projSize = new Size(episcan.Screen.Size.Width, episcan.Screen.Size.Height);
        var pattern = new Mat(projSize, MatType.CV_8UC1);
        pattern.ColRange(600, 800).SetTo(255);

        // Switch to regular mode
        episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Global;

        episcan.SetDelayExposure(Delay, Exposure);
        episcan.Screen.BackgroundMat = pattern;
        await Task.Delay(100);
        var capture = await episcan.CaptureAverage(AverageNum);
        Cv2.ImShow("captured", capture);
        Cv2.WaitKey();
    }
}
