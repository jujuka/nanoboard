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
                return new NanoHttpResponse(StatusCode.Ok, sb.ToString());
            }

            catch
            {
                return new NanoHttpResponse(StatusCode.Ok, "Error");
            }
        }
    }
    
}