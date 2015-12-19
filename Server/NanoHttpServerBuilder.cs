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
    class NanoHttpServerBuilder
    {
        public NanoHttpServer Build(int port)
        {
            return new NanoHttpServer(port);
        }
    }
    
}