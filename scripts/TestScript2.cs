

using System;
using System.Diagnostics;
using UserScriptHost;
using System.Collections.Generic;
using System.Linq;
using EpiscanUtil;
using OpenCvSharp;
using System.Threading.Tasks;
using System.IO;

public class TestScript2 : IEpiscanScript
{
    public void Configure(MyEpiscan episcan)
    {
        Debug.Write("hello");
    }
    public async Task Run(MyEpiscan episcan)
    {
        float delay = 0f;
        float exposure = 0.5f;
        int pc2 = 68;
        int gain = 0;
        int scaler = 150;
        episcan.Sensor.Delay = (int)(delay - 0.5f * exposure * 1e3f);
        //episcan.Sensor.Delay = delay;
        episcan.Sensor.Exposure = exposure;
        episcan.Sensor.PixelClock = pc2;
        episcan.Sensor.MasterGain = gain;
        episcan.Sensor.Scaler = scaler;
        episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Rolling;
        //Mat img = new Mat("C:/episcandemo-win-taka20190529-001/episcandemo-win-taka/bin/Debug/scripts/Capture.png", 0);
        Mat img = new Mat("C:/episcandemo-win-taka20190529-001/episcandemo-win-taka/Capture.png", 0);

        Mat dst1 = new Mat();

        OpenCvSharp.ORB orb = ORB.Create();
        KeyPoint[] keypoints = orb.Detect(img, null);
        Mat descriptor = new Mat();
        orb.Compute(img, ref keypoints, descriptor);
        Cv2.DrawKeypoints(img, keypoints, img, new Scalar(0, 255, 0), DrawMatchesFlags.Default);
        //Cv2.ImShow("image3", img);
        Cv2.ImWrite("keypointall.png", img);
        await Task.Delay(100);

        var capture = await episcan.CaptureAverage();
        KeyPoint[] keypoints2 = orb.Detect(capture, null);
        Mat descriptor2 = new Mat();
        orb.Compute(capture, ref keypoints2, descriptor2);

        //Cv2.Canny(capture, dst, 50, 100);
        //Cv2.ImWrite("test.png", capture);
        Cv2.DrawKeypoints(capture, keypoints2, capture, new Scalar(0, 255, 0), DrawMatchesFlags.Default);
        Cv2.ImShow("capture", capture);
        int X = 100;
        int Y = keypoints2.Length;
        Console.WriteLine(Y);
        // matching descriptors
        /*
        var matcher = new BFMatcher();
        var matches = matcher.Match(descriptor, descriptor2);
        // drawing the results
        var imgMatches = new Mat();
        Cv2.DrawMatches(img, keypoints, capture, keypoints2, matches, imgMatches);
        Cv2.ImWrite("Matches.png", imgMatches);*/



        while (Y == 0)
        {
            delay = delay + 500f;
            episcan.Sensor.Delay = (int)(delay - 0.5f * exposure * 1e3f);

            episcan.Sensor.Exposure = exposure;

            episcan.Sensor.MasterGain = gain;
            episcan.Sensor.Scaler = scaler;
            episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Rolling;
            var capture3 = await episcan.CaptureAverage();
            KeyPoint[] keypoints3 = orb.Detect(capture3, null);
            Mat descriptor3 = new Mat();
            orb.Compute(capture, ref keypoints3, descriptor3);

            //Cv2.Canny(capture, dst, 50, 100);
            //Cv2.ImWrite("test.png", capture);
            Cv2.DrawKeypoints(capture3, keypoints3, capture3, new Scalar(0, 255, 0), DrawMatchesFlags.Default);
            Cv2.ImShow("capture", capture3);
            Y = keypoints3.Length;
            Console.WriteLine("{0},{1}", "Synchronisation Delay", episcan.Sensor.Delay.ToString());
            Console.WriteLine("{0},{1}", "Synchronisation Exposure", episcan.Sensor.Exposure.ToString());
            Console.WriteLine("{0},{1}", "Synchronisation pixelclock", episcan.Sensor.PixelClock.ToString());
            Console.WriteLine("{0},{1}", "Keypoint ref ", X.ToString());
            Console.WriteLine("{0},{1}", "keypoint img ", Y.ToString());


            //await Task.Delay(100);


        }
        while (Y < X)
        {
            exposure = exposure + 500f;
            delay = delay;
           
            episcan.Sensor.Delay = (int)(delay - 0.5f * exposure * 1e3f);
            episcan.Sensor.Exposure = exposure;
            pc2 = pc2 + 2;
            episcan.Sensor.PixelClock = pc2;
            episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Rolling;
            var capture3 = await episcan.CaptureAverage();
            KeyPoint[] keypoints3 = orb.Detect(capture3, null);
            Mat descriptor3 = new Mat();
            orb.Compute(capture, ref keypoints3, descriptor3);

            //Cv2.Canny(capture, dst, 50, 100);
            //Cv2.ImWrite("test.png", capture);
            Cv2.DrawKeypoints(capture3, keypoints3, capture3, new Scalar(0, 255, 0), DrawMatchesFlags.Default);
            Cv2.ImShow("capture", capture3);
            Y = keypoints3.Length;
            Console.WriteLine("{0},{1}", "Synchronisation Delay", episcan.Sensor.Delay.ToString());
            Console.WriteLine("{0},{1}", "Synchronisation Exposure", episcan.Sensor.Exposure.ToString());
            Console.WriteLine("{0},{1}", "Synchronisation pixelclock", episcan.Sensor.PixelClock.ToString());
            Console.WriteLine("{0},{1}", "Keypoint ref ", X.ToString());
            Console.WriteLine("{0},{1}", "keypoint img ", Y.ToString());
        }

    }
}