using Newtonsoft.Json;
using NDB;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Text;

namespace NDB
{
    static class PostsValidator
    {
        public static bool Validate(Post p)
        {
            var bytes = Encoding.UTF8.GetBytes(p.message);

            if (bytes.Length > 65536)
            {
                return false;
            }

            p.hash = HashCalculator.Calculate(p.replyto + p.message);

            if (p.replyto.Length > 32)
                return false;

            foreach (var ch in p.replyto)
            {
                if (!((ch >= 'a' && ch <= 'f') || (ch >= '0' && ch <= '9')))
                {
                    return false;
                }
            }

            return true;
        }

        public static Post[] Validate(Post[] posts)
        {
            var res = new List<Post>();

            foreach (var p in posts)
            {
                if (Validate(p))
                {
                    res.Add(p);
                }
            }

            return res.ToArray();
        }
    }
}