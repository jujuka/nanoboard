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

            _agg.ProgressChanged += () => 
            {
                if (_agg.InProgress == 0)
                {
                    NotificationHandler.Instance.AddNotification("Поиск сообщений завершен.");
                }
            };
        }

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            if (_agg.InProgress <= 0)
            {
                NotificationHandler.Instance.AddNotification("Начат поиск сообщений.");
                _agg.Aggregate();
            }

            else
            {
                NotificationHandler.Instance.AddNotification("Повторно начат поиск сообщений.");
                _agg.Aggregate();
            }

            return new NanoHttpResponse(StatusCode.Ok, "<html><body onload='history.back()'></body></html>");
        }
    }
    
}