using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace nboard
{
    static class NanoPostPackUtil
    {
        public static byte[] Pack(NanoPost[] posts)
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(Encoding.UTF8.GetBytes(posts.Length.ToString("x6")));

            foreach (var p in posts)
            {
                var len = p.SerializedBytes().Length;
                bytes.AddRange(Encoding.UTF8.GetBytes(len.ToString("x6")));
            }

            foreach (var p in posts)
            {
                bytes.AddRange(p.SerializedBytes());
            }

            return GZipUtil.Compress(bytes.ToArray());
        }

        public static NanoPost[] Unpack(byte[] bytes)
        {
            bytes = GZipUtil.Decompress(bytes);
            List<NanoPost> posts = new List<NanoPost>();
            string str = Encoding.UTF8.GetString(bytes);

            int count = int.Parse(str.Substring(0, 6), System.Globalization.NumberStyles.HexNumber);
            List<int> sizes = new List<int>();
            List<string> raws = new List<string>();

            for (int i = 0; i < count; i++)
            {
                int size = int.Parse(str.Substring((i+1)*6, 6), System.Globalization.NumberStyles.HexNumber);
                sizes.Add(size);
            }

            int offset = count * 6 + 6;

            for (int i = 0; i < sizes.Count; i++)
            {
                int size = sizes[i];
                raws.Add(str.Substring(offset, size));
                offset += size;
            }

            for (int i = 0; i < raws.Count; i++)
            {
                posts.Add(new NanoPost(raws[i]));
            }

            return posts.ToArray();
        }
    }
    
}