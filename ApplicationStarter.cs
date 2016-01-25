using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace nboard
{
    class ApplicationStarter
    {
        public ApplicationStarter()
        {
            PrepareFolders();

            var db = new NanoDB();
            new DownloadCheckDaemon(db);
            db.ReadPosts();
            /*form.FormClosed += (object sender, FormClosedEventArgs e) => {
                daemon.Stop();
                new PngMailer().FillOutbox(db);
            };*/

            try
            {
                if (!File.Exists("port.txt"))
                {
                    File.WriteAllText("port.txt", "7345");
                }
            }
            catch
            {
                Logger.LogError("Cant write to port.txt");
            }

            int port = 0;
            try
            {
                int.TryParse(File.ReadAllText("port.txt"), out port);
            }
            catch
            {
                Logger.LogError("Error reading port.txt");
            }

            if (port <= 0)
            {
                port = 7345;
            }

            Console.WriteLine("Do not terminate manually, use shutdown action in web-interface.");
            var serv = new NanoHttpServerBuilder(db).Build(port);
            serv.Run();
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