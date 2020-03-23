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


public class XorCodeCalib : IEpiscanScript
{
    public float PlaneDistance { set; get; } = 500f;
    public float Baseline { set; get; } = 80f;
    public float FocalLength { set; get; } = 1000f;
    public string OutputFile { set; get; } = "calib.txt";
    public int AverageNum { set; get; } = 4;

    private XorCode code;
    private DisparityCalibration calib;
    private Mat[] patterns;

    public void Configure(MyEpiscan episcan)
    {
        var size = new Size(episcan.Screen.Size.Width, episcan.Screen.Size.Height);
        code = new XorCode(size, xaxis:true, yaxis:false, inversePattern:true);

        calib = new DisparityCalibration();

        patterns = code.GeneratePatterns();
    }

    public async Task Run(MyEpiscan episcan)
    {
        episcan.Sensor.PixelFormat = MySensor.PixelFormatList.Mono8;

        // capture
        var captured = new List<Mat>();
        for (int i = 0; i < patterns.Count(); i++)
        {
            episcan.Screen.BackgroundMat = patterns[i];
            await Task.Delay(200);
            var capture = await episcan.CaptureAverage(AverageNum);
            captured.Add(capture);
        }

        // decode
        Console.WriteLine("Decoding");
        var decoded = code.Decode(captured.ToArray());

        var d0 = Baseline * FocalLength / PlaneDistance;
        var filtered = new Mat();
        Cv2.BilateralFilter(decoded[0], filtered, -1, 4, 4);
        calib.CalibrateCoefficient(filtered, d0);

        // save
        Console.WriteLine("Saving");
        Console.WriteLine(calib);
        calib.Save(OutputFile);
    }
}
