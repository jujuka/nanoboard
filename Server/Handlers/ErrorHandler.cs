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

namespace nboard
{
    class ErrorHandler : IRequestHandler
    {
        private readonly string _statusLine;
        private readonly string _description;

        public ErrorHandler(string statusLine, string description)
        {
            _statusLine = statusLine;
            _description = description;
        }

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            return new NanoHttpResponse(
                _statusLine, 
                (_statusLine.ToHeader(2) + _description.ToPar()).ToNoStyleHtmlBody());
        }
    }
}