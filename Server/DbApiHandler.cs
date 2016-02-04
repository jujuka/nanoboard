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

namespace NServer
{
    class DbApiHandler : IRequestHandler
    {
        private PostDb _db;
        private Dictionary<string, Func<string,string,HttpResponse>> _handlers;

        public DbApiHandler(PostDb db)
        {
            _db = db;
            _handlers = new Dictionary<string, Func<string, string, HttpResponse>>();
            _handlers["get"] = GetPostByHash;
            _handlers["delete"] = DeletePost;
            _handlers["add"] = AddPost;
            _handlers["replies"] = GetReplies;
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
                var splitted = request.Address.Split('/');
                var cmd = splitted.Skip(splitted.Length-2).First();
                var arg = splitted.Last();

                if (_handlers.ContainsKey(cmd))
                {
                    return _handlers[cmd](arg, request.Content);
                }

                else
                {
                    return new ErrorHandler(StatusCode.BadRequest, "No such command.").Handle(request);
                }
            }

            catch
            {
                return new ErrorHandler(StatusCode.InternalServerError, "").Handle(request);
            }
        }
    }
}