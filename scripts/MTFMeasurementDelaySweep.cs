using System;
using System.Diagnostics;
using UserScriptHost;
using System.Collections.Generic;
using System.Linq;
using EpiscanUtil;
using OpenCvSharp;
using System.Threading.Tasks;
using System.IO;

public class MTFMeasurementDelaySweepScript : IEpiscanScript
{
    public int DelayMin { set; get; } = 9000;
    public int DelayMax { set; get; } = 12000;
    public int Shift { set; get; } = 250;
    public float FreqMin { set; get; } = 0f;
    public float FreqMax { set; get; } = 20f;
    public float FreqStep { set; get; } = 1f;
    public float PPCM { set; get; } = 0.05f;
    public float Exposure { set; get; } = 0.3f;
    public int AverageNum { set; get; } = 8;

    public void Configure(MyEpiscan episcan)
    {
        Debug.Write("hello");
    }

    public async Task Run(MyEpiscan episcan)
    {
        // generate patterns
        var projSize = new Size(episcan.Screen.Size.Width, episcan.Screen.Size.Height);
        var patterns = new List<Mat>();
        var xidx = Enumerable.Range(0, projSize.Width);
        //var xidx = Enumerable.Range(0, projSize.Height);

        for (float f = FreqMin, k = 0; f <= FreqMax; f += FreqStep)
        {
            for (int p = 0; p < 3; p++, k++)
            {
                var pattern = new Mat(projSize, MatType.CV_32FC1);
                var phase = p * 2f * Math.PI / 3f;
                var cos = xidx.Select(i => Math.Cos(i * f * PPCM + phase))
                    .Select(x => ((float)x * 0.5f + 0.5f) * 255f)
                    .ToArray();
                Debug.WriteLine(cos);

                var tmp = new Mat(new int[] { 1, projSize.Width },
                    MatType.CV_32FC1, cos);
                //var tmp = new Mat(new int[] { projSize.Height, 1 },
                //    MatType.CV_32FC1, cos);

                Cv2.Resize(tmp, pattern, projSize);
                var img = new Mat();
                pattern.ConvertTo(img, MatType.CV_8UC1);
                //Cv2.ImWrite($"pattern_{k}.png", img);
                patterns.Add(img);
                //File.WriteAllText($"pattern_{k}.txt", String.Join(",", cos));
            }
        }

        // Switch to synchronized mode
        episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Global;
        //episcan.SetDelayExposure(10000, 5f);

        episcan.SetDelayExposure(0f, Exposure);
        //episcan.SetDelayExposure(0f, 30);
        episcan.Screen.BackColor = System.Drawing.Color.Black;
        await Task.Delay(100);
        var black = await episcan.CaptureAverage(AverageNum);
        Cv2.ImWrite("capture_black.png", black);


        for (int delay = DelayMin, i = 0; delay <= DelayMax; delay += Shift, i++)
        {
            episcan.SetDelayExposure(delay, Exposure);
            await Task.Delay(1);

            for (int j = 0; j < patterns.Count; j++)
            {
                episcan.Screen.BackgroundMat = patterns[j];
                await Task.Delay(100);

                var captured = await episcan.CaptureAverage(AverageNum);
            var filename = $"capture_{i:D5}_{j:D5}.png";
            //var filename = $"capture_h_{j:D5}.png";
            Cv2.ImWrite(filename, captured);
            }
        }
    }
}
