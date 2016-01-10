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
                    ("Наноборда<span style='font-size:0.5em;'><sup>v"+App.Version+"</sup></span>").ToSpan("big noselect","").AddBreak() +
                    ("[Главная]".ToRef("/")) + 
                    ("[Создать PNG]".ToPostRef("/asmpng")) + 
                    //("[Сохранить базу]".ToPostRef("/save")) + 
                    ("[Свежие посты]".ToRef("/fresh")) + 
                    "[Искать посты]".ToPostRef("/aggregate") +
                    "[Картинка&gt;Base64]".ToRefBlank("/image") +
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

            string s1 = "<a href='#' onclick='location.reload()'>[Обновить]</a>";

            if (thread.Value != _db.RootHash.Value)
            {
                if (!_expand)
                    s1 += "<a href='#' onclick='window.location.href=window.location.toString().replace(\"thread\",\"expand\")'>[Развернуть]</a>";
                else
                    s1 += "<a href='#' onclick='window.location.href=window.location.toString().replace(\"expand\",\"thread\")'>[Свернуть]</a>";
            }

            sb.Append(s1.ToDiv("", ""));

            NanoPost[] posts = null;

            /*
            if (!_expand)
                posts = _db.GetThreadPosts(thread).ExceptHidden(_db);
            else
                posts = _db.GetExpandedThreadPosts(thread).ExceptHidden(_db);
            */

            if (!_expand)
                posts = _db.GetThreadPosts(thread);
            else
                posts = _db.GetExpandedThreadPosts(thread);

            bool first = true;

            foreach (var p in posts)
            {
                string pMessage = p.Message;
                string hMessage = "[i]Пост " + p.GetHash().Value + " скрыт.[/i]";
                bool hidden = false;

                if (_db.IsHidden(p.GetHash()))
                {
                    hidden = true;
                    //pMessage = hMessage;
                }

                if (first && !p.GetHash().Zero && !p.ReplyTo.Zero)
                {
                    sb.Append(
                        (
                            pMessage.Strip(true).Replace("\n", "<br/>").ToDiv("postinner", p.GetHash().Value) +
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
                            ("Добро пожаловать на Наноборду!\nЭто корневой нанопост. Отвечая на него вы создаете тред в верхнем уровне.\n"+
                            "На самом деле тредов тут нет, а есть лишь нанопосты ссылающиеся друг на друга, идущие от корневого.\n" +
                            "Отвечать нужно на конкретное сообщение, а не \"в тред\" как вы привыкли, иначе будет путаница.\n" + 
                            "Можно и нужно делать вложенные треды/категории. Помещать всё в корень - вариант только на первое время.").Strip().Replace("\n", "<br/>").ToDiv("postinner", p.GetHash().Value) +
                            (("[Ответить]").ToRef("/reply/" + p.GetHash().Value)).ToDiv("", "") + 
                            ("[Развернуть всё (осторожно!)]").ToRef("/expand/f682830a470200d738d32c69e6c2b8a4").ToDiv("", "") +
                            ("[Категории]").ToRef("/thread/bdd4b5fc1b3a933367bc6830fef72a35").ToDiv("", "")
                        ).ToDiv("post main", ""));
                }
                else
                {
                    sb.Append(
                        (
                            pMessage.Strip(true).Replace("\n", "<br/>").ToStyledDiv("postinner", p.GetHash().Value, hidden?"visibility:hidden;height:0px;":"") +
                            ((answers > MinAnswers ? ("[" + answers + " " + ans + "]").ToRef("/thread/" + p.GetHash().Value) : "") +
                            (hidden?"[Вернуть]":"[Удалить]").ToButton("", "", @"var x = new XMLHttpRequest(); x.open('POST', '../hide/" + p.GetHash().Value + @"', true);
                        x.send('');
                        var elem = document.getElementById('" + p.GetHash().Value + @"');
                        if (elem.style.visibility != 'hidden') {
                            elem.style.visibility='hidden';
                            elem.style.height = '0px';
                            innerHTML = '[Вернуть]';
                        } else { 
                            elem.style.visibility='visible';
                            elem.style.height = '100%';
                            innerHTML = '[Удалить]';
                        }
                        ") +
                        //("[В закладки]").ToRef("/bookmark/" + p.GetHash().Value) +
                            ("[Ответить]").ToRef("/reply/" + p.GetHash().Value)).ToDiv("", "")
                        ).ToStyledDiv("post", "", "position:relative;left:" + p.DepthTag * 20 + "px;"));
                }
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