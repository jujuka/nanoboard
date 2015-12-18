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

    class ApplicationStarter
    {
        public ApplicationStarter()
        {
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            PrepareFolders();

            var db = new NanoDB();
            var daemon = new DownloadCheckDaemon(db);
            db.ReadPosts();
            /*var post = NanoPost.Create("Example thread");
            db.AddPost(post);
            post = NanoPost.Reply(post.GetHash(), "First example reply in thread");
            db.AddPost(post);
            post = NanoPost.Reply(post.GetHash(), "Second example reply in thread");
            db.AddPost(post);
            */
            var form = new Form();
            form.FormClosed += (object sender, FormClosedEventArgs e) => {
                daemon.Stop();
                new PngMailer().FillOutbox(db);
            };
            new UI(form, db).Update(db.GetThreadPosts(db.Threads[0]));
            Application.Run(form);
        }

        private void PrepareFolders()
        {
            if (!Directory.Exists(Strings.Containers))
            {
                Directory.CreateDirectory(Strings.Containers);
                string file = Strings.Containers + Path.DirectorySeparatorChar + "dummy" + Strings.PngExt;
                Bitmap bmp = new Bitmap(512, 512);
                bmp.Save(file, System.Drawing.Imaging.ImageFormat.Png);
            }
            else if (new DirectoryInfo(Strings.Containers).GetFiles().Length == 0)
            {
                string file = Strings.Containers + Path.DirectorySeparatorChar + "dummy" + Strings.PngExt;
                Bitmap bmp = new Bitmap(512, 512);
                bmp.Save(file, System.Drawing.Imaging.ImageFormat.Png);
            }

            if (!Directory.Exists(Strings.Upload))
            {
                Directory.CreateDirectory(Strings.Upload);
            }

            if (!Directory.Exists(Strings.Download))
            {
                Directory.CreateDirectory(Strings.Download);
            }
        }
    }
    
}