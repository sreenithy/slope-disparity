using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageFileUtil
{
    public class Png
    {

        public static void SaveBgr48(string path, int width, int height, UInt16[] data)
        {
            var rgb = new UInt16[data.Length];
            for (int i = 0; i < data.Length / 3; i+=3)
            {
                var tmp = rgb[i * 3 + 2];
                rgb[i * 3 + 2] = data[i * 3];
                rgb[i * 3] = tmp;
            }
            SaveRgb48(path, width, height, rgb);
        }

        public static void SaveRgb48(string path, int width, int height, UInt16[] data)
        {
            int stride = width * 6; // ピクセル当たり6Byte

            // Creates a new empty image with the pre-defined palette

            System.Windows.Media.Imaging.BitmapSource image = System.Windows.Media.Imaging.BitmapSource.Create(
                width,
                height,
                96,
                96,
                System.Windows.Media.PixelFormats.Rgb48,
                null,
                data,
                stride);
            System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Create);
            System.Windows.Media.Imaging.PngBitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
            encoder.Save(stream);

            stream.Close();
        }
        public static void SaveRgba64(string path, int width, int height, UInt16[] data)
        {
            int stride = width * 8; // ピクセル当たり8Byte

            // Creates a new empty image with the pre-defined palette

            System.Windows.Media.Imaging.BitmapSource image = System.Windows.Media.Imaging.BitmapSource.Create(
                width,
                height,
                96,
                96,
                System.Windows.Media.PixelFormats.Rgba64,
                null,
                data,
                stride);
            System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Create);
            System.Windows.Media.Imaging.PngBitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
            encoder.Save(stream);

            stream.Close();
        }
        public static void SaveRgb128(string path, int width, int height, float[] data)
        {
            int stride = width * 16; // ピクセル当たり16Byte

            // Creates a new empty image with the pre-defined palette
            System.Windows.Media.Imaging.BitmapSource image = System.Windows.Media.Imaging.BitmapSource.Create(
                width,
                height,
                96,
                96,
                System.Windows.Media.PixelFormats.Rgb128Float,
                null,
                data,
                stride);
            System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Create);
            System.Windows.Media.Imaging.PngBitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
            encoder.Save(stream);

            stream.Close();
        }
        public static void SaveGray16(string path, int width, int height, UInt16[] data)
        {
            int stride = width * 2; // ピクセル当たり2Byte

            // Creates a new empty image with the pre-defined palette
            System.Windows.Media.Imaging.BitmapSource image = System.Windows.Media.Imaging.BitmapSource.Create(
                width,
                height,
                96,
                96,
                System.Windows.Media.PixelFormats.Gray16,
                null,
                data,
                stride);
            System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Create);
            System.Windows.Media.Imaging.PngBitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
            encoder.Save(stream);

            stream.Close();
        }
        public static void SaveGray8(string path, int width, int height, byte[] data)
        {
            int stride = width * 1; // ピクセル当たり1Byte

            // Creates a new empty image with the pre-defined palette
            System.Windows.Media.Imaging.BitmapSource image = System.Windows.Media.Imaging.BitmapSource.Create(
                width,
                height,
                96,
                96,
                System.Windows.Media.PixelFormats.Gray8,
                null,
                data,
                stride);
            System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Create);
            System.Windows.Media.Imaging.PngBitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
            encoder.Save(stream);

            stream.Close();
        }
        public static void SaveBgr24(string path, int width, int height, byte[] data)
        {
            int stride = width * 3; // ピクセル当たり3Byte

            // Creates a new empty image with the pre-defined palette
            System.Windows.Media.Imaging.BitmapSource image = System.Windows.Media.Imaging.BitmapSource.Create(
                width,
                height,
                96,
                96,
                System.Windows.Media.PixelFormats.Bgr24,
                null,
                data,
                stride);
            System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Create);
            System.Windows.Media.Imaging.PngBitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
            encoder.Save(stream);

            stream.Close();
        }
        public static void SaveRgb24(string path, int width, int height, byte[] data)
        {
            int stride = width * 3; // ピクセル当たり3Byte

            // Creates a new empty image with the pre-defined palette
            System.Windows.Media.Imaging.BitmapSource image = System.Windows.Media.Imaging.BitmapSource.Create(
                width,
                height,
                96,
                96,
                System.Windows.Media.PixelFormats.Rgb24,
                null,
                data,
                stride);
            System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Create);
            System.Windows.Media.Imaging.PngBitmapEncoder encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
            encoder.Save(stream);

            stream.Close();
        }
        /// <summary>
        /// http://efreedom.com/Question/1-7276212/Reading-Preserving-PixelFormatFormat48bppRgb-PNG-Bitmap-NET
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Bitmap FromFile(string path)
        {
            // Open a Stream and decode a PNG image
            Stream imageStreamSource = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            PngBitmapDecoder decoder = new PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            BitmapSource bitmapSource = decoder.Frames[0];

            int stride = bitmapSource.Format.BitsPerPixel / 8 * bitmapSource.PixelWidth;
            int height = bitmapSource.PixelHeight;
            int width = bitmapSource.PixelWidth;

            byte[] pixels = new byte[stride * height];

            bitmapSource.CopyPixels(pixels, stride, 0);

            Bitmap bmp = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, ConvertFormat(bitmapSource.Format));
            Rectangle rect = new Rectangle(0,0,width,height);

            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

            //byte[] b;
            //byte[] pixels = new byte[bmpd.Stride * bmpd.Height];
            //Marshal.Copy(bmpd.Scan0, pixels, 0, pixels.Length);
            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);

            bmp.UnlockBits(data);

            return bmp;

            //{
            //    byte[] pixels = new byte[800 * 600 * 64];
            //    bitmapSource.CopyPixels(pixels, 6400, 0);
            //}


            //// Convert WPF BitmapSource to GDI+ Bitmap
            //Bitmap bmp;

            //using (MemoryStream outStream = new MemoryStream())
            //{
            //    // from System.Media.BitmapImage to System.Drawing.Bitmap 
            //    BitmapEncoder enc = new BmpBitmapEncoder();
            //    enc.Frames.Add(BitmapFrame.Create(bitmapSource));
            //    enc.Save(outStream);
            //    bmp = new System.Drawing.Bitmap(outStream);
            //}

            //String info = String.Format("PixelFormat: {0}", bmp.PixelFormat);
            //return bmp;
        }
        public static System.Drawing.Imaging.PixelFormat ConvertFormat(System.Windows.Media.PixelFormat format)
        {
            switch (format.BitsPerPixel)
            {
                case 64:
                    return System.Drawing.Imaging.PixelFormat.Format64bppArgb;
                case 24:
                    return System.Drawing.Imaging.PixelFormat.Format24bppRgb;
                case 32:
                    return System.Drawing.Imaging.PixelFormat.Format32bppArgb;
                default:
                    return System.Drawing.Imaging.PixelFormat.Undefined;
            }
        }
    }
}
