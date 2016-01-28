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
    class AddPlaceHandler : IRequestHandler
    {
        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            try
            {
                var places = new List<string>(File.ReadAllLines(Strings.Places));
                var place = request.Address.Substring(5);

                bool found = true;

                if (!places.Contains(place))
                {
                    found = false;
                    places.Add(place);
                }

                File.WriteAllLines(Strings.Places, places.ToArray());
                return new NanoHttpResponse(StatusCode.Ok, (found?"Exists ":"Added ") + place);
            }

            catch (Exception e)
            {
                NotificationHandler.Instance.AddNotification("Не удалось обновить places.txt");
                return new ErrorHandler(StatusCode.InternalServerError, e.ToString()).Handle(request);
            }
        }
    }
}