using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace nboard
{
    static class NanoPostExtensions
    {
        static Random random = new Random();

        public static int EvaluateSize(this List<NanoPost> posts)
        {
            int size = 6;
            size += posts.Count * 6;

            foreach (var p in posts)
            {
                size += p.Message.Length + p.ReplyTo.Value.Length;
            }

            return size;
        }

        public static NanoPost[] Randomized(this NanoPost[] posts)
        {
            var list = new List<NanoPost>(posts);
            var newl = new List<NanoPost>();

            while (list.Count > 0)
            {
                int i = random.Next(list.Count-1);
                newl.Add(list[i]);
                list.RemoveAt(i);
            }

            return newl.ToArray();
        }

        public static NanoPost[] ExceptHidden(this NanoPost[] posts, NanoDB db)
        {
            if (db.IsHiddenListEmpty()) return posts;
            return posts.Where(p => !db.IsHidden(p.GetHash())).ToArray();
        }

        public static NanoPost[] Sorted(this NanoPost[] posts)
        {
            var list = new List<NanoPost>();

            for (int i = 0; i < posts.Length; i++)
            {
                NanoPost post = posts[i];

                for (int x = 0; x < posts.Length; x++)
                {
                    if (x == i)
                    {
                        continue;
                    }

                    NanoPost reply = posts[x];

                    if (reply.ReplyTo == post.GetHash())
                    {
                        if (!list.Contains(post))
                        {
                            list.Add(post);
                        }

                        list.Add(reply);
                    }
                }
            }

            var added = new HashSet<NanoPost>(list);

            foreach (var post in posts)
            {
                if (!added.Contains(post))
                {
                    list.Add(post);
                }
            }

            return list.ToArray();
        }
    }
}