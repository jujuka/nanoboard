using System;
using System.IO;
using Newtonsoft.Json;
using nboard;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Net;
using System.Text;
using System.Threading;

namespace nbpack
{
    public class NBPackMain
    {
        public static void Main_(string[] args)
        {
            if (!Directory.Exists("upload"))
                Directory.CreateDirectory("upload");
            if (!Directory.Exists("download"))
                Directory.CreateDirectory("download");
            if (!Directory.Exists("containers"))
                Directory.CreateDirectory("containers");

            if (args.Length <= 2)
            {
                Console.WriteLine(@"Shitty tool that can pack/unpack png containers of 1.x nanoboard format,
also can talk to 2.0 server to: 
    1) feed posts from containers (download folder) to 2.0
    2) extract posts from 2.0 to form a container
This is the main intent of this tool - to help 2.0 to use transport 
compatible with 1.x nanoboard PNG containers format.

Example usages:
    nbpack -g http://127.0.0.1:7346 nano
        (creates PNG container using random picture from containers folder
         and puts result into upload folder)
    nbpack -a http://127.0.0.1:7346 nano
        (for each picture from download folder, tries to unpack it as a
         container, then sends it's posts to the 2.0 server and deletes it)
Other usages (may be useful if you're developing your own client):
    nbpack -v posts.json output.json                         (rewrite hashes)
    nbpack -p posts.json template.png crypto_key output.png  (pack container)
    nbpack -u container.png crypto_key output.json           (unpack container)
Sample JSON (note that message contains utf-8 BYTES converted to base64 string) 
 { ""posts"" : [ { ""hash"" : "".."", ""replyTo"" : "".."", ""message"" : ""base64"" }, .. ] }");
                return;
            }

            switch (args[0])
            {
                case "-g":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Insufficient parameters count");
                        return;
                    }
                    Create(args[1], args[2]);
                    break;
                case "-a":
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Insufficient parameters count");
                        return;
                    }
                    AutoParse(args[1], args[2]);
                    break;
                case "-v": // validate
                    if (args.Length < 3)
                    {
                        Console.WriteLine("Insufficient parameters count");
                        return;
                    }
                    Validate(args[1], args[2]);
                    break;
                case "-p": // pack
                    if (args.Length < 5)
                    {
                        Console.WriteLine("Insufficient parameters count");
                        return;
                    }
                    Pack(args[1], args[2], args[3], args[4]);
                break;
                case "-u": // unpack
                    if (args.Length < 4)
                    {
                        Console.WriteLine("Insufficient parameters count");
                        return;
                    }
                    Unpack(args[1], args[2], args[3]);
                break;
            }
        }

        private static string GetString(string url)
        {
            var wc = new WebClient();
            try
            {
                return Encoding.UTF8.GetString(wc.DownloadData(url));
            }
            catch (WebException)
            {
                Console.WriteLine("nanodb.exe is not running");
                return null;
            }
        }

        private static bool ByteCountUnder(List<Post> posts, int limit)
        {
            int byteCount = 0;

            foreach (var p in posts)
            {
                byteCount += Convert.FromBase64String(p.message).Length + 32;
                if (byteCount > limit) return false;
            }

            return true;
        }

        private static int ByteCount(Post p)
        {
            return Convert.FromBase64String(p.message).Length + 32;
        }

        /*
            Takes 50 or less last posts (up to 150000 bytes max total),
            adds  50 or less random posts (up to 150000 bytes max total),
            random is shifted towards latest posts.
        */
        private static void Create(string address, string key)
        {
            var countStr = GetString(address.Trim('/') + "/api/pcount");

            if (countStr == null)
            {
                return;
            }

            var count = int.Parse(countStr);
            var take = 50;
            var last50s = GetString(address.Trim('/') + "/api/prange/" + Math.Max(count - take, 0) + "-" + take);
            var list = JsonConvert.DeserializeObject<Post[]>(last50s).ToList();

            while (!ByteCountUnder(list, 150000))
            {
                list.RemoveAt(0);
            }

            var r = new Random();
            int rbytes = 0;

            for (int i = 0; i < 50; i++)
            {
                int index = (int)Math.Min(Math.Pow(r.NextDouble(), 0.3) * count, count - 1);
                var p = JsonConvert.DeserializeObject<Post[]>(GetString(address.Trim('/') + "/api/prange/" + index + "-" + 1))[0];
                var bc = ByteCount(p);
                if (rbytes + bc > 150000) break;
                rbytes += bc;
                list.Add(p);
            }

            var files = Directory.GetFiles("containers", "*.png").ToList();
            files.AddRange(Directory.GetFiles("containers", "*.jpg"));
            files.AddRange(Directory.GetFiles("containers", "*.jpeg"));
            var file = files[r.Next(files.Count)];
            Pack(list.ToArray(), file, key, "upload/" + Guid.NewGuid().ToString() + ".png");
        }

        private static void AutoParse(string address, string key)
        {
            var files = Directory.GetFiles("download");

            foreach (var f in files)
            {
                var posts = Unpack(f, key);
                GC.Collect();
                try
                {
                    var wc = new WebClient();
                    wc.UploadData(new Uri(address.Trim('/') + "/api/addmany"), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(posts)));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                try
                {
                    File.Delete(f);
                }

                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static void Validate(Posts posts)
        {
            foreach (var p in posts.posts)
            {
                p.hash = HashCalculator.Calculate(p.replyto + Encoding.UTF8.GetString(Convert.FromBase64String(p.message)));
            }
        }

        private static void Validate(string postsPath, string outputPath)
        {
            var json = File.ReadAllText(postsPath);
            var posts = JsonConvert.DeserializeObject<Posts>(json);
            Validate(posts);
            var result = JsonConvert.SerializeObject(posts, Formatting.Indented);
            File.WriteAllText(outputPath, result);
        }

        private static void Pack(Post[] arr, string templatePath, string key, string outputPath)
        {
            var posts = new Posts { posts = arr };
            Validate(posts);
            var nposts = new List<NanoPost>();
            foreach (var p in posts.posts)
            {
                nposts.Add(new NanoPost(p.replyto + Encoding.UTF8.GetString(Convert.FromBase64String(p.message))));
            }
            var packed = NanoPostPackUtil.Pack(nposts.ToArray());
            var encrypted = ByteEncryptionUtil.EncryptSalsa20(packed, key);
            var bmp = Bitmap.FromFile(templatePath);
            var capacity = (bmp.Width * bmp.Height * 3) / 8 - 32;

            if (encrypted.Length > capacity)
            {
                float scale = (encrypted.Length / (float)capacity);
                Console.WriteLine("Warning: scaling image to increase capacity: " + scale.ToString("n2") + "x");
                scale = (float)Math.Sqrt(scale);
                bmp = new Bitmap(bmp, (int) (bmp.Width * scale + 1), (int) (bmp.Height * scale + 1));
            }

            new PngStegoUtil().HideBytesInPng(bmp, outputPath, encrypted);
        }

        private static void Pack(string postsPath, string templatePath, string key, string outputPath)
        {
            var json = File.ReadAllText(postsPath);
            var posts = JsonConvert.DeserializeObject<Posts>(json);
            Validate(posts);
            var nposts = new List<NanoPost>();
            foreach (var p in posts.posts)
            {
                nposts.Add(new NanoPost(p.replyto + Encoding.UTF8.GetString(Convert.FromBase64String(p.message))));
            }
            var packed = NanoPostPackUtil.Pack(nposts.ToArray());
            var encrypted = ByteEncryptionUtil.EncryptSalsa20(packed, key);
            var bmp = Bitmap.FromFile(templatePath);
            var capacity = (bmp.Width * bmp.Height * 3) / 8 - 32;

            if (encrypted.Length > capacity)
            {
                float scale = (encrypted.Length / (float)capacity);
                Console.WriteLine("Warning: scaling image to increase capacity: " + scale.ToString("n2") + "x");
                scale = (float)Math.Sqrt(scale);
                bmp = new Bitmap(bmp, (int) (bmp.Width * scale + 1), (int) (bmp.Height * scale + 1));
            }

            new PngStegoUtil().HideBytesInPng(bmp, outputPath, encrypted);
        }

        private static Post[] Unpack(string containerPath, string key)
        {
            try
            {
                var encrypted = new PngStegoUtil().ReadHiddenBytesFromPng(containerPath);
                var decrypted = ByteEncryptionUtil.DecryptSalsa20(encrypted, key);
                var nposts = NanoPostPackUtil.Unpack(decrypted);
                var posts = new Posts(); // TODO: remove this useless shit everywhere
                posts.posts = nposts.Select(
                    np => new Post
                    {
                        replyto = np.SerializedString().Substring(0, 32),
                        message = Convert.ToBase64String(Encoding.UTF8.GetBytes(np.SerializedString().Substring(32)))
                    }).ToArray();
                Validate(posts);
                return posts.posts;
            }
            catch
            {
                var posts = new Posts();
                posts.posts = new Post[0];
                return posts.posts;
            }
        }

        private static void Unpack(string containerPath, string key, string outputPath)
        {
            try
            {
                var encrypted = new PngStegoUtil().ReadHiddenBytesFromPng(containerPath);
                var decrypted = ByteEncryptionUtil.DecryptSalsa20(encrypted, key);
                var nposts = NanoPostPackUtil.Unpack(decrypted);
                var posts = new Posts();
                posts.posts = nposts.Select(
                    np => new Post
                    {
                        replyto = np.SerializedString().Substring(0, 32),
                        message = Convert.ToBase64String(Encoding.UTF8.GetBytes(np.SerializedString().Substring(32)))
                    }).ToArray();
                Validate(posts);
                var result = JsonConvert.SerializeObject(posts, Formatting.Indented);
                File.WriteAllText(outputPath, result);
            }
            catch
            {
                var posts = new Posts();
                posts.posts = new Post[0];
                var result = JsonConvert.SerializeObject(posts);
                File.WriteAllText(outputPath, result);
            }
        }
    }
}