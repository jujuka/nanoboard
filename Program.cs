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
        //[STAThread]
        public static void Main(string[] args)
        {
            new ApplicationStarter();
            /*
            new PngCrypter().Crypt("test.bmp", "test.bmp", new byte[]{13,17,19});
            var bytes = new PngCrypter().Decrypt("test.bmp");
            Console.WriteLine(bytes[0]);
            Console.WriteLine(bytes[1]);
            Console.WriteLine(bytes[2]);
            */
            /*
            var sbytes = Encoding.UTF8.GetBytes("001122");
            string str = Encoding.UTF8.GetString(sbytes);
            new SlowPngCrypter().Crypt("containers/dummy.png", "test.png", sbytes);
            var bytes = new SlowPngCrypter().Decrypt("test.png");
            Console.WriteLine(Encoding.UTF8.GetString(bytes, 0, 6));
            */
            //new ApplicationStarter();
            /*
            var nc = new NanoCrypter();
            var posts = new List<NanoPost>();
            posts.Add(new NanoPost(Hash.CreateZero(), "testТест1"));
            posts.Add(new NanoPost(Hash.CreateZero(), "testТест2"));
            byte[] packed = nc.Pack(posts.ToArray());
            NanoPost[] unpacked = nc.Unpack(packed);
            Console.WriteLine(unpacked[1].Message);
            return;
            */
            /*
            var db = new NanoDB();
            //db.ReadPosts();
            var post = NanoPost.Create("Example thread");
            db.AddPost(post);
            post = NanoPost.Reply(post.GetHash(), "First reply in thread");
            db.AddPost(post);
            post = NanoPost.Reply(post.GetHash(), "Second reply in thread");
            db.AddPost(post);
            //db.WritePosts();

            var form = new Form();
            new ThreadView(form, db).Update(db.GetThreadPosts(db.Threads[0]));
            Application.Run(form);
            */
        }
    }
}