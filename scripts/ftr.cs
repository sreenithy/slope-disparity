

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

public class ftr : IEpiscanScript
{
    public void Configure(MyEpiscan episcan)
    {
        Debug.Write("hello");
    }

    public async Task Run(MyEpiscan episcan)
    {
        float delay = 17643f;
        float exposure = 1.648f;
        int pc2 = 70;
        int gain = 0;
        //int scaler = 150;
        episcan.Sensor.Delay = (int)(delay - 0.5f * exposure * 1e3f);
        //episcan.Sensor.Delay = delay;
        episcan.Sensor.Exposure = exposure;
        episcan.Sensor.PixelClock = pc2;
        episcan.Sensor.MasterGain = gain;
        //episcan.Sensor.Scaler = scaler;
        episcan.Sensor.ShutterMode = MySensor.ShutterModeList.Rolling;
        Mat img = new Mat("C:/Users/sreen/PycharmProjects/featureextraction/maskmethod/box_full_black.png");
        keypoints_database = cPickle.load(open("C:/Users/sreen/PycharmProjects/featureextraction/maskmethod/box_4.p", "rb"));
        Keypoints[] keypoints;
        Mat descriptors;
        /*
        for (point in keypoints_database[0])
        {


        }
            
        temp_feature = cv2.KeyPoint(x = point[0][0], y = point[0][1], _size = point[1], _angle = point[2], _response = point[3], _octave = point[4], _class_id = point[5])
        temp_descriptor = point[6]

        keypoints.append(temp_feature)
        descriptors.append(temp_descriptor)
        kp1, des1 = cPickle_keypoints_draw(keypoints_database[0]);
        */
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



    }
}

