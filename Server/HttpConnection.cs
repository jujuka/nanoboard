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

    class HttpConnection
    {
        public readonly string Request;
        private Action<string,string> _callback;
        public readonly byte[] Raw;

        public HttpConnection(byte[] raw, string request, Action<string,string> asciiUtf8callback)
        {
            this.Raw = raw;
            this.Request = request;
            this._callback = asciiUtf8callback;
        }

        public void Response(NanoHttpResponse response)
        {
            _callback(response.GetResponse(), response.GetContent());
        }
    }
}