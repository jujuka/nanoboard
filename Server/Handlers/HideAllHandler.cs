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
    class HideAllHandler : IRequestHandler
    {
        private readonly NanoDB _db;

        public HideAllHandler(NanoDB db)
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
                var tag = _db.Get(hash).ContainerTag;

                for (int i = 0; i < _db.GetPostCount(); i++)
                {
                    var p = _db.GetPost(i);

                    if (p.ContainerTag != null && p.ContainerTag.Equals(tag))
                    {
                        _db.HideOnce(p.GetHash());
                    }
                }

                return new NanoHttpResponse(StatusCode.Ok, "\n");
            }

            catch (Exception e)
            {
                return new ErrorHandler(StatusCode.InternalServerError, e.ToString().Replace("\n", "<br>")).Handle(request);
            }
        }
    }
    
}