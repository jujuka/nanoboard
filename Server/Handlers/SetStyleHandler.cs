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
    class SetStyleHandler : IRequestHandler
    {
        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            var name = request.Content;

            if (name != " ")
            {

                try
                {
                    var filename = "styles/" + name.Replace(".css", "") + ".css";
                    var style = File.ReadAllText(filename);
                    HtmlStringExtensions.Style = style;
                    File.WriteAllText("setstyle.txt", filename);
                }
                catch
                {
                }
            }

            return new NanoHttpResponse(StatusCode.Ok, "");
        }
    }
    
}