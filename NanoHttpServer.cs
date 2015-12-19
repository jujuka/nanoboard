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
            _root = new StubHandler("Page not ready yet".ToHtmlBody());
        }

        public void AddRootHandler(IRequestHandler handler)
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
            var request = new NanoHttpRequest(connection.Request);
            if (request.Method == "GET" || request.Method == "POST") Process(connection, request);
            else connection.Response(
                new NanoHttpResponse(
                    StatusCode.MethodNotAllowed, 
                    (StatusCode.MethodNotAllowed.ToHeader(2) + "Server only supports GET and POST".ToPar()).ToHtmlBody()));
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
                connection.Response(
                    new NanoHttpResponse(StatusCode.BadRequest, (StatusCode.BadRequest.ToHeader(2) + "Unknown endpoint: " + endpoint).ToHtmlBody()));
            }
        }

        public void Stop()
        {
            _tcp.Stop();
        }
    }
}