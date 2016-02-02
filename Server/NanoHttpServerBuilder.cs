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
            var server = new NanoHttpServer(port);
            //server.SetRootHandler(new ThreadViewHandler(_db));
            server.AddHandler("write", new WriteHandler());
            server.AddHandler("rawpost", new RawPostHandler());
            server.AddHandler("shutdown", new ShutdownHandler(server));
            server.AddHandler("status", new NotificationHandler());
            server.AddHandler("image", new ImageBase64ConvertHandler());
            server.AddHandler("convert", new ConvertResultHandler());
            server.AddHandler("compress", new CompressImageHandler());
            return server;
        }
    }
}