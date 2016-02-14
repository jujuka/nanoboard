using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace NServer
{
    class TcpServer
    {
        private TcpListener _server;
        private Boolean _isRunning;

        public event Action<HttpConnection> ConnectionAdded;

        public TcpServer(string ip, int port)
        {
            Console.WriteLine("Listening on port " + port);
            Console.WriteLine("You can change port in port.txt file, ip in ip.txt");

            if (ip == "localhost")
            {
                IPAddress ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
                Console.WriteLine(ipAddress.ToString() + ":" + port);
                IPEndPoint ipLocalEndPoint = new IPEndPoint(ipAddress, port);
                _server = new TcpListener(ipLocalEndPoint);
            }
                
            else
            {
                _server = new TcpListener(IPAddress.Parse(ip), port);
            }
        }

        public void Run()
        {
            _server.Start();
            _isRunning = true;
            LoopClients();
        }

        public void Stop()
        {
            Console.WriteLine("Server was shut down");
            _isRunning = false;
        }

        private void LoopClients()
        {
            while (_isRunning)
            {
                if (!_server.Pending())
                {
                    Thread.Sleep(100);
                    continue;
                }

                try
                {
                    TcpClient newClient = _server.AcceptTcpClient();
                    Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                    t.Start(newClient);
                }
                catch
                {
                }
            }
        }

        private void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            var stream = client.GetStream();
            String readData = "";
            stream.ReadTimeout = 100;
            var buffer = new byte[1024];
            int len = 0;
            List<byte> raw = new List<byte>();

            try
            {
                do
                {
                    len = stream.Read(buffer, 0, buffer.Length);
                    var block = System.Text.Encoding.UTF8.GetString(buffer, 0, len);
                    readData += block;

                    for (int i = 0; i < len; i++) raw.Add(buffer[i]);
                }
                while (len > 0);
            }

            catch (IOException)
            {
                // that's ok, we've reached end of the stream (read timeout)
            }

            if (ConnectionAdded != null)
            {
                ConnectionAdded(new HttpConnection(raw.ToArray(), readData, (ascii,utf8) =>
                {
                    byte[] ba = Encoding.ASCII.GetBytes(ascii);
                    byte[] bu = Encoding.UTF8.GetBytes(utf8);
                    try{
                    stream.Write(ba, 0, ba.Length);
                    stream.Write(bu, 0, bu.Length);
                    stream.Flush();
                    }catch{}finally{
                    client.Close();
                    }
                }, (ascii,bytes) => 
                {
                    byte[] ba = Encoding.ASCII.GetBytes(ascii);
                    byte[] bu = bytes;
                    try{
                    stream.Write(ba, 0, ba.Length);
                    stream.Write(bu, 0, bu.Length);
                    stream.Flush();
                    }catch{}finally{
                    client.Close();
                    }
                }));
            }
        }
    }
}