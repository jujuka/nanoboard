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
    class CompressImageHandler : IRequestHandler
    {
        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            try
            {
                var @params = request.Address.Split('-');
                int q = int.Parse(@params[1]);
                int s = int.Parse(@params[2]);

                var sb = new StringBuilder();

                int offset = Encoding.ASCII.GetString(request.Connection.Raw).IndexOf("\r\n\r\n");
                offset = Encoding.ASCII.GetString(request.Connection.Raw).IndexOf("\r\n\r\n", offset + 4);
                int length = request.Connection.Raw.Length;

                int stepBack = 2; // multipart ending
                int dashes = 0;

                for (int i = request.Connection.Raw.Length - 1; i > 0; i--)
                {
                    stepBack += 1;
                    byte b = request.Connection.Raw[i];
                    char c = (char)b;

                    if (c == '-')
                    {
                        dashes += 1;

                        if (dashes == 31)
                        {
                            break;
                        }
                    }
                }

                byte[] slice = new byte[length - offset - 4];

                for (int i = 0; i < length - offset - 4; i++)
                {
                    slice[i] = request.Connection.Raw[i + offset + 4];
                }

                slice = ImageCompressor.Compress(slice, q, s/100.0f);
                sb.Append("<img src='data:image/jpg;base64,");
                sb.Append(Convert.ToBase64String(slice, 0, slice.Length));
                sb.Append("' >");
                string prep = "";
                bool tooBig = sb.Length > 64512;
                if (sb.Length > 16384) prep = "Превышен лимит в 16384 символа. Такой нанопост будет хуже ретранслироваться другими.\n";
                if (tooBig) prep = "Превышен лимит в 64512 символов. Такая картикна не отобразится.\n";
                prep += string.Format("Размер: {0}, base64: {1}", slice.Length, sb.Length);
                prep += "<br>";

                sb.Append("<br><br>Triple-click, Cmd/Ctrl+C<br><div style='font-size:25%'>[img=");
                sb.Append(Convert.ToBase64String(slice, 0, slice.Length));
                sb.Append("]");
                sb.Append("</div><br>");

                if (tooBig) sb.Clear(); // show nothing if it's invalid anyway

                return new NanoHttpResponse(StatusCode.Ok, prep + sb.ToString(), "text/html; charset=utf-8");
            }

            catch (Exception e)
            {
                return new NanoHttpResponse(StatusCode.Ok, "Error" + e.ToString(), "text/html; charset=utf-8");
            }
        }
    }
    
}