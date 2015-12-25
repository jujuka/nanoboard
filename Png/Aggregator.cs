using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace nboard
{
    class Aggregator
    {
        private const string UserAgentConfig = "useragent.config";
        private const string Downloaded = "downloaded.txt";
        private const string Config = "places.txt";
        private const string ImgPattern = "href=\"[A-z0-9/\\.]*\\.png\"";

        private int _inProgress = 0;
        public int InProgress
        { 
            get
            {
                return _inProgress;
            }

            set
            {
                _inProgress = value;
                ProgressChanged();
            }
        }

        public event Action ProgressChanged = delegate {};

        private readonly List<string> _places;

        private readonly HashSet<string> _downloaded;

        private readonly WebHeaderCollection _headers;

        public Aggregator()
        {
            if (!File.Exists(UserAgentConfig))
            {
                File.WriteAllText(UserAgentConfig, "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2227.1 Safari/537.36");
            }

            string userAgent = File.ReadAllLines(UserAgentConfig).First(l => !l.StartsWith("#")).Trim();
            _headers = new WebHeaderCollection();
            _headers[HttpRequestHeader.UserAgent] = userAgent;
            _headers[HttpRequestHeader.Accept] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";

            if (File.Exists(Downloaded))
            {
                _downloaded = new HashSet<string>(File.ReadAllLines(Downloaded));
            }

            else
            {
                _downloaded = new HashSet<string>();
            }

            if (File.Exists(Config))
            {
                _places = new List<string>(File.ReadAllLines(Config));
            }

            else
            {
                File.WriteAllText(Config, "# put urls to threads here, each at new line:\n");
                _places = new List<string>();
            }
        }

        public void Aggregate()
        {
            bool empty = true;

            foreach (string place in _places)
            {
                if (!place.StartsWith("#"))
                {
                    empty = false;
                    ParseText(place);
                }
            }

            if (empty)
            {
                InProgress = 0;
            }
        }

        private void ParseText(string address)
        {
            Logger.Log(address);
            var client = new WebClient();
            //client.Headers = _headers;

            client.DownloadDataCompleted += (object sender, DownloadDataCompletedEventArgs e) => 
            {
                InProgress -= 1;
                string imageAddress = "";
                try
                {
                    string text = Encoding.UTF8.GetString(e.Result);
                    string host = Regex.Match(address, "https?://[A-z\\.0-9-]*").Value;

                    var images = Regex.Matches(text, ImgPattern);

                    foreach (Match im in images)
                    {
                        imageAddress = host + im.Value.Replace("href=", "").Trim('"');
                        ParseImage(imageAddress);
                    }
                }

                catch (Exception ex)
                {
                    InProgress -= 1;
                    Logger.LogErrorDrawLine();
                    Logger.LogError(imageAddress);
                    if (e.Error != null)
                        Logger.LogError(e.Error.Message);
                    Logger.LogError(ex.Message);
                }
            };

            InProgress += 1;
            client.DownloadDataAsync(new Uri(address));
        }

        private void ParseImage(string address)
        {
            if (_downloaded.Contains(address)) return;
            _downloaded.Add(address);
            File.AppendAllText(Downloaded, address + "\n");
            Logger.Log(address);
            var client = new WebClient();
            client.Headers = _headers;

            client.DownloadDataCompleted += (object sender, DownloadDataCompletedEventArgs e) => 
            {
                InProgress -= 1;

                try
                {
                    if (!Directory.Exists(Strings.Download))
                    {
                        Directory.CreateDirectory(Strings.Download);
                    }

                    File.WriteAllBytes(Strings.Download + Path.DirectorySeparatorChar + new Uri(address).Segments.Last(), e.Result);
                }

                catch (Exception ex)
                {
                    Logger.LogErrorDrawLine();
                    Logger.LogError(address);
                    if (e.Error != null)
                        Logger.LogError(e.Error.Message);
                    Logger.LogError(ex.Message);
                }
            };

            InProgress += 1;
            client.DownloadDataAsync(new Uri(address));
        }
    }
}