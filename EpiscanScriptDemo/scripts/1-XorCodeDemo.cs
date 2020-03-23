//css_import scripts\_XorCode;
//css_import scripts\_DisparityCalibration;

using System;
using System.Diagnostics;
using UserScriptHost;
using System.Linq;
using EpiscanUtil;
using OpenCvSharp;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;


public class XorCodeDemo : IEpiscanScript
{
    public string OutputDir { set; get; } = "capture";
    public string CalibFile { set; get; } = "calib.txt";
    public string DisparityFileBasename { set; get; } = "disparity";
    public int AverageNum { set; get; } = 4;

    private XorCode code;
    private DisparityCalibration calib;
    private Mat[] patterns;

    public void Configure(MyEpiscan episcan)
    {
        var size = new Size(episcan.Screen.Size.Width, episcan.Screen.Size.Height);
        code = new XorCode(size, xaxis:true, yaxis:false, inversePattern:true);

        calib = new DisparityCalibration();
        calib.Load(CalibFile);

        patterns = code.GeneratePatterns();
    }

    public async Task Run(MyEpiscan episcan)
    {
        if (!Directory.Exists(OutputDir)) Directory.CreateDirectory(OutputDir);

        episcan.Sensor.PixelFormat = MySensor.PixelFormatList.Mono8;

        // capture
        var captured = new List<Mat>();
        for (int i = 0; i < patterns.Count(); i++)
        {
            episcan.Screen.BackgroundMat = patterns[i];
            await Task.Delay(200);
            var capture = await episcan.CaptureAverage(AverageNum);
            captured.Add(capture);

            var filename = $"capture_{i:03D}.png";
            Cv2.ImWrite($"{OutputDir}\\{filename}", capture);
        }

        // decode
        var decoded = code.Decode(captured.ToArray());
        Console.WriteLine("calib");
        Console.WriteLine(calib);
        Console.WriteLine("next");
        var disparity = code.CovertToDisparity(decoded[0], calib);
        Cv2.ImWrite($"{OutputDir}\\decoded.png", decoded[0]);

        // save
        var disparityUint = new Mat();
        disparity.ConvertTo(disparityUint, MatType.CV_16UC1, 100);
        Cv2.ImWrite($"{OutputDir}\\{DisparityFileBasename}.png", disparityUint);

        // convert mat to float array
        var dispArr = new float[disparity.Height][];
        for (int i = 0; i < disparity.Height; i++)
        {
            dispArr[i] = new float[disparity.Width];
            for (int j = 0; j < disparity.Width; j++)
                dispArr[i][j] = disparity.At<float>(i, j);
        }
        File.WriteAllLines($"{OutputDir}\\{DisparityFileBasename}.txt", dispArr.Select(s => String.Join(" ", s)));

    }
}
