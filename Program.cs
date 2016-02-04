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
            var serv = new HttpServerBuilder(new PostDb()).Build();
            serv.Run();
        }
    }
}