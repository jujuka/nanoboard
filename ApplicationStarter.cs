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
            var serv = new NanoHttpServerBuilder().Build(port);
            serv.Run();
        }
    }
    
}