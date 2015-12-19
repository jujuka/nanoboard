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
    class ThreadViewHandler : IRequestHandler
    {
        private NanoDB _db;
        private Hash _thread;

        public ThreadViewHandler(NanoDB db, Hash thread)
        {
            _db = db;
            _thread = thread;
        }

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            var sb = new StringBuilder();
            var posts = _db.GetThreadPosts(_thread);

            foreach (var p in posts)
            {
                sb.Append(p.Message.ToPar());
                sb.Append("<hr/>");
            }

            return new NanoHttpResponse(StatusCode.Ok, sb.ToString().ToHtmlBody());
        }
    }
}