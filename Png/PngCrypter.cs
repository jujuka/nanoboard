using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
 
namespace nboard
{
    class PngCrypter
    {
        public void Crypt(string inputImageFileName, string outputImageFileName, byte[] hiddenBytes)
        {
            byte[] hiddenLengthBytes = BitConverter.GetBytes(hiddenBytes.Length);
            byte[] hiddenCombinedBytes = PngCrypterUtils.Combine(hiddenLengthBytes, hiddenBytes);
            Image innocuousBmp = Image.FromFile(inputImageFileName);
            byte[] rgbComponents = PngCrypterUtils.RgbComponentsToBytes(innocuousBmp);
            byte[] encodedRgbComponents = EncodeBytes(hiddenCombinedBytes, rgbComponents);
            Bitmap encodedBmp = PngCrypterUtils.ByteArrayToBitmap(encodedRgbComponents, innocuousBmp.Width, innocuousBmp.Height);
            encodedBmp.Save(outputImageFileName, ImageFormat.Png);
            encodedBmp.Dispose();
            innocuousBmp.Dispose();
        }
 
        private static byte[] EncodeBytes(byte[] hiddenBytes, byte[] innocuousBytes)
        {
            BitArray hiddenBits = new BitArray(hiddenBytes);
            byte[] encodedBitmapRgbComponents = new byte[innocuousBytes.Length];

            for (int i = 0; i < innocuousBytes.Length; i++)
            {
                if (i < hiddenBits.Length)
                {
                    byte evenByte = (byte)(innocuousBytes[i] - innocuousBytes[i] % 2);
                    encodedBitmapRgbComponents[i] = (byte)(evenByte + (hiddenBits[i] ? 1 : 0));
                }

                else
                {
                    encodedBitmapRgbComponents[i] = innocuousBytes[i];
                }
            }
            return encodedBitmapRgbComponents;
        }
 
        public byte[] Decrypt(string imageFileName)
        {
            Bitmap loadedEncodedBmp = new Bitmap(imageFileName);
            byte[] loadedEncodedRgbComponents = PngCrypterUtils.RgbComponentsToBytes(loadedEncodedBmp);
            const int bytesInInt = 4;
            byte[] loadedHiddenLengthBytes = DecodeBytes(loadedEncodedRgbComponents, 0, bytesInInt);
            int loadedHiddenLength = BitConverter.ToInt32(loadedHiddenLengthBytes, 0);
            byte[] loadedHiddenBytes = DecodeBytes(loadedEncodedRgbComponents, bytesInInt, loadedHiddenLength);
            loadedEncodedBmp.Dispose();
            return loadedHiddenBytes;
        }
 
        private static byte[] DecodeBytes(byte[] innocuousLookingData, int byteIndex, int byteCount)
        {
            const int bitsInBytes = 8;
            int bitCount = byteCount * bitsInBytes;
            int bitIndex = byteIndex * bitsInBytes;
            bool[] loadedHiddenBools = new bool[bitCount];

            for (int i = 0; i < bitCount; i++)
            {
                loadedHiddenBools[i] = innocuousLookingData[i + bitIndex] % 2 == 1;
            }

            BitArray loadedHiddenBits = new BitArray(loadedHiddenBools);
            byte[] loadedHiddenBytes = new byte[loadedHiddenBits.Length / bitsInBytes];
            loadedHiddenBits.CopyTo(loadedHiddenBytes, 0);
            return loadedHiddenBytes;
        }
    }
}