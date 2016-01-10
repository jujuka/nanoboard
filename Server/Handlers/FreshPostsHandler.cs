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
    class FreshPostsHandlder : IRequestHandler
    {
        private readonly NanoDB _db;

        public FreshPostsHandlder(NanoDB db)
        {
            _db = db;
        }

        public NanoHttpResponse Handle(NanoHttpRequest request)
        {
            try
            {
                return HandleSafe(request);
            }

            catch
            {
                return new ErrorHandler(StatusCode.InternalServerError, "").Handle(request);
            }
        }

        private NanoHttpResponse HandleSafe(NanoHttpRequest request)
        {
            var sb = new StringBuilder();
            ThreadViewHandler.AddHeader(sb);

            sb.Append("[Очистить список]".ToButton("","", @"
                var x = new XMLHttpRequest(); 
                x.open('POST', '../save/', true);
                x.send('');
                location.reload();
            ").ToDiv("",""));

            var posts = _db.GetNewPosts().ExceptHidden(_db);
            posts = posts.Reverse().ToArray();

            foreach (var p in posts)
            {
                int answers = _db.CountAnswers(p.GetHash());
                string ans = "ответ";
                if (answers != 11 && answers % 10 == 1)
                {
                    //
                }
                else if (answers != 11 && answers % 10 == 5)
                {
                    ans += "ов";
                }
                else
                {
                    ans += "а";
                }

                sb.Append(
                    (
                        p.Message.Strip(true).Replace("\n", "<br/>").ToDiv("postinner", p.GetHash().Value) +
                        ((answers > ThreadViewHandler.MinAnswers ? ("[" + answers + " " + ans + "]").ToRef("/thread/" + p.GetHash().Value):"") +
                        "[-]".ToButton("", "", @"var x = new XMLHttpRequest(); x.open('POST', '../hide/" + p.GetHash().Value + @"', true);
                        x.send('');
                        document.getElementById('" + p.GetHash().Value + @"').parentNode.style.visibility='hidden';") +
                        //("[В закладки]").ToRef("/bookmark/" + p.GetHash().Value) +
                        ("[В тред]").ToRef("/thread/" + p.ReplyTo.Value) +
                        ("[Ответить]").ToRef("/reply/" + p.GetHash().Value)).ToDiv("", "")
                    ).ToDiv("post", ""));
            }

            string s1 = "<a href='#' onclick='location.reload()'>[Обновить]</a>";
            sb.Append(s1.ToDiv("",""));
            sb.Append("<div style='height:50px'></div>");

            return new NanoHttpResponse(StatusCode.Ok, sb.ToString().ToHtmlBody(ThreadViewHandler.NotifierScript));
        }
    }
    
}