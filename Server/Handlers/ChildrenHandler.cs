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
    class ChildrenHandler : IRequestHandler
    {
        private readonly NanoDB _db;
        private readonly bool _expanded;

        public ChildrenHandler(NanoDB db, bool expanded)
        {
            _db = db;
            _expanded = expanded;
        }

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            try
            {
                return HandleSafe(request);
            }

            catch (Exception e)
            {
                return new ErrorHandler(StatusCode.InternalServerError, e.ToString().Replace("\n", "<br>")).Handle(request);
            }
        }

        private NanoHttpResponse HandleSafe(NanoHttpRequest request)
        {
            Hash thread = new Hash(request.Address.Split('/').Last());

            if (thread.Invalid)
            {
                return new ErrorHandler(StatusCode.BadRequest, "Wrong hash format.").Handle(request);
            }

            var sb = new StringBuilder();

            sb.Append("{ \n    \"posts\" : [\n");

            var posts = _expanded ? _db.GetExpandedThreadPosts(thread) : _db.GetThreadPosts(thread);

            for (int i = 0; i < posts.Length; i++)
            {
                var p = posts[i];
                bool last = i == posts.Length - 1;
                sb.Append("    {\n        \"hash\" :    \"");
                sb.Append(p.GetHash().Value);
                sb.Append("\", \n        \"isHidden\" : \"");
                sb.Append(_db.IsHidden(p.GetHash()) ? "1" : "0");
                sb.Append("\", \n        \"replyTo\" : \"");
                sb.Append(p.ReplyTo.Value);
                sb.Append("\", \n        \"message\" : \"");

                string s = p.SerializedString().Substring(32);
                s = s.Replace("\\", "\\\\");
                s = s.Replace("\n", "\\n");
                s = s.Replace("\"", "\\\"");
                s = s.Replace("\t", "\\t");
                s = s.Replace("\r", "\\r");
                sb.Append(s);
                sb.Append("\"\n    }");
                if (!last) sb.Append(",\n");
            }

            sb.Append("\n    ]\n}");

            return new NanoHttpResponse(StatusCode.Ok, sb.ToString(), "application/json; charset=utf-8");
        }    
    }
}