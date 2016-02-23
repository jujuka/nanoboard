using System;
using NDB;
using System.Linq;
using NServer;

namespace NDB
{
    class MainClass
    {
        /*
            Application's entry point. 
            Builds server, assigns new DB instance to it and runs it;
        */
        public static void Main(string[] args)
        {
            var serv = new HttpServerBuilder(new PostDb()).Build();
            serv.Run();
        }
    }
}
