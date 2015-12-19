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
    class StubHandler : IRequestHandler
    {
        private readonly string _stub;

        public StubHandler(string stub)
        {
            _stub = stub;
        }

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            return new NanoHttpResponse(StatusCode.Ok, _stub);
        }
    }
    
}