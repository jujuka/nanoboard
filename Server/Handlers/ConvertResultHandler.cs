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
    class ConvertResultHandler : IRequestHandler
    {
        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            try
            {
                var sb = new StringBuilder();

                int offset = Encoding.ASCII.GetString(request.Connection.Raw).IndexOf("\r\n\r\n");
                offset = Encoding.ASCII.GetString(request.Connection.Raw).IndexOf("\r\n\r\n", offset + 4);
                int length = request.Connection.Raw.Length;
                sb.Append("[img=");
                sb.Append(Convert.ToBase64String(request.Connection.Raw, offset + 4, length - offset - 4));
                sb.Append("]");
                string prep = "";
                if (sb.Length > 16384) prep = "Превышен лимит в 16384 символа. Такой нанопост будет хуже ретранслироваться другими.\n";
                if (sb.Length > 32768) prep = "Превышен лимит в 32768 символов. Некоторые браузеры могут не отобразить.\n";
                if (sb.Length > 65000) prep = "Превышен лимит в 65000 символов. Это не влезет в нанопост.\n";
                return new NanoHttpResponse(StatusCode.Ok, prep + sb.ToString(), "text/plain; charset=utf-8");
            }

            catch
            {
                return new NanoHttpResponse(StatusCode.Ok, "Error");
            }
        }
    }
    
}