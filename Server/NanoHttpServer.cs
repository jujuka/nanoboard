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
    class NanoHttpServer
    {
        private readonly TcpServer _tcp;
        private readonly Dictionary<string,IRequestHandler> _handlers;
        private IRequestHandler _root;

        public NanoHttpServer(int port)
        {
            _handlers = new Dictionary<string, IRequestHandler>();
            _tcp = new TcpServer(port);
            _tcp.ConnectionAdded += OnConnectionAdded;
            _root = new StubHandler("Page not ready yet".ToNoStyleHtmlBody());
        }

        public void SetRootHandler(IRequestHandler handler)
        {
            _root = handler;   
        }

        public void AddHandler(string endpoint, IRequestHandler handler)
        {
            _handlers[endpoint] = handler;
        }

        public void Run()
        {
            _tcp.Run();
        }

        private void OnConnectionAdded(HttpConnection connection)
        {
            var request = new NanoHttpRequest(connection, connection.Request);
            if (request.Method == "GET" || request.Method == "POST") Process(connection, request);
            else connection.Response(new ErrorHandler(StatusCode.MethodNotAllowed, "Server only supports GET and POST").Handle(request));
        }

        private void Process(HttpConnection connection, NanoHttpRequest request)
        {
            if (request.Address == "/")
            {
                connection.Response(_root.Handle(request));
                return;
            }

            var endpoint = request.Address.Split(new char[]{'/'}, StringSplitOptions.RemoveEmptyEntries)[0];

            if (_handlers.ContainsKey(endpoint))
            {
                connection.Response(_handlers[endpoint].Handle(request));
            }

            else
            {
                connection.Response(new ErrorHandler(StatusCode.BadRequest, "Unknown endpoint: " + endpoint).Handle(request));
            }
        }

        public void Stop()
        {
            _tcp.Stop();
        }
    }
}