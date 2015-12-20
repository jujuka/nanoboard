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
using System.Linq;

namespace nboard
{
    class SaveHandler : IRequestHandler
    {
        private readonly NanoDB _db;

        public SaveHandler(NanoDB db)
        {
            _db = db;
        }

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            _db.WritePosts(false);
            return new NanoHttpResponse(StatusCode.Ok, "");
        }
    }
}