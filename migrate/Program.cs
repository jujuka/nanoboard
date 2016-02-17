using System;
using System.IO;
using System.Text;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace migrate
{
    class Post
    {
        [JsonProperty("message")]
        public string message;
        [JsonProperty("replyTo")]
        public string replyto;
    }

    class MainClass
    {
        private const string Index = "index.db";
        private const string Data = "data.db";

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Pass nanodb (2.0 server address) as first argument");
                Console.WriteLine("Example (Linux/OSX):");
                Console.WriteLine("mono migrate.exe http://127.0.0.1:7346");
                Console.WriteLine("Example (Windows):");
                Console.WriteLine("migrate http://127.0.0.1:7346");
                Console.WriteLine("nanodb should be running!");
            }

            if (!File.Exists(Index) || !File.Exists(Data))
            {
                Console.WriteLine("Please put index.db and data.db here.");
                return;
            }

            string indexes = Encoding.UTF8.GetString(File.ReadAllBytes(Index));
            string posts = Encoding.UTF8.GetString(File.ReadAllBytes(Data));

            try
            {
                var list = new List<Post>();
                for (int i = 0; i < indexes.Length / 8; i += 2)
                {
                    string offset = indexes.Substring(i * 8, 8);
                    string length = indexes.Substring(i * 8 + 8, 8);
                    string rawpost = posts.Substring(
                                     int.Parse(offset, System.Globalization.NumberStyles.HexNumber), 
                                     int.Parse(length, System.Globalization.NumberStyles.HexNumber));
                    var post = new Post { replyto = rawpost.Substring(0,32), message = rawpost.Substring(32) };
                    list.Add(post);
                }

                var wc = new WebClient();
                wc.UploadData(new Uri(args[0].Trim('/') + "/api/addmany"), 
                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(list.ToArray())));
            }

            catch
            {
                Console.WriteLine("Fail");
                return;
            }

            Console.WriteLine("Success");
        }
    }
}
