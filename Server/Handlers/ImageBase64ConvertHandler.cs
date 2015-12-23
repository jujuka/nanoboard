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
    class ImageBase64ConvertHandler : IRequestHandler
    {
        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            var sb = new StringBuilder();
            //ThreadViewHandler.AddHeader(sb);

            sb.Append(@"<form action=""convert"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            return new NanoHttpResponse(StatusCode.Ok, sb.ToString().ToHtmlBody());
        }
    }
}