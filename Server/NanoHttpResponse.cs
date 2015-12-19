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
    class NanoHttpResponse
    {
        private readonly string _response;
        private readonly string _content;

        public NanoHttpResponse(string code, string content)
        {
            _response = "HTTP/1.1 " + code + (string.IsNullOrEmpty(content) ? "\r\n" : ("\r\n\r\n"));
            _content = content;
        }

        public string GetResponse()
        {
            return _response;
        }

        public string GetContent()
        {
            return _content;
        }
    }
}