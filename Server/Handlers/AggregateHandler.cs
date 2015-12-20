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
    class AggregateHandler : IRequestHandler
    {
        private readonly Aggregator _agg;

        public AggregateHandler()
        {
            _agg = new Aggregator();
        }

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            if (_agg.InProgress == 0)
            {
                _agg.Aggregate();
            }

            return new NanoHttpResponse(StatusCode.Ok, "<html><body onload='history.back()'></body></html>");
        }
    }
    
}