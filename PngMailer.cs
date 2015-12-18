using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace nboard
{
    class PngMailer : IMailer
    {
        public void ReadInbox(NanoDB to)
        {
            if (Directory.Exists(Strings.Download))
            {
                string[] files = Directory.GetFiles(Strings.Download);
                var nanoCrypter = new NanoCrypter();

                foreach (string f in files)
                {
                    string pathToPng = f;
                    byte[] packed = new PngCrypter().Decrypt(pathToPng);
                    NanoPost[] posts = null;

                    try
                    {   
                        posts = nanoCrypter.Unpack(packed);
                    }
                    catch(Exception e)
                    {
                        Logger.Log(f);
                        Logger.Log(e.ToString());
                    }

                    if (posts != null)
                    {
                        foreach (var p in posts)
                        {
                            to.AddPost(p);
                        }
                    }
                }

                foreach (string f in files)
                {
                    File.Delete(f);
                }
            }
        }

        public void FillOutbox(NanoDB from)
        {
            if (!Directory.Exists(Strings.Upload))
            {
                Directory.CreateDirectory(Strings.Upload);
            }

            NanoPost[] posts = from.GetNewPosts();
            new CryptHelper().PutPostsToOutbox(posts, from);
            from.WritePosts();
        }
    }
}