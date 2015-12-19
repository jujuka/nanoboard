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
    class HideHandler : IRequestHandler
    {
        private readonly NanoDB _db;

        public HideHandler(NanoDB db)
        {
            _db = db;
        }

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            Hash hash = new Hash(request.Address.Split('/').Last());

            if (hash.Invalid)
            {
                return new ErrorHandler(StatusCode.BadRequest, "Invalid hash").Handle(request);
            }

            try
            {
                _db.Hide(hash);
                return new NanoHttpResponse(StatusCode.Ok, "");
            }

            catch
            {
                return new ErrorHandler(StatusCode.InternalServerError, "").Handle(request);
            }
        }
    }
    
}