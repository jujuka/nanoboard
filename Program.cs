using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace nboard
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            /*
            var bytes = Encoding.UTF8.GetBytes("Ёлки, палки.");
            new PngStegoUtil().HideBytesInPng("containers/dummy.png", "upload/test.png", GZipUtil.Compress(bytes));
            var read = new PngStegoUtil().ReadHiddenBytesFromPng("upload/test.png");
            Console.WriteLine(Encoding.UTF8.GetString(GZipUtil.Decompress(read)));

            var p = NanoPost.Create("Проверка1");
            var packed = NanoPostPackUtil.Pack(new []{p});
            NanoPost[] unpacked = NanoPostPackUtil.Unpack(packed);
            Console.WriteLine(unpacked[0].Message);
            */
            new ApplicationStarter();
        }
    }
}