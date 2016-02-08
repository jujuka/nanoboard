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
    class PngContainerCreatorNew
    {
        private int FreshPosts = 32;
        private int FreshPostsNotLimitedTo16384Allowed = 8;
        private int RandomPostsLimitedTo16384Allowed = 16;
        private int RandomPostsLimitedTo8192ALlowed = 64;

        private bool _smaller = false;

        static Random random = new Random();

        public PngContainerCreatorNew()
        {
        }

        private PngContainerCreatorNew(int scale)
        {
            FreshPosts /= scale;
            RandomPostsLimitedTo16384Allowed /= scale;
            _smaller = true;
        }

        public void SaveToPngContainer(NanoDB db)
        {
            db.RewriteDbExceptHidden();
            db.ClearDb();
            db.ReadPosts();

            string[] ext = new[] { ".png", ".jpg" };
            var files = new DirectoryInfo(Strings.Containers).GetFiles().Where(f => ext.Contains(f.Extension.ToLower())).ToArray();

            if (files.Length == 0)
            {
                NotificationHandler.Instance.AddNotification("Не найдены PNG файлы в папке containers.");
                return;
            }
            else if (files.Length <= 5)
            {
                if (!_smaller)
                {
                    NotificationHandler.Instance.AddNotification("Предупреждение: у вас мало контейнеров.");
                }
            }

            FileInfo file = files[random.Next(files.Length - 1)];
            var bmp = new Bitmap(file.FullName);
            int capacity = (int)(bmp.Width * bmp.Height * 3 / 8) - 32;
            string sessionPrefix = random.Next().ToString("x8");
            sessionPrefix += random.Next().ToString("x8");

            var packed = new byte[0];

            var posts = new List<NanoPost>();

            int i = db.GetPostCount() - 1;

            while (i >= 0 && posts.Count < FreshPosts)
            {
                var p = db.GetPost(i--);

                if (!db.IsHidden(p.GetHash()))
                {
                    posts.Add(p);
                }
            }

            var parr = posts.ToArray();

            var parents = new List<NanoPost>();

            foreach (var post in parr)
            {
                var p = db.Get(post.ReplyTo);

                if (p != null && !db.IsHidden(p.GetHash()))
                {
                    parents.Add(p);
                }
            }

            foreach (var post in parents)
            {
                posts.Add(post);
                var p = db.Get(post.ReplyTo);

                if (p != null && !db.IsHidden(p.GetHash()))
                {
                    posts.Add(p);
                }
            }

            var slice0 = posts.GetRange(0, Math.Max(posts.Count, FreshPostsNotLimitedTo16384Allowed));
            var slice1 = posts.GetRange(slice0.Count, posts.Count - slice0.Count);
            posts.Clear();
            posts.AddRange(slice0);
            posts.AddRange(slice1.ToArray().FilterBySize(16384));
            posts.AddRange(db.GetNRandomPosts(RandomPostsLimitedTo8192ALlowed).ExceptHidden(db).FilterBySize(8192));
            posts.AddRange(db.GetNRandomPosts(RandomPostsLimitedTo16384Allowed).ExceptHidden(db).FilterBySize(16384));
            posts = posts.Distinct().ToList();
            packed = NanoPostPackUtil.Pack(posts.ToArray());

            float scale = 1;

            if (packed.Length > capacity)
            {
                scale = (packed.Length / (float)capacity);
                scale = (float)Math.Sqrt(scale);

                if (scale > 2 && !_smaller)
                {   
                    new PngContainerCreatorNew(4).SaveToPngContainer(db);
                    return;
                }

                bmp = new Bitmap(bmp, (int) (bmp.Width * scale + 1), (int) (bmp.Height * scale + 1));
            }

            new PngStegoUtil().HideBytesInPng(
                        bmp, 
                        Strings.Upload + Path.DirectorySeparatorChar + sessionPrefix + Strings.PngExt, 
                        packed);

            Console.WriteLine(
                string.Format(
                    "PNG capacity:{0}, posts amount:{1}, packed size:{2}, image scaling: {3:n2}x", 
                        capacity, posts.Count, packed.Length, scale));

            Console.WriteLine("Total posts in db: {0}, post length limit (bytes): {1}", db.GetPostCount(), NanoPost.MaxPostByteLength);

            NotificationHandler.Instance.AddNotification("Контейнер сохранён: " + Strings.Upload + Path.DirectorySeparatorChar + sessionPrefix + Strings.PngExt);
        }
    }
    
}