using System;
using NDB;
using System.Linq;
using NServer;

namespace NDB
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            // jsfiddle.net/handtrix/xztfbx1m/72
            var serv = new HttpServerBuilder(new PostDb()).Build();
            serv.Run();
        }
    }
}