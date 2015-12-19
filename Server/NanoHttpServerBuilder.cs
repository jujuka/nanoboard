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
        private readonly NanoDB _db;

        public NanoHttpServerBuilder(NanoDB db)
        {
            _db = db;
        }

        public NanoHttpServer Build(int port)
        {
            var server = new NanoHttpServer(port);
            server.SetRootHandler(new ThreadViewHandler(_db));
            server.AddHandler("thread", new ThreadViewHandler(_db));
            server.AddHandler("reply", new ReplyViewHandler(_db));
            server.AddHandler("write", new WriteHandler(_db));
            server.AddHandler("hide", new HideHandler(_db));
            return server;
        }
    }
}