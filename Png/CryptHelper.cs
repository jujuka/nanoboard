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
    class CryptHelper
    {
        private const int Attempts = 10;
        static Random random = new Random();

        /*
            Also adds some random posts if space in PNG allows it.
        */
        public void PutPostsToOutbox(NanoPost[] posts, NanoDB db)
        {
            posts = posts.ExceptHidden(db);
            string sessionPrefix = random.Next().ToString("x8");
            sessionPrefix += random.Next().ToString("x8");
            byte[] packed = new NanoCrypter().Pack(posts);
            var files = new DirectoryInfo(Strings.Containers).GetFiles("*.png");

            if (files.Length == 0)
            {
                Logger.LogError("Containers folder contains no png files.");
                return;
            }

            for (int i = 0; i < Attempts; i++)
            {
                FileInfo file = files[random.Next(files.Length - 1)];
                var bmp = new Bitmap(file.FullName);
                int capacity = (int) (bmp.Width * bmp.Height * 3 / 8);

                if (packed.Length <= capacity)
                {
                    var newPosts = new List<NanoPost>(posts);
                    int postsToRequest = (int)(3f * (capacity - packed.Length) / (packed.Length / (1+(float)posts.Length)));
                    newPosts.AddRange(db.GetNLastPosts(postsToRequest/2).ExceptHidden(db));
                    newPosts.AddRange(db.GetNRandomPosts(postsToRequest/2).ExceptHidden(db));
                    byte[] newpacked = new NanoCrypter().Pack(newPosts.ToArray());
                    packed = newpacked;

                    new PngCrypter().Crypt(
                        file.FullName, 
                        Strings.Upload + Path.DirectorySeparatorChar + sessionPrefix + Strings.PngExt, 
                        packed);
                    Console.WriteLine(string.Format("Capacity: {0}, Packed: {1}, Count: {2}", capacity, packed.Length, newPosts.Count));
                    return;
                }
            }

            PutPostsToOutbox(posts.ToList().GetRange(0, posts.Length/2).ToArray(), db);
            PutPostsToOutbox(posts.ToList().GetRange(posts.Length/2, posts.Length - posts.Length/2).ToArray(), db);
        }
    }
}