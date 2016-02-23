using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Linq;
using NDB;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace NServer
{
    /*
        Gives access to various DB and server functions, mainly related to reading/writing posts.
    */
    class DbApiHandler : IRequestHandler
    {
        private PostDb _db;
        private Dictionary<string, Func<string,string,HttpResponse>> _handlers;

        public DbApiHandler(PostDb db)
        {
            _db = db;
            _handlers = new Dictionary<string, Func<string, string, HttpResponse>>();
            // filling handlers dictionary with actions that will be called with (request address, request content) args:
            _handlers["get"] = GetPostByHash;
            _handlers["delete"] = DeletePost;
            _handlers["add"] = AddPost;
            _handlers["addmany"] = AddPosts;
            _handlers["readd"] = ReAddPost;
            _handlers["replies"] = GetReplies;
            _handlers["count"] = GetPostCount;
            _handlers["nget"] = GetNthPost;
            _handlers["prange"] = GetPresentRange;
            _handlers["pcount"] = GetPresentCount;
            _handlers["search"] = Search;
            _handlers["paramset"] = ParamSet;
            _handlers["paramget"] = ParamGet;
            _handlers["params"] = Params;
        }

        // example: prange/90-10 - gets not deleted posts from 91 to 100 (skip 90, take 10)
        private HttpResponse GetPresentRange(string fromto, string notUsed = null)
        {
            var spl = fromto.Split('-');
            int skip = int.Parse(spl[0]);
            int count = int.Parse(spl[1]);
            var posts = _db.RangePresent(skip, count);
            return new HttpResponse(StatusCode.Ok, JsonConvert.SerializeObject(posts));
        }

        private HttpResponse ParamGet(string addr, string content)
        {
            if (!Configurator.Instance.HasValue(addr))
            {
                return new ErrorHandler(StatusCode.NotFound, "No such param.").Handle(null);
            }

            return new HttpResponse(StatusCode.Ok, Configurator.Instance.GetValue(addr, ""));
        }

        // returns available params in a JSON array
        private HttpResponse Params(string addr, string content)
        {
            var @params = Configurator.Instance.GetParams();
            return new HttpResponse(StatusCode.Ok, JsonConvert.SerializeObject(@params));
        }

        private HttpResponse ParamSet(string addr, string content)
        {
            Configurator.Instance.SetValue(addr, content);
            return new HttpResponse(StatusCode.Ok, "Ok");
        }

        private HttpResponse GetPostByHash(string hash, string notUsed = null)
        {
            var post = _db.GetPost(hash);

            if (post == null)
            {
                return new ErrorHandler(StatusCode.NotFound, "No such post.").Handle(null);
            }

            return new HttpResponse(StatusCode.Ok, JsonConvert.SerializeObject(post));
        }

        // includes deleted
        private HttpResponse GetNthPost(string n, string notUsed = null)
        {
            var post = _db.GetNthPost(int.Parse(n));

            if (post == null)
            {
                return new ErrorHandler(StatusCode.NotFound, "No such post.").Handle(null);
            }

            return new HttpResponse(StatusCode.Ok, JsonConvert.SerializeObject(post));
        }

        // returns count of all posts including deleted
        private HttpResponse GetPostCount(string notUsed1, string notUsed = null)
        {
            return new HttpResponse(StatusCode.Ok, _db.GetPostCount().ToString());
        }

        // returns count of not deleted posts
        private HttpResponse GetPresentCount(string notUsed1, string notUsed = null)
        {
            return new HttpResponse(StatusCode.Ok, _db.GetPresentCount().ToString());
        }

        // returns array of posts with messages including searchString, search avoids [img=..] tag contents.
        private HttpResponse Search(string searchString, string notUsed = null)
        {
            searchString = notUsed.FromB64();
            var found = new List<Post>();
            const int limit = 500;

            for (int i = _db.GetPostCount() - 1; i >= 0; i--)
            {
                var post = _db.GetNthPost(i);

                if (post == null)
                    continue;

                var msg = post.message.FromB64();

                if (msg.Contains("[img="))
                {
                    msg = Regex.Replace(msg, "\\[img=[A-Za-z0-9+=/]{4,64512}\\]", "");
                }

                if (msg.Contains(searchString))
                {
                    found.Add(post);

                    if (found.Count >= 500) break;
                }
            }

            return new HttpResponse(StatusCode.Ok, found.Count == 0 ? "[]" : JsonConvert.SerializeObject(found.ToArray()));
        }

        private HttpResponse GetReplies(string hash, string notUsed = null)
        {
            var replies = _db.GetReplies(hash);
            return new HttpResponse(StatusCode.Ok, JsonConvert.SerializeObject(replies));
        }

        private HttpResponse AddPost(string replyTo, string content)
        {
            var post = new Post(replyTo, content);
            var added = _db.PutPost(post);

            if (!added)
            {
                return new ErrorHandler(StatusCode.BadRequest, "Can't add post, probably already exists").Handle(null);
            }

            return new HttpResponse(StatusCode.Ok, JsonConvert.SerializeObject(post));
        }

        private HttpResponse AddPosts(string none, string content)
        {
            try
            {
                var posts = JsonConvert.DeserializeObject<Post[]>(content);
                foreach (var p in posts)
                    _db.PutPost(p);
            }
            catch
            {
                return new HttpResponse(StatusCode.InternalServerError, "Error");
            }
            return new HttpResponse(StatusCode.Ok, "Ok");
        }

        // same as add but allows for putting deleted post back
        private HttpResponse ReAddPost(string replyTo, string content)
        {
            var post = new Post(replyTo, content);
            var added = _db.PutPost(post, true);

            if (!added)
            {
                return new ErrorHandler(StatusCode.BadRequest, "Can't add post, probably already exists").Handle(null);
            }

            return new HttpResponse(StatusCode.Ok, JsonConvert.SerializeObject(post));
        }

        private HttpResponse DeletePost(string hash, string notUsed = null)
        {
            var deleted = _db.DeletePost(hash);

            if (!deleted)
            {
                return new ErrorHandler(StatusCode.BadRequest, "No such post.").Handle(null);
            }

            return new HttpResponse(StatusCode.Ok, "");
        }

        public HttpResponse Handle(HttpRequest request)
        {
            try
            {
                var splitted = request.Address.Split(new char[]{'/'}, StringSplitOptions.RemoveEmptyEntries);
                var cmd = splitted.Length < 2 ? "" : splitted[1];
                var arg = splitted.Length < 3 ? "" : splitted[2];

                if (_handlers.ContainsKey(cmd))
                {
                    return _handlers[cmd](arg, request.Content);
                }

                else
                {
                    return new ErrorHandler(StatusCode.BadRequest, "No such command: " + cmd + ". Available commands: " + JsonConvert.SerializeObject(_handlers.Keys.ToArray())).Handle(request);
                }
            }

            catch (Exception e)
            {
                return new ErrorHandler(StatusCode.InternalServerError, e.Message).Handle(request);
            }
        }
    }
}
