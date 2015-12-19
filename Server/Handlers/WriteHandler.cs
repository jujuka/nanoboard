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
    class WriteHandler : IRequestHandler
    {
        private readonly NanoDB _db;

        public WriteHandler(NanoDB db)
        {
            _db = db;
        }

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            if (string.IsNullOrEmpty(request.Content))
            {
                return new ErrorHandler(StatusCode.BadRequest, "Empty message").Handle(request);
            }

            Hash thread = new Hash(request.Address.Split('/').Last());

            if (thread.Invalid)
            {
                return new ErrorHandler(StatusCode.BadRequest, "Invalid hash").Handle(request);
            }

            _db.AddPost(new NanoPost(thread, request.Content));
            return new NanoHttpResponse(StatusCode.Ok, "");
        }
    }
}