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
using captcha;

namespace NServer
{
    /*
        Address: captcha token
        Content: toBase64(captcha answer)
        Response: 200 OK in case of success
    */
    class SolveCaptchaAndAddPostHandler : IRequestHandler
    {
        private readonly PostDb _db;

        public SolveCaptchaAndAddPostHandler(PostDb db)
        {
            _db = db;
        }

        public HttpResponse Handle(HttpRequest request)
        {
            var token = request.Address.Split('/').Last();

            if (!CaptchaTracker.Captchas.ContainsKey(token))
            {
                return new HttpResponse(StatusCode.BadRequest, "Not existing token");
            }

            var captcha = CaptchaTracker.Captchas[token];
            var post = CaptchaTracker.Posts[token];
            var guess = request.Content.FromB64();

            if (!captcha.CheckGuess(guess))
            {
                return new HttpResponse(StatusCode.BadRequest, "Wrong answer");
            }

            if (_db.PutPost(new Post(post.Substring(0, 32), post.Substring(32))))
            {
                return new HttpResponse(StatusCode.Ok, "Post was successfully added");
            }

            CaptchaTracker.Captchas.Remove(token);
            CaptchaTracker.Posts.Remove(post);

            return new HttpResponse(StatusCode.BadRequest, "Unable to add post.");
        }
    }
}
