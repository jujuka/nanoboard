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

        public NanoHttpServer(int port)
        {
            _tcp = new TcpServer(port);
            _tcp.ConnectionAdded += OnConnectionAdded;
            _tcp.Run();
        }

        private void OnConnectionAdded(HttpConnection connection)
        {
            Console.WriteLine("Connection added");
            var request = new NanoHttpRequest(connection.Request);
            if (request.Method == "GET") ProcessGet(connection, request.Address);
            else if (request.Method == "POST") ProcessPost(connection, request.Address, request.Content);
            else connection.Response(
                new NanoHttpResponse(
                    StatusCode.MethodNotAllowed, 
                    (StatusCode.MethodNotAllowed.ToHeader(2) + "Server only supports GET and POST".ToPar()).ToHtmlBody()).ToString());
        }

        private void ProcessGet(HttpConnection connection, string address)
        {
        }

        private void ProcessPost(HttpConnection connection, string address, string content)
        {
        }

        public void Stop()
        {
            _tcp.Stop();
        }
    }
}