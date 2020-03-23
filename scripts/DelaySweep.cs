using System;
using System.Diagnostics;
using UserScriptHost;
using System.Collections.Generic;
using System.Linq;
using EpiscanUtil;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Threading.Tasks;

public class DelaySweepScript : IEpiscanScript
{
    public int DelayMin { set; get; } = -500;
    public int DelayMax { set; get; } = 500;
    public int Shift { set; get; } = 25;
    public float Exposure { set; get; } = 0.3f;
    public int AverageNum { set; get; } = 4;

    public void Configure(MyEpiscan episcan)
    {
        Debug.Write("hello");
    }

    public async Task Run(MyEpiscan episcan)
    {
        // generate patterns


        // Switch to synchronized mode
        episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Rolling;

        episcan.SetDelayExposure(0, Exposure);
        episcan.Screen.BackColor = System.Drawing.Color.Black;
        await Task.Delay(100);
        var black = await episcan.CaptureAverage(AverageNum);
        Cv2.ImWrite("capture_black.png", black);


        episcan.Screen.BackColor = System.Drawing.Color.White;
        await Task.Delay(100);
        for (int delay = DelayMin, i = 0; delay <= DelayMax; delay += Shift, i++)
        {
            episcan.SetDelayExposure(delay, Exposure);
            await Task.Delay(1);
            
            var captured = await episcan.CaptureAverage(AverageNum);
            var filename = $"capture_{i:D5}_{(int)delay:D5}.png";
            Cv2.ImWrite(filename, captured);
        }
    }
}
