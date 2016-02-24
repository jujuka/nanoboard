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

            catch (Exception e)
            {
                return new ErrorHandler(StatusCode.InternalServerError, e.ToString().Replace("\n", "<br>")).Handle(request);
            }
        }

        private NanoHttpResponse HandleSafe(NanoHttpRequest request)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var sb = new StringBuilder();
            ThreadViewHandler.AddHeader(sb);

            sb.Append(string.Format("<div>Количество правил игнорирования постов: {0}. Настройте spamfilter.txt под себя.</div>", SpamDetector.RuleCount));

            sb.Append("[Очистить список]".ToButton("", "", @"
                var x = new XMLHttpRequest(); 
                x.open('POST', '../save/', true);
                x.send('');
                location.reload();
            ").ToDiv("", ""));

            var posts = _db.GetNewPosts();//.ExceptHidden(_db);
            posts = posts.Reverse().ToArray();

            if (posts.Length == 0)
            {
                posts = _db.GetNLastPosts(200).Reverse().ToArray();
            }

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

                bool corePost = false;

                if (_db.Get(p.ReplyTo) != null && 
                    (_db.Get(p.ReplyTo).ReplyTo.Value == NanoDB.CategoriesHashValue ||
                     _db.Get(p.ReplyTo).ReplyTo.Value == NanoDB.RootHashValue))
                {
                    corePost = true;
                }

                bool hidden = _db.IsHidden(p.GetHash());
                sb.Append(
                    (
                        p.Message.Strip(true).Replace("\n", "<br/>").ToStyledDiv("postinner", p.GetHash().Value, hidden?"visibility:hidden;height:0px;":"") +
                        ((answers > ThreadViewHandler.MinAnswers ? ("[" + answers + " " + ans + "]").ToRef("/expand/" + p.GetHash().Value):"") +
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
                        (p.ContainerTag != null ?
                        "[Отклонить контейнер]".ToButton("","",@"var x = new XMLHttpRequest(); x.open('POST', '../hideall/" + p.GetHash().Value + @"', true);
                        x.send('');location.reload();") : "")+
                        //("[В закладки]").ToRef("/bookmark/" + p.GetHash().Value) +
                        ("[В тред]").ToRef((corePost?"/thread/":"/expand/") + p.ReplyTo.Value) +
                        ("[Ответить]").ToRef("/reply/" + p.GetHash().Value)).ToDiv("", "")
                    ).ToDiv("post", ""));
            }

            string s1 = "<a href='#' onclick='location.reload()'>[Обновить]</a>";
            sb.Append(s1.ToDiv("",""));

            sw.Stop();
            ThreadViewHandler.AddFooter(sb, sw.ElapsedMilliseconds, _db);

            return new NanoHttpResponse(StatusCode.Ok, sb.ToString().ToHtmlBody(ThreadViewHandler.NotifierScript));
        }
    }
    
}