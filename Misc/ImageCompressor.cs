using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using System.Drawing.Imaging;

namespace nboard
{
    static class ImageCompressor
    {
        public static byte[] Compress(byte[] image, int quality0to100, float sizeRatio)
        {
            if (!Directory.Exists("temp"))
                Directory.CreateDirectory("temp");
            string file = new Guid().ToString().Trim('{', '}');
            char sep = Path.DirectorySeparatorChar;
            string path = "temp" + sep + file;

            try
            {
                File.WriteAllBytes(path, image);
                var im = Image.FromFile(path);
                var bmp = new Bitmap(im, (int) (im.Width * sizeRatio + 1), (int) (im.Height * sizeRatio + 1));
                bmp = Sharpen(bmp);
                SaveJpeg(path + ".jpg", bmp, quality0to100);
                var bytes =  File.ReadAllBytes(path + ".jpg");
                File.Delete(path);
                File.Delete(path + ".jpg");
                return bytes;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        // http://stackoverflow.com/questions/903632/sharpen-on-a-bitmap-using-c-sharp
        private static Bitmap Sharpen(Bitmap image)
        {
            Bitmap sharpenImage = (Bitmap)image.Clone();

            int filterWidth = 3;
            int filterHeight = 3;
            int width = image.Width;
            int height = image.Height;

            // Create sharpening filter.
            double[,] filter = new double[filterWidth, filterHeight];
            filter[0, 0] = filter[0, 1] = filter[0, 2] = filter[1, 0] = filter[1, 2] = filter[2, 0] = filter[2, 1] = filter[2, 2] = -1;
            filter[1, 1] = 9;

            double factor = 1.0;
            double bias = 0.0;

            Color[,] result = new Color[image.Width, image.Height];

            // Lock image bits for read/write.
            BitmapData pbits = sharpenImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            // Declare an array to hold the bytes of the bitmap.
            int bytes = pbits.Stride * height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(pbits.Scan0, rgbValues, 0, bytes);

            int rgb;
            // Fill the color array with the new sharpened color values.
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    double red = 0.0, green = 0.0, blue = 0.0;

                    for (int filterX = 0; filterX < filterWidth; filterX++)
                    {
                        for (int filterY = 0; filterY < filterHeight; filterY++)
                        {
                            int imageX = (x - filterWidth / 2 + filterX + width) % width;
                            int imageY = (y - filterHeight / 2 + filterY + height) % height;

                            rgb = imageY * pbits.Stride + 3 * imageX;

                            red += rgbValues[rgb + 2] * filter[filterX, filterY];
                            green += rgbValues[rgb + 1] * filter[filterX, filterY];
                            blue += rgbValues[rgb + 0] * filter[filterX, filterY];
                        }
                        int r = Math.Min(Math.Max((int)(factor * red + bias), 0), 255);
                        int g = Math.Min(Math.Max((int)(factor * green + bias), 0), 255);
                        int b = Math.Min(Math.Max((int)(factor * blue + bias), 0), 255);

                        result[x, y] = Color.FromArgb(r, g, b);
                    }
                }
            }

            // Update the image with the sharpened pixels.
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    rgb = y * pbits.Stride + 3 * x;

                    rgbValues[rgb + 2] = (byte)(3*rgbValues[rgb + 2]/4 + result[x, y].R/4);
                    rgbValues[rgb + 1] = (byte)(3*rgbValues[rgb + 1]/4 + result[x, y].G/4);
                    rgbValues[rgb + 0] = (byte)(3*rgbValues[rgb + 0]/4 + result[x, y].B/4);
                }
            }

            // Copy the RGB values back to the bitmap.
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, pbits.Scan0, bytes);
            // Release image bits.
            sharpenImage.UnlockBits(pbits);

            return sharpenImage;
        }

        // from http://geekswithblogs.net/bullpit/archive/2009/04/29/compress-image-files-using-c.aspx
        /// <summary> 
        /// Saves an image as a jpeg image, with the given quality 
        /// </summary> 
        /// <param name="path">Path to which the image would be saved.</param> 
        // <param name="quality">An integer from 0 to 100, with 100 being the 
        /// highest quality</param> 
        private static void SaveJpeg (string path, Image img, int quality) 
        {
            if (quality<0  ||  quality>100) 
                throw new ArgumentOutOfRangeException("quality must be between 0 and 100."); 
             
            // Encoder parameter for image quality 
            EncoderParameter qualityParam = 
                new EncoderParameter (System.Drawing.Imaging.Encoder.Quality, quality); 
            // Jpeg image codec 
            ImageCodecInfo   jpegCodec = GetEncoderInfo("image/jpeg"); 

            EncoderParameters encoderParams = new EncoderParameters(1); 
            encoderParams.Param[0] = qualityParam;
            img.Save (path, jpegCodec, encoderParams); 
        }

        /// <summary> 
        /// Returns the image codec with the given mime type 
        /// </summary> 
        private static ImageCodecInfo GetEncoderInfo(string mimeType) 
        { 
            // Get image codecs for all image formats 
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders(); 

            // Find the correct image codec 
            for(int i=0; i<codecs.Length; i++) 
                if(codecs[i].MimeType == mimeType) 
                    return codecs[i]; 
            return null; 
        } 
    }
}