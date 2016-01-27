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
            server.AddHandler("expand", new ThreadViewHandler(_db, true));
            server.AddHandler("reply", new ReplyViewHandler(_db));
            server.AddHandler("write", new WriteHandler(_db));
            server.AddHandler("hide", new HideHandler(_db));
            server.AddHandler("hideall", new HideAllHandler(_db));
            server.AddHandler("save", new SaveHandler(_db));
            server.AddHandler("fresh", new FreshPostsHandlder(_db));
            server.AddHandler("asmpng", new AsmPngHandler(_db));
            server.AddHandler("rawpost", new RawPostHandler(_db));
            server.AddHandler("postcount", new PostCountHandler(_db));
            server.AddHandler("lastposts", new LastPostsHandler(_db));
            server.AddHandler("getpost", new GetPostHandler(_db));
            server.AddHandler("children", new ChildrenHandler(_db, false));
            server.AddHandler("allchildren", new ChildrenHandler(_db, true));
            server.AddHandler("aggregate", new AggregateHandler());
            server.AddHandler("shutdown", new ShutdownHandler(server, _db));
            server.AddHandler("status", new NotificationHandler());
            server.AddHandler("image", new ImageBase64ConvertHandler());
            server.AddHandler("convert", new ConvertResultHandler());
            server.AddHandler("setstyle", new SetStyleHandler());
            server.AddHandler("compress", new CompressImageHandler());
            return server;
        }
    }
}