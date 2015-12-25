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
    class AsmPngHandler : IRequestHandler
    {
        private readonly NanoDB _db;

        public AsmPngHandler(NanoDB db)
        {
            _db = db;
        }

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            new PngMailer().FillOutbox(_db);
            return new NanoHttpResponse(StatusCode.Ok, "\n");
        }
    }
}