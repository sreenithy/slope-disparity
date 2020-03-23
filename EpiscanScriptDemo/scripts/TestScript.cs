

using System;
using System.Diagnostics;
using UserScriptHost;
using System.Collections.Generic;
using System.Linq;
using EpiscanUtil;
using OpenCvSharp;
using System.Threading.Tasks;
using System.IO;
//using Emgu.CV;
//using Emgu.CV.CvEnum;
//using Emgu.CV.Structure;

public class TestScript : IEpiscanScript
{
    public void Configure(MyEpiscan episcan)
    {
        Debug.Write("hello");
    }

    public async Task Run(MyEpiscan episcan)
    {
        float delay = 17643f;
        float exposure = 1.648f;
        int pc2 = 54;
        int gain = 0;
        //int scaler = 150;
        episcan.Sensor.Delay = (int)(delay - 0.5f * exposure * 1e3f);
        //episcan.Sensor.Delay = delay;
        episcan.Sensor.Exposure = exposure;
        episcan.Sensor.PixelClock = pc2;
        episcan.Sensor.MasterGain = gain;
        //episcan.Sensor.Scaler = scaler;
        episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Rolling;
        //Mat img = new Mat("C:/episcandemo-win-taka20190529-001/episcandemo-win-taka/bin/Debug/scripts/Capture.png", 0);
        Mat img = new Mat("C:/Users/sreen/PycharmProjects/featureextraction/box_full.png");
        Mat dst1 = new Mat();
        OpenCvSharp.ORB orb = ORB.Create();
        KeyPoint[] keypoints = orb.Detect(img, null);
        Mat descriptor = new Mat();
        orb.Compute(img, ref keypoints, descriptor);
        //Cv2.DrawKeypoints(img, keypoints, img, new Scalar(0, 255, 0), DrawMatchesFlags.DrawRichKeypoints);
        int X = keypoints.Length;
        Cv2.ImShow("image", img);
        Cv2.ImWrite("keypointall.png", img);
        await Task.Delay(100);

        //Segment refernce image into three
        Mat imgorg1 = img[0, 260, 0, 1600];
        KeyPoint[] keypoints11 = orb.Detect(imgorg1, null);
        Cv2.ImWrite("imgorg1.png", imgorg1);
        Mat descriptor11 = new Mat();
        orb.Compute(img, ref keypoints11, descriptor11);

        Mat imgorg2 = img[260, 520, 0, 1600];
        KeyPoint[] keypoints12 = orb.Detect(imgorg2, null);
        Mat descriptor12 = new Mat();
        Cv2.ImWrite("imgorg2.png", imgorg2);
        orb.Compute(img, ref keypoints12, descriptor12);

        Mat imgorg3 = img[520, 780, 0, 1600];
        KeyPoint[] keypoints13 = orb.Detect(imgorg3, null);
        Mat descriptor13 = new Mat();
        Cv2.ImWrite("imgorg3.png", imgorg3);
        orb.Compute(img, ref keypoints13, descriptor13);

        //Capture the image
        var capture = await episcan.CaptureAverage();
        KeyPoint[] keypoints2 = orb.Detect(capture, null);
        Mat descriptor2 = new Mat();
        orb.Compute(capture, ref keypoints2, descriptor2);
        //Cv2.DrawKeypoints(capture, keypoints2, capture, new Scalar(0, 255, 0), DrawMatchesFlags.DrawRichKeypoints);
        Cv2.ImWrite("cap.png", capture);
        Mat captureimg = new Mat("C:/episcandemo-win-taka20190529-001/episcandemo-win-taka/bin/Debug/cap.png", 0);

        //Segment captured scenen into three halves
        Mat cap1 = captureimg[0, 260, 0, 1600];
        Cv2.ImWrite("cap11.png", cap1);
        KeyPoint[] capkeypoints11 = orb.Detect(cap1, null);
        Mat capdescriptor11 = new Mat();
        orb.Compute(cap1, ref capkeypoints11, capdescriptor11);

        Mat cap2 = captureimg[260, 520, 0, 1600];
        KeyPoint[] capkeypoints12 = orb.Detect(cap2, null);
        Mat capdescriptor12 = new Mat();
        Cv2.ImWrite("cap12.png", cap2);
        orb.Compute(cap2, ref capkeypoints12, capdescriptor12);

        Mat cap3 = captureimg[520, 780, 0, 1600];
        KeyPoint[] capkeypoints13 = orb.Detect(cap3, null);
        Cv2.ImWrite("cap13.png", cap3);
        Mat capdescriptor13 = new Mat();
        orb.Compute(cap3, ref capkeypoints13, capdescriptor13);

        //Match the three segments with the refernece image
        var matcher = new BFMatcher();
        var M1 = matcher.Match(descriptor11, capdescriptor11);
        var imgMatches1 = new Mat();
        Cv2.DrawMatches(imgorg1, keypoints11, cap1, capkeypoints11, M1, imgMatches1);
        Cv2.ImWrite("match1.png", imgMatches1);

        var M2 = matcher.Match(descriptor12, capdescriptor12);
        var imgMatches2 = new Mat();
        Cv2.DrawMatches(imgorg2, keypoints12, cap2, capkeypoints12, M2, imgMatches2);
        Cv2.ImWrite("match2.png", imgMatches2);

        var M3 = matcher.Match(descriptor13, capdescriptor13);
        var imgMatches3 = new Mat();
        Cv2.DrawMatches(imgorg3, keypoints13, cap3, capkeypoints13, M3, imgMatches3);
        Cv2.ImWrite("match3.png", imgMatches3);
        Console.WriteLine("{0},{1},{2}", M1.Length, M2.Length, M3.Length);

        //If M1/M2/M3 is zero implies that region is dark
        //Non-zero value depicts the roi is captured 
        /*
        while (keypoints11.Length>=capkeypoints11.Length)
        {
            Console.WriteLine("{0},{1},{2},{3},{4},{5}", keypoints11.Length, capkeypoints11.Length, keypoints12.Length, capkeypoints12.Length, keypoints13.Length, capkeypoints13.Length);

            exposure = exposure + 0.5f;
            episcan.Sensor.Delay = (int)(delay - 0.5f * exposure * 1e3f);
            //episcan.Sensor.Delay = delay;
            episcan.Sensor.Exposure = exposure;
            episcan.Sensor.PixelClock = pc2;
            episcan.Sensor.MasterGain = gain;
            //episcan.Sensor.Scaler = scaler;
            episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Rolling;
            var cap= await episcan.CaptureAverage();

            Cv2.ImShow("capnextframe.png", capture);
            Cv2.ImWrite("capnextframe.png", capture);
            Mat captureimg2 = new Mat("C:/episcandemo-win-taka20190529-001/episcandemo-win-taka/bin/Debug/capnextframe.png", 0);

            //Segment captured scenen into three halves
            Mat cap11 = captureimg2[0, 260, 0, 1600];
            Cv2.ImWrite("cap111.png", cap11);
            capkeypoints11 = orb.Detect(cap11, null);
            Mat capdescriptor111 = new Mat();
            orb.Compute(cap11, ref capkeypoints11, capdescriptor111);

            Mat cap22 = captureimg2[260, 520, 0, 1600];
            capkeypoints12 = orb.Detect(cap22, null);
            Mat capdescriptor122 = new Mat();
            Cv2.ImWrite("cap122.png", cap22);
            orb.Compute(cap22, ref capkeypoints12, capdescriptor122);

            Mat cap33 = captureimg2[520, 780, 0, 1600];
            capkeypoints13 = orb.Detect(cap33, null);
            Cv2.ImWrite("cap133.png", cap33);
            Mat capdescriptor133 = new Mat();
            orb.Compute(cap33, ref capkeypoints13, capdescriptor133);

            //Match the three segments with the refernece image
            var matcher2 = new BFMatcher();
            var M11 = matcher2.Match(descriptor11, capdescriptor111);
            var imgMatches11 = new Mat();
            Cv2.DrawMatches(imgorg1, keypoints11, cap11, capkeypoints11, M11, imgMatches11);
            Cv2.ImWrite("match1.png", imgMatches11);

            var M22 = matcher2.Match(descriptor12, capdescriptor122);
            var imgMatches22 = new Mat();
            Cv2.DrawMatches(imgorg2, keypoints12, cap22, capkeypoints12, M22, imgMatches2);
            Cv2.ImWrite("match2.png", imgMatches22);

            var M33 = matcher2.Match(descriptor13, capdescriptor133);
            var imgMatches33 = new Mat();
            Cv2.DrawMatches(imgorg3, keypoints13, cap33, capkeypoints13, M33, imgMatches3);
            Cv2.ImWrite("match3.png", imgMatches33);
            Console.WriteLine("{0},{1},{2}", M11.Length, M22.Length, M33.Length);

            
        }
        */



    }
}

/*
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

 */
