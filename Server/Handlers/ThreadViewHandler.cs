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
    class ThreadViewHandler : IRequestHandler
    {
        public const int MinAnswers = -1;
        private readonly NanoDB _db;
        private readonly bool _expand;

        public ThreadViewHandler(NanoDB db, bool expand = false)
        {
            _db = db;
            _expand = expand;
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

        public const string NotifierScript = @"
        window.onload = function() {
             setInterval(function() { 
                var elem = document.getElementById('notif1');
                var x = new XMLHttpRequest();
                x.open('POST', '/status', true);
                x.onreadystatechange = function() {
                    if (x.readyState != 4 || x.status != 200) return;
                    elem.innerHTML = x.responseText;
                }
                x.send('');
            }, 1000);
        }
";

        public static void AddHeader(StringBuilder sb)
        {
            sb.Append(
                (
                    ("Наноборда<span style='font-size:0.5em;'><sup>v1.0.6</sup></span>").ToSpan("big noselect","").AddBreak() +
                    ("[Главная]".ToRef("/")) + 
                    ("[Создать PNG]".ToPostRef("/asmpng")) + 
                    ("[Сохранить базу]".ToPostRef("/save")) + 
                    ("[Свежие посты]".ToRef("/fresh")) + 
                    "[Искать посты]".ToPostRef("/aggregate") +
                    "[Картинку - в base64]".ToRefBlank("/image") +
                    ("[Выключить сервер]".ToRef("/shutdown")) +

                    "<div id='notif1' style='text-align:right;position:fixed;right:2%;top:10px;'></div>"

                ).ToDiv("head", "")
            );
            sb.Append("".ToDiv("step", ""));
        }

        private NanoHttpResponse HandleSafe(NanoHttpRequest request)
        {
            Hash thread = null;

            if (request.Address != "/")
            {
                thread = new Hash(request.Address.Split('/').Last());

                if (thread.Invalid)
                {
                    return new ErrorHandler(StatusCode.BadRequest, "Wrong hash format.").Handle(request);
                }
            }
            else
            {
                thread = _db.RootHash;
            }

            var sb = new StringBuilder();
            AddHeader(sb);

            NanoPost[] posts = null;

            if (!_expand)
                posts = _db.GetThreadPosts(thread).ExceptHidden(_db);
            else
                posts = _db.GetExpandedThreadPosts(thread).ExceptHidden(_db);
            
            bool first = true;

            foreach (var p in posts)
            {
                if (first && !p.GetHash().Zero && !p.ReplyTo.Zero)
                {
                    sb.Append(
                        (
                            p.Message.Strip().Replace("\n", "<br/>").ToDiv("postinner", p.GetHash().Value) +
                            (("[Вверх]").ToRef("/thread/" + p.ReplyTo.Value) +
                            //("[В закладки]").ToRef("/bookmark/" + p.GetHash().Value) +
                            ("[Ответить]").ToRef("/reply/" + p.GetHash().Value)).ToDiv("","")
                        ).ToDiv("post", ""));
                    first = false;
                    continue;
                }

                first = false;
                int answers = _db.CountAnswers(p.GetHash());
                string ans = "ответ";
                if (answers != 11 && answers % 10 == 1)
                {
                    //
                }
                else if (answers == 0 || (answers != 11 && answers % 10 == 5))
                {
                    ans += "ов";
                }
                else
                {
                    ans += "а";
                }

                if (p.GetHash().Value == _db.RootHash.Value)
                {
                    sb.Append(
                        (
                            p.Message.Strip().Replace("\n", "<br/>").ToDiv("postinner", p.GetHash().Value) +
                            (("[Ответить]").ToRef("/reply/" + p.GetHash().Value)).ToDiv("", "")
                        ).ToDiv("post main", ""));
                }
                else
                {
                    sb.Append(
                        (
                            p.Message.Strip().Replace("\n", "<br/>").ToDiv("postinner", p.GetHash().Value) +
                            ((answers > MinAnswers ? ("[" + answers + " " + ans + "]").ToRef("/thread/" + p.GetHash().Value) : "") +
                            "[-]".ToButton("", "", @"var x = new XMLHttpRequest(); x.open('POST', '../hide/" + p.GetHash().Value + @"', true);
                        x.send('');
                        document.getElementById('" + p.GetHash().Value + @"').parentNode.style.visibility='hidden';") +
                        //("[В закладки]").ToRef("/bookmark/" + p.GetHash().Value) +
                            ("[Ответить]").ToRef("/reply/" + p.GetHash().Value)).ToDiv("", "")
                        ).ToStyledDiv("post", "", "position:relative;left:" + p.DepthTag * 20 + "px;"));
                }
            }

            string s1 = "<a href='#' onclick='location.reload()'>[Обновить]</a>";

            if (thread.Value != _db.RootHash.Value)
            {
                if (!_expand)
                    s1 += "<a href='#' onclick='window.location.href=window.location.toString().replace(\"thread\",\"expand\")'>[Развернуть]</a>";
                else
                    s1 += "<a href='#' onclick='window.location.href=window.location.toString().replace(\"expand\",\"thread\")'>[Свернуть]</a>";
            }

            sb.Append(s1.ToDiv("", ""));

            sb.Append("<div style='height:50px'></div>");

            /*
            if (!_expand)
                sb.Append("Развернуть".ToButton("", "", "window.location.href=window.location.toString().replace('thread','expand')").ToDiv("",""));
            else
                sb.Append("Обновить".ToButton("", "", "location.reload()").ToDiv("",""));
            */
            return new NanoHttpResponse(StatusCode.Ok, sb.ToString().ToHtmlBody(NotifierScript));
        }
    }
}