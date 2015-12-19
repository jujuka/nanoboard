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
        private Action<string> _callback;

        public HttpConnection(string request, Action<string> callback)
        {
            this.Request = request;
            this._callback = callback;
        }

        public void Response(NanoHttpResponse repsonse)
        {
            _callback(repsonse.ToString());
        }

        [Obsolete]
        public void Response(string repsonse)
        {
            _callback(repsonse);
        }
    }
    
}