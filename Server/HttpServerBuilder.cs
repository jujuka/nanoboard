using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using NDB;

namespace NServer
{
    class HttpServerBuilder
    {
        private readonly PostDb _db;

        public HttpServerBuilder(PostDb db)
        {
            _db = db;
        }

        public HttpServer Build()
        {
            string ip = Configurator.Instance.GetValue("ip", "127.0.0.1");
            int port = int.Parse(Configurator.Instance.GetValue("port", "7346"));
            var server = new HttpServer(ip, port);
            var pagesHandler = new FileHandler("pages", MimeType.Html);
            server.SetRootHandler(pagesHandler);
            server.AddHandler("api", new DbApiHandler(_db));
            server.AddHandler("pages", pagesHandler);
            server.AddHandler("scripts", new FileHandler("scripts", MimeType.Js));
            server.AddHandler("styles", new FileHandler("styles", MimeType.Css));
            server.AddHandler("images", new FileHandler("images", MimeType.Image, true));
            server.AddHandler("shutdown", new ActionHandler("Server was shut down.", ()=>server.Stop()));
            server.AddHandler("restart", new ActionHandler("Server was restarted.", ()=>{
                server.Stop();
                server = Build();
                server.Run();
            }));
            return server;
        }
    }
}
