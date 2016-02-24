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
        private readonly NanoDB _db;

        public PngContainerCreator(NanoDB db)
        {
            _db = db;
        }

        private static bool ByteCountUnder(List<NanoPost> posts, int limit)
        {
            int byteCount = 0;

            foreach (var p in posts)
            {
                byteCount += p.Message.Length + 32;
                if (byteCount > limit) return false;
            }

            return true;
        }

        private static int ByteCount(NanoPost p)
        {
            return p.Message.Length + 32;
        }

        /*
            Takes 50 or less last posts (up to 150000 bytes max total),
            adds  50 or less random posts (up to 150000 bytes max total),
            random is shifted towards latest posts.
        */
        public void Create()
        {
            var count = _db.GetPostCount();
            var take = 50;
            var last50s = _db.GetNLastPosts(take * 4).ExceptHidden(_db).Reverse().Take(take).ToArray();
            var list = new List<NanoPost>(last50s);

            while (!ByteCountUnder(list, 150000))
            {
                list.RemoveAt(list.Count - 1);
            }

            var r = new Random();
            int rbytes = 0;
            int max = 50;

            for (int i = 0; i < max; i++)
            {
                int index = (int)Math.Min(Math.Pow(r.NextDouble(), 0.3) * count, count - 1);
                var p = _db.GetPost(index);
                if (_db.IsHidden(p.GetHash()))
                {
                    if (max < 10000)
                        max += 1;
                    continue;
                }
                var bc = ByteCount(p);
                if (rbytes + bc > 150000)
                    break;
                rbytes += bc;
                list.Add(p);
            }

            string[] ext = new[] { ".png", ".jpg", ".jpeg" };
            var files = new DirectoryInfo(Strings.Containers).GetFiles().Where(f => ext.Contains(f.Extension.ToLower())).ToArray();


            if (files.Length == 0)
            {
                NotificationHandler.Instance.AddNotification("Не найдены PNG файлы в папке containers.");
                return;
            }
            else if (files.Length <= 5)
            {
                NotificationHandler.Instance.AddNotification("Предупреждение: у вас мало контейнеров.");
            }

            string sessionPrefix = r.Next().ToString("x8");
            sessionPrefix += r.Next().ToString("x8");

            foreach (var p in list)
            {
                NotificationHandler.Instance.AddNotification(p.Message.Strip());
            }

            var file = files[r.Next(files.Length)];
            Pack(list.ToArray(), file.FullName, "upload/" + sessionPrefix + ".png");
        }

        private static void Pack(NanoPost[] arr, string templatePath, string outputPath)
        {
            var packed = NanoPostPackUtil.Pack(arr);
            var bmp = Bitmap.FromFile(templatePath);
            var capacity = (bmp.Width * bmp.Height * 3) / 8 - 32; // 32 is for header with hidden bytes count

            float scale = 1;
            if (packed.Length > capacity)
            {
                scale = (packed.Length / (float)capacity);
                scale = (float)Math.Sqrt(scale);
                Console.WriteLine("Warning: scaling image to increase capacity: " + scale.ToString("n2") + "x");
                bmp = new Bitmap(bmp, (int) (bmp.Width * scale + 1), (int) (bmp.Height * scale + 1));
            }

            new PngStegoUtil().HideBytesInPng(bmp, outputPath, packed);
            NotificationHandler.Instance.AddNotification("Контейнер сохранён: " + outputPath);
            Console.WriteLine(
                string.Format(
                    "PNG capacity:{0}, posts amount:{1}, packed size:{2}, image scaling: {3:n2}x", 
                        capacity, arr.Length, packed.Length, scale));

        }
    }


    class PngContainerCreatorNew
    {
        public PngContainerCreatorNew()
        {
        }

        public void SaveToPngContainer(NanoDB db)
        {
            NotificationHandler.Instance.AddNotification("Перезапись базы, подождите...");
            db.RewriteDbExceptHidden();
            db.ClearDb();
            db.ReadPosts();
            new PngContainerCreator(db).Create();
            return;
        }
    }
    
}