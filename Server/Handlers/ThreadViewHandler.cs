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
        private readonly NanoDB _db;

        public ThreadViewHandler(NanoDB db)
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

            sb.Append(
                (
                    ("Наноборда").ToSpan("big noselect","").AddBreak() +
                    ("[Главная]".ToRef("/")) + 
                    ("[Создать PNG]".ToRef("/asmpng")) + 
                    ("[Сохранить базу]".ToRef("/save")) + 
                    ("[Свежие посты]".ToRef("/fresh")) + 
                    ("[Искать посты]".ToRef("/aggregate")) + 
                    ("[Выключить сервер]".ToRef("/shutdown")) 
                ).ToDiv("head", "")         
            );

            sb.Append("".ToDiv("step", ""));

            var posts = _db.GetThreadPosts(thread).ExceptHidden(_db);

            bool first = true;

            foreach (var p in posts)
            {
                if (first && !p.GetHash().Zero && !p.ReplyTo.Zero)
                {
                    sb.Append(
                        (
                            p.Message.Replace("\n", "<br/>").ToDiv("postinner", p.GetHash().Value) +
                            ("[Вверх]").ToRef("/thread/" + p.ReplyTo.Value) +
                            //("[В закладки]").ToRef("/bookmark/" + p.GetHash().Value) +
                            ("[Ответить]").ToRef("/reply/" + p.GetHash().Value)
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
                else if (answers != 11 && answers % 10 == 5)
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
                        p.Message.Replace("\n", "<br/>").ToDiv("postinner", p.GetHash().Value) +
                        ("[Ответить]").ToRef("/reply/" + p.GetHash().Value)
                    ).ToDiv("post main", ""));
                }
                else
                sb.Append(
                    (
                        p.Message.Replace("\n", "<br/>").ToDiv("postinner", p.GetHash().Value) +
                        (answers > 0 ? ("[" + answers + " " + ans + "]").ToRef("/thread/" + p.GetHash().Value):"") +
                        "[-]".ToButton("", "", @"var x = new XMLHttpRequest(); x.open('POST', '../hide/" + p.GetHash().Value + @"', true);
                        x.send('');
                        document.getElementById('" + p.GetHash().Value + @"').parentNode.style.visibility='hidden';") +
                        //("[В закладки]").ToRef("/bookmark/" + p.GetHash().Value) +
                        ("[Ответить]").ToRef("/reply/" + p.GetHash().Value)
                    ).ToDiv("post", ""));
            }

            sb.Append("Обновить".ToButton("", "", "location.reload()").ToDiv("",""));

            return new NanoHttpResponse(StatusCode.Ok, sb.ToString().ToHtmlBody());
        }
    }
}