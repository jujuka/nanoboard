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

    class ReplyViewHandler : IRequestHandler
    {
        private readonly NanoDB _db;

        public ReplyViewHandler(NanoDB db)
        {
            _db = db;
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
            ThreadViewHandler.AddHeader(sb);
            var p = _db.Get(thread);

                sb.Append(
                    (
                        p.Message.Strip(true).Replace("\n", "<br/>").ToDiv("postinner", p.GetHash().Value)
                    ).ToDiv("post", ""));
                sb.Append(((/*">" + p.Message.StripInput().Replace("\n", "\n>") + "\n"*/"").ToTextArea("", "reply").AddBreak() +
                ("Отправить".ToButton("", "sendbtn", @"
                    var x = new XMLHttpRequest();
                    x.open('POST', '../write/"+p.GetHash().Value+@"', true);
                    x.send(document.getElementById('reply').value);
                    location.replace('/expand/" + p.GetHash().Value + @"');
                "))).ToDiv("post", ""));

            return new NanoHttpResponse(StatusCode.Ok, sb.ToString().ToHtmlBody());
        }
    }
}