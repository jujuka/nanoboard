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

            sb.Append("<br>Не применять сжатие:</br>");

            sb.Append(@"<form action=""convert"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать незримо:</br>");
            sb.Append(@"<form action=""compress/-90-90"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать слегка:</br>");
            sb.Append(@"<form action=""compress/-80-85"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать умеренно:</br>");
            sb.Append(@"<form action=""compress/-70-80"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать весьма:</br>");
            sb.Append(@"<form action=""compress/-60-75"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать нехило:</br>");
            sb.Append(@"<form action=""compress/-50-70"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать ощутимо:</br>");
            sb.Append(@"<form action=""compress/-45-65"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать основательно:</br>");
            sb.Append(@"<form action=""compress/-40-60"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать конкретно:</br>");
            sb.Append(@"<form action=""compress/-35-55"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать строго:</br>");
            sb.Append(@"<form action=""compress/-30-50"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать безжалостно:</br>");
            sb.Append(@"<form action=""compress/-25-45"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать бескомпромиссно:</br>");
            sb.Append(@"<form action=""compress/-20-40"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать сильно:</br>");
            sb.Append(@"<form action=""compress/-20-30"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать неистово:</br>");
            sb.Append(@"<form action=""compress/-15-25"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать безумно:</br>");
            sb.Append(@"<form action=""compress/-15-20"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать кошмарно:</br>");
            sb.Append(@"<form action=""compress/-10-15"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать чудовищно:</br>");
            sb.Append(@"<form action=""compress/-10-10"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            sb.Append("<br>Пожать бессмысленно:</br>");
            sb.Append(@"<form action=""compress/-5-10"" method=""post"" enctype=""multipart/form-data"">
                <input type=""file"" name=""file"" />
                <input type=""submit"" value=""convert"" />
            </form>");

            return new NanoHttpResponse(StatusCode.Ok, sb.ToString().ToNoStyleHtmlBody());
        }
    }
}