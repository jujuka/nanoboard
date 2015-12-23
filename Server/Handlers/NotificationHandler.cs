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
    class NotificationHandler : IRequestHandler
    {
        private static Queue<string> _messages = new Queue<string>();

        public void AddNotification(string message)
        {
            _messages.Enqueue(message);
        }

        #region IRequestHandler implementation

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            try
            {
                return new NanoHttpResponse(StatusCode.Ok, _messages.Dequeue());
            }

            catch
            {
                return new NanoHttpResponse(StatusCode.NotFound, "\n");
            }
        }

        #endregion

        public static readonly NotificationHandler Instance = new NotificationHandler();

        public NotificationHandler()
        {
        }
    }
}