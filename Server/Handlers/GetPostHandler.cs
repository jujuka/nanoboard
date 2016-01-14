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
    class GetPostHandler : IRequestHandler
    {
        private readonly NanoDB _db;

        public GetPostHandler(NanoDB db)
        {
            _db = db;
        }

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            try
            {
                return HandleSafe(request);
            }

            catch
            {
                return new ErrorHandler(StatusCode.InternalServerError, "").Handle(request);
            }
        }

        private NanoHttpResponse HandleSafe(NanoHttpRequest request)
        {
            int n = int.Parse(request.Address.Split('/').Last());

            var sb = new StringBuilder();
            var p = _db.GetPost(n);

            sb.Append("{\n    \"hash\" :    \"");
            sb.Append(p.GetHash().Value);
            sb.Append("\", \n    \"isHidden\" : \"");
            sb.Append(_db.IsHidden(p.GetHash()) ? "1" : "0");
            sb.Append("\", \n    \"replyTo\" : \"");
            sb.Append(p.ReplyTo.Value);
            sb.Append("\", \n    \"message\" : \"");

            string s = p.SerializedString().Substring(32);
            s = s.Replace("\\", "\\\\");
            s = s.Replace("\n", "\\n");
            s = s.Replace("\"", "\\\"");
            s = s.Replace("\t", "\\t");
            s = s.Replace("\r", "\\r");
            sb.Append(s);
            sb.Append("\"\n}");

            return new NanoHttpResponse(StatusCode.Ok, sb.ToString(), "application/json; charset=utf-8");
        }
    }
    
}