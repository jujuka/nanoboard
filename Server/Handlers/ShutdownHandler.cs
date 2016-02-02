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
    class ShutdownHandler : IRequestHandler
    {
        private readonly NanoHttpServer _server;

        public ShutdownHandler(NanoHttpServer server)
        {
            _server = server;
        }

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            _server.Stop();
            return new ErrorHandler(StatusCode.Ok, "Сервер выключен, данные автоматически сохранены, посты, помеченные на удаление навсегда удалены из базы.").Handle(request);
        }
    }
}