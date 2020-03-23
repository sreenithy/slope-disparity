using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenCvSharp;


public class XorCode
{
    private Size projSize;
    private bool xaxis, yaxis;
    private bool inversePattern;
    private int[] codeToInt, intToCode;
    private int numX, numY;
    private int[][] neighbourCodes;

    public XorCode(Size projSize, bool xaxis = true, bool yaxis = false, bool inversePattern = false)
    {
        this.projSize = projSize;
        this.xaxis = xaxis;
        this.yaxis = yaxis;
        this.inversePattern = inversePattern;

        string codeFile = @"scripts/code11x1280.txt";
        int codeLength = 2048;
        if (this.projSize.Width > 1024)
        {
            codeFile = @"scripts/code11x1280.txt";
            codeLength = 2048;
        }

        codeToInt = new int[codeLength];
        intToCode = new int[codeLength];
        for (int i = 0; i < codeLength; i++)
        {
            codeToInt[i] = -1;
            intToCode[i] = -1;
        }
            
        // fill code to int
        using (var f = new StreamReader(codeFile))
        {
            string buf = "";
            int k = 0;
            while ((buf = f.ReadLine()) != null)
            {
                codeToInt[int.Parse(buf)] = k;
                ++k;
            }
        }
        for (int i = 0, k = 0; i < codeLength; i++)
        {
            if (codeToInt[i] == -1)
            {
                codeToInt[i] = this.projSize.Width + k;
                ++k;
            }
        }

        // fill int to code
        for (int i = 0; i < codeLength; i++)
            intToCode[codeToInt[i]] = i;

        // max code
        int maxCodeX = intToCode.Max(), maxCodeY = intToCode.Max();
        numX = xaxis ? (int)Math.Ceiling(Math.Log(maxCodeX, 2)) : 0;
        numY = yaxis ? (int)Math.Ceiling(Math.Log(maxCodeY, 2)) : 0;

        // populate neighbor
        neighbourCodes = new int[codeLength][];
        for (int i = 0; i < codeLength; i++)
        {
            var neighbour = new List<int>();
            var c0 = intToCode[i];
            for (int j = 0; j < Math.Max(this.projSize.Width, this.projSize.Height); j++)
            {
                var c1 = intToCode[j];
                var xor = c0 ^ c1;
                if (Convert.ToString(xor, 2).ToCharArray().Count(c => c == '1') <= 1)   // # of different bits <= 1
                    neighbour.Add(codeToInt[c1]);
            }

            neighbourCodes[i] = neighbour.ToArray();
        }
    }

    public Mat[] GeneratePatterns()
    {
        var pmult = inversePattern ? 2 : 1;
        var patterns = new Mat[pmult * (numX + numY)];

        // vertical patterns
        for (int k = 0; k < numX; k++)
        {
            var pattern = Mat.Zeros(projSize, MatType.CV_8UC1).ToMat();
            for (int i = 0; i < projSize.Width; i++)
            {
                var code = (intToCode[i] & (1 << k)) != 0 ? 255 : 0;
                pattern.Col[i].SetTo(code); 
            }
            pattern.Col[2].SetTo(255);
            pattern.Col[projSize.Width - 1].SetTo(255);

            if (inversePattern)
            {
                patterns[2 * k] = pattern;
                patterns[2 * k + 1] = 255 - pattern;
            }
            else
            {
                patterns[k] = pattern;
            }
        }

        // horizontal patterns
        for (int k = 0; k < numY; k++)
        {
            var pattern = new Mat(projSize, MatType.CV_8UC1);
            for (int i = 0; i < projSize.Height; i++)
            {
                var code = (intToCode[i] & (1 << k)) != 0 ? 255 : 0;
                pattern.Row[i].SetTo(code);
            }
            pattern.Row[2].SetTo(255);
            pattern.Row[projSize.Height - 1].SetTo(255);

            if (inversePattern)
            {
                patterns[2 * (k + numX)] = pattern;
                patterns[2 * (k + numX) + 1] = 255 - pattern;
            }
            else
            {
                patterns[k] = pattern;
            }
        }

        return patterns;
    }

    public Mat[] Decode(Mat[] imgs)
    {
        if (inversePattern)
        {
            if ((xaxis && !yaxis) || (!xaxis && yaxis))
                return new[] { DecodeInvXorCode(imgs, 0, imgs.Length) };
            else
            {
                return new[] {
                    DecodeInvXorCode(imgs, 0, 2 * numX),
                    DecodeInvXorCode(imgs, 2 * numX, imgs.Length)};
            }
        }
        else
        {
            if ((xaxis && !yaxis) || (!xaxis && yaxis))
                return new[] { DecodeXorCode(imgs, 0, imgs.Length) };
            else
            {
                return new[] {
                    DecodeXorCode(imgs, 0, numX),
                    DecodeXorCode(imgs, numX, imgs.Length)};
            }
        }
    }
        
    public Mat CovertToDisparity(Mat decoded, DisparityCalibration calib)
    {
        var imgSize = decoded.Size();
        Mat disparity = new Mat(imgSize, MatType.CV_32FC1);
        for (int y = 0; y < imgSize.Height; y++)
        {
            for (int x = 0; x < imgSize.Width; x++)
            {
                var v = decoded.At<float>(y, x);
                if (v < 0) disparity.Set(y, x, -1f);
                else
                {
                    var d = calib.Warp(v, y) - x;
                    disparity.Set(y, x, (float)d);
                }
            }
        }

        return disparity;
    }

    static public void FindMinMaxImage(Mat[] imgs, int idx0, int idx1, out Mat minImg, out Mat maxImg)
    {
        var subImgs = imgs.Skip(idx0).Take(idx1 - idx0).ToArray();
        minImg = subImgs[0].Clone();
        maxImg = subImgs[0].Clone();

        foreach (var img in subImgs)
        {
            Cv2.Min(minImg, img, minImg);
            Cv2.Max(maxImg, img, maxImg);
        }
        minImg.ConvertTo(minImg, MatType.CV_32FC1);
        maxImg.ConvertTo(maxImg, MatType.CV_32FC1);
    }

    public Mat DecodeSimple(Mat[] imgs, int idx0, int idx1)
    {
        Mat minImg = new Mat(), maxImg = new Mat();
        FindMinMaxImage(imgs, idx0, idx1, out minImg, out maxImg);
        var diff = maxImg - minImg;
        var T = ((minImg + maxImg) / 2).ToMat();
        //T.ConvertTo(T, MatType.CV_32FC1);
        //Cv2.ImShow("min", minImg / 255f);
        //Cv2.ImShow("max", maxImg / 255f);
        //Cv2.WaitKey();


        var imgSize = imgs[0].Size();
        var result = new Mat(imgSize, MatType.CV_32FC1);
        var img = new Mat(imgSize, MatType.CV_32FC1);
        var ones = Mat.Ones(imgSize, MatType.CV_32FC1);
        for (int i = idx1 - 1; i >= idx0; i--)
        {
            imgs[i].ConvertTo(img, MatType.CV_32FC1);
            result *= 2f;
            Cv2.Add(result, ones, result, img.GreaterThanOrEqual(T));
        }
        //Cv2.ImShow("diff", result * 0.001);

        for (int i = 0; i < imgSize.Height; i++)
            for (int j = 0; j < imgSize.Width; j++)
                result.Set(i, j, (float)codeToInt[(int)result.At<float>(i, j)]);

        //Cv2.ImShow("disp", result * 0.001);
        //Cv2.WaitKey();
        return result;
    }

    public Mat DecodeXorCode(Mat[] imgs, int idx0, int idx1)
    {
        //return DecodeSimple(imgs, idx0, idx1);
        //return MakeCorrections(DecodeSimple(imgs, idx0, idx1));
        var imgSize = imgs[0].Size();
        Mat result = DecodeSimple(imgs, idx0, idx1);
        Mat msk = CreateValidPixelMask(imgs, idx0, idx1);
        var resultMasked = new Mat(imgSize, MatType.CV_32FC1, -5f);
        result.CopyTo(resultMasked, msk);

        int count = 0;
        //try restore masked out pixels by looking at their neighbours
        for (int i = 1; i < imgSize.Width; i++)
            for (int j = 0; j < imgSize.Height; j++)
                if (resultMasked.At<float>(j, i) < 0 &&
                    Math.Abs(result.At<float>(j, i) - resultMasked.At<float>(j, i - 1)) <= 2)
                {
                    count++;
                    resultMasked.Set(j, i, result.At<float>(j, i));
                }


        for (int i = 0; i < imgSize.Width; i++)
            for (int j = 1; j < imgSize.Height; j++)
                if (resultMasked.At<float>(j, i) < 0 &&
                    Math.Abs(result.At<float>(j, i) - resultMasked.At<float>(j - 1, i)) <= 2)
                {
                    resultMasked.Set(j, i, result.At<float>(j, i));

                    count++;
                }

        for (int i = 0; i < imgSize.Width; i++)
            for (int j = 1; j < imgSize.Height; j++)
                if (resultMasked.At<float>(j, i) < 0)
                        count++;

                //return resultMasked;
        return MakeCorrections(resultMasked);
    }

    public Mat DecodeInvXorCode(Mat[] imgs, int idx0, int idx1)
    {
        var imgSize = imgs[0].Size();

        var result = new Mat(imgSize, MatType.CV_32FC1);
        var diffMax = new Mat(imgSize, MatType.CV_32FC1);
        for (int i = idx1 - 1; i >= idx0; i -= 2)
        {
            var img0 = imgs[i];
            var img1 = imgs[i - 1];
            //imgs[i].ConvertTo(img0, MatType.CV_32FC1);
            //imgs[i - 1].ConvertTo(img1, MatType.CV_32FC1);

            var diff = (img1 - img0).ToMat();
            diff.ConvertTo(diff, MatType.CV_32FC1);
            Cv2.Max(diff, diffMax, diffMax);
            result *= 2;
            Cv2.Add(result, Mat.Ones(imgSize, MatType.CV_32FC1), result, img1.GreaterThan(img0));
        }

        for (int i = 0; i < imgSize.Height; i++)
            for (int j = 0; j < imgSize.Width; j++)
                result.Set(i, j, (float)codeToInt[(int)result.At<float>(i, j)]);

        var mask = diffMax.GreaterThanOrEqual(2.0);
        var resultMasked = new Mat(imgSize, MatType.CV_32FC1);
        result.CopyTo(resultMasked, mask);

        return result;
    }

    private Mat CreateValidPixelMask(Mat[] imgs, int idx0, int idx1)
    {

        Mat minImg = new Mat(), maxImg = new Mat();
        FindMinMaxImage(imgs, idx0, idx1, out minImg, out maxImg);

        Mat diff = maxImg - minImg;
        Mat msk1 = new Mat(), msktmp = new Mat();
        Cv2.Threshold(diff, msktmp, 2, 255, ThresholdTypes.Binary);
        msktmp.ConvertTo(msk1, MatType.CV_8UC1);
	    return msk1;
    }


    private Mat MakeCorrections(Mat disparity)
    {
        var corr0 = new Mat();
        disparity.ConvertTo(corr0, MatType.CV_32FC1);
        var imgSize = corr0.Size();
        float tol = 3;

        var corr1 = corr0.Clone();
        Mat corr0_filt = new Mat();
        Cv2.MedianBlur(corr0, corr0_filt, 3);
            
        int successCount = 0;
        int failureCount = 0;

        for (int j = 0; j < imgSize.Height; j++)
        {
            for (int i = 0; i < imgSize.Width; i++)
            {
                if (corr0.At<float>(j, i) < 0)
                    continue;

                float curVal = corr0.At<float>(j, i);
                float filtVal = corr0_filt.At<float>(j, i);

                float diff = Math.Abs(curVal - filtVal);

                if (diff > tol || curVal >= projSize.Width)
                {
                    if (filtVal < 0)
                        corr1.Set(j, i, filtVal);
                    else
                    {
                        bool successFlag = false;
                        foreach (var v in neighbourCodes[(int)curVal])
                        {
                            if (Math.Abs(v - filtVal) < tol &&
                                Math.Abs(v - filtVal) <
                                Math.Abs(corr1.At<float>(j, i) - filtVal))
                            {
                                successCount++;
                                corr1.Set(j, i, v);
                                successFlag = true;
                            }
                        }
                        if (!successFlag)
                        {
                            failureCount++;
                            corr1.Set(j, i, -1);
                        }
                    }
                }
            }
        }

        return corr1;
    }
}