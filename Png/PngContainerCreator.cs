using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Drawing;

namespace nboard
{
    class PngContainerCreator
    {
        private const int Attempts = 10;
        static Random random = new Random();

        public void SaveToPngContainer(NanoDB db)
        {
            var files = new DirectoryInfo(Strings.Containers).GetFiles("*.png");

            if (files.Length == 0)
            {
                NotificationHandler.Instance.AddNotification("Не найдены PNG файлы в папке containers.");
                return;
            }

            FileInfo file = files[random.Next(files.Length - 1)];
            var bmp = new Bitmap(file.FullName);
            int capacity = (int)(bmp.Width * bmp.Height * 3 / 8);
            string sessionPrefix = random.Next().ToString("x8");
            sessionPrefix += random.Next().ToString("x8");

            byte[] packed = null;
            byte[] prevPacked = null;
            int amount = 1;

            int realAmount = 0;

            while (amount < 65535 && (prevPacked == null || packed == null || packed.Length < capacity))
            {
                prevPacked = packed;
                var lastPosts = db.GetNLastPosts(amount).ExceptHidden(db);
                var randomPosts = db.GetNRandomPosts(amount).ExceptHidden(db).FilterBySize(16384);
                var posts = new List<NanoPost>();
                posts.AddRange(lastPosts);
                posts.AddRange(randomPosts);
                realAmount = posts.Count;
                packed = Pack(posts.ToArray().Randomized());
                amount = 1 + amount + amount * 10 / 15;
            }

            packed = prevPacked;
            Console.WriteLine(string.Format("PNG capacity:{0}, posts amount (not unique):{1}, packed size:{2}", capacity, realAmount, packed.Length));

            if (capacity != packed.Length)
            {
                var noise = new byte[capacity - packed.Length];
                random.NextBytes(noise);
                var temp = new List<byte>();
                temp.AddRange(packed);
                temp.AddRange(noise);
                packed = temp.ToArray();
            }

            new PngStegoUtil().HideBytesInPng(
                        file.FullName, 
                        Strings.Upload + Path.DirectorySeparatorChar + sessionPrefix + Strings.PngExt, 
                        packed);
            NotificationHandler.Instance.AddNotification("Контейнер сохранён: " + Strings.Upload + Path.DirectorySeparatorChar + sessionPrefix + Strings.PngExt);
        }

        private byte[] Pack(NanoPost[] posts)
        {
            return NanoPostPackUtil.Pack(posts);
        }
    }
}