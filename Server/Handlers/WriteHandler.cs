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
    class WriteHandler : IRequestHandler
    {
        static string ToMonthName(int i)
        {
            switch (i)
            {
                case 1: return "Jan";
                case 2: return "Feb";
                case 3: return "Mar";
                case 4: return "Apr";
                case 5: return "May";
                case 6: return "Jun";
                case 7: return "Jul";
                case 8: return "Aug";
                case 9: return "Sep";
                case 10: return "Oct";
                case 11: return "Nov";
                case 12: return "Dec";
            }

            return "ERR";
        }

        static string GetPostHeader()
        {
            var now = DateTime.UtcNow;
            return string.Format("{0:00}/{1}/{2}, {3:00}:{4:00}:{5:00} (UTC), client: nboard v" + App.Version, now.Day, ToMonthName(now.Month), now.Year, now.Hour, now.Minute, now.Second);
        }

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            if (string.IsNullOrEmpty(request.Content))
            {
                return new ErrorHandler(StatusCode.BadRequest, "Empty message").Handle(request);
            }

            string thread = request.Address.Split('/').Last();

            var str = Encoding.UTF8.GetString(request.Connection.Raw);
            str = str.Substring(str.IndexOf("\r\n\r\n") + 4);

            if (SpamDetector.IsSpam(str))
            {
                NotificationHandler.Instance.AddNotification("Ваш пост из-за своего содержания будет считаться спамом.");
                return new NanoHttpResponse(StatusCode.BadRequest, "");
            }
            else
            {
                //if (_db.AddPost(post))
                return new NanoHttpResponse(StatusCode.Ok, HashCalculator.Calculate(thread + str));
            }
        }
    }
}