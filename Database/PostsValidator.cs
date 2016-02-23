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
        /*
            This extension methods are used everywhere to convert Post.message from Base64 string to real text
        */
        public static string FromB64(this string s)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(s));
        }

        /*
            See above. Inverse operation.
        */
        public static string ToB64(this string s)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
        }

        /*
            Rewrites post hash.
            Checks if post meets rules:
                max message size in utf-8 bytes is 65536,
                replyTo should be valid hash
            returns false if rules violated
        */
        public static bool Validate(Post p)
        {
            var bytes = Encoding.UTF8.GetBytes(p.message.FromB64());

            if (bytes.Length > 65536)
            {
                return false;
            }

            p.hash = HashCalculator.Calculate(p.replyto + p.message.FromB64());

            if (p.replyto.Length != 32)
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

        /*
            Validates range of posts in a batch.
        */
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
