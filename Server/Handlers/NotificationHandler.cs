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
        private string _message;

        public void SetNotification(string message)
        {
            _message = message;
        }

        #region IRequestHandler implementation

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            return new NanoHttpResponse(StatusCode.Ok, _message);
        }

        #endregion

        public static readonly NotificationHandler Instance = new NotificationHandler();

        public NotificationHandler()
        {
        }
    }
}