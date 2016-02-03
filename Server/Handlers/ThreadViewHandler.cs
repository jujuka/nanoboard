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
using System.Text.RegularExpressions;
using System.Linq;

namespace nboard
{
    class ThreadViewHandler : IRequestHandler
    {
        public const int MinAnswers = -1;
        private readonly NanoDB _db;
        private readonly bool _expand;
        private string[] _places;
        private List<string> _allowed;

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

            catch (Exception e)
            {
                return new ErrorHandler(StatusCode.InternalServerError, e.ToString().Replace("\n", "<br>")).Handle(request);
            }
        }

        public const string FractalMusicScript = @"
   var sampleRate = 8000;

    function encodeAudio8bit(data) {
      var n = data.length;
      var integer = 0,
        i;
      var header = 'RIFF<##>WAVEfmt \x10\x00\x00\x00\x01\x00\x01\x00<##><##>\x01\x00\x08\x00data<##>';

      function insertLong(value) {
        var bytes = '';
        for (i = 0; i < 4; ++i) {
          bytes += String.fromCharCode(value % 256);
          value = Math.floor(value / 256);
        }
        header = header.replace('<##>', bytes);
      }

      insertLong(36 + n);
      insertLong(sampleRate);
      insertLong(sampleRate);
      insertLong(n);

      for (var i = 0; i < n; ++i) {
        header += String.fromCharCode(Math.round(Math.min(1, Math.max(-1, data[i])) * 127 + 127));
      }
      return 'data:audio/wav;base64,' + btoa(header);
    }

    function addFractalMusic(func,len,id) {
        var elem = document.getElementById(id);
      var arr = []
      for (var t = 0; t < len; t++) {
        arr[t] = func(t);
        arr[t] = (arr[t] & 0xff) / 256.0;
      }
      var dataUri = encodeAudio8bit(arr);
      elem.setAttribute('src', dataUri);
      elem.style.visibility = 'visible';
      elem.setAttribute('controls', true);
    }
";

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

        public static string PostScript (string other)
        {
            return @"
        window.onload = function() {" + other + @"
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
        }

        private static string _styles;

        private static string Styles()
        {
            if (_styles != null)
                return _styles;

            string r = "";

            if (Directory.Exists("styles"))
            {
                var files = Directory.GetFiles("styles");

                if (files.Length > 0)
                {
                    r += "<select id='stylesel' onchange='var x = new XMLHttpRequest(); x.open(\"POST\", \"../setstyle\", true); x.onreadystatechange = function(){location.reload();}; x.send(value.toString());'>";

                    r += "<option> </option>";

                    foreach (var file in files)
                    {
                        if (file.EndsWith(".css"))
                        {
                            var file1 = file.Replace("styles/", "").Replace("styles\\", "").Replace(".css", "");
                            r += "<option>" + file1 + "</option>";
                        }
                    }

                    r += "</select>";
                }
            }

            _styles = r;

            return r;
        }

        public static void AddHeader(StringBuilder sb)
        {
            sb.Append(
                (
                    //("Наноборда<span style='font-size:0.5em;'><sup>v"+App.Version+"</sup></span>").ToSpan("big noselect","").AddBreak() +
                    ("Наноборда").ToSpan("big noselect","").AddBreak() +
                    ("[Главная]".ToRef("/")) + 
                    ("[Создать PNG]".ToPostRef("/asmpng")) + 
                    //("[Сохранить базу]".ToPostRef("/save")) + 
                    ("[Свежие посты]".ToRef("/fresh")) + 
                    "[Искать посты]".ToPostRef("/aggregate") +
                    "[Картинка&gt;Base64]".ToRefBlank("/image") +
                    ("[Выключить сервер]".ToRef("/shutdown")) +

                    "<div id='notif1' style='text-align:right;position:fixed;right:2%;top:10px;'></div>"
                    +
                    Styles()
                ).ToDiv("head", "")
            );
            sb.Append("".ToDiv("step", ""));
            sb.Append(HtmlStringExtensions.Catalog);
        }

        public static void AddFooter(StringBuilder sb, long elapsedMs, NanoDB db)
        {
            sb.Append("<div style='height:15px'></div>");
            sb.Append(HtmlStringExtensions.Catalog);
            sb.Append("<div style='height:15px'></div>");
            sb.Append("<div style='text-align:center;width:100%;'><g>Сервер nboard_"+App.Version+" // Время генерации: "+elapsedMs+" ms // Постов в базе: "+db.GetPostCount()+"</g></div>");
            sb.Append("<div style='height:15px'></div>");
        }

        private NanoHttpResponse HandleSafe(NanoHttpRequest request)
        {
            _places = HtmlStringExtensions.UpdatePlaces().ToArray();
            _allowed = HtmlStringExtensions.UpdateAllowed();
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

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
            string s1 = "";

            if (thread.Value != _db.RootHash.Value)
            {
                s1 = "<a href='#' onclick='history.go(-1)'>[Назад]</a>";
                s1 += "<a href='#' onclick='location.reload()'>[Обновить]</a>";

                if (!_expand)
                    s1 += "<a href='#' onclick='window.location.href=window.location.toString().replace(\"thread\",\"expand\")'>[Развернуть]</a>";
                else
                    s1 += "<a href='#' onclick='window.location.href=window.location.toString().replace(\"expand\",\"thread\")'>[Свернуть]</a>";
            }
            else
            {
                s1 = "<a href='#' onclick='location.reload()'>[Обновить]</a>";
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

            if (!_expand)
            {
                posts = posts.OrderByDescending(p => p.NumberTag).ToArray();
            }

            string postScript = "";

            foreach (var sp in posts)
            {
                var p = sp;
                //string pMessage = p.Message;
                string pMessage = p.Message.Strip(true);

                var fmPattern = "\\[fm=.*\\]";
                var music = Regex.Matches(pMessage, fmPattern);

                int musicNum = 0;

                foreach (var m in music)
                {
                    musicNum += 1;
                    var value = (m as Match).Value;
					value = value.Replace ("post", "dummy0");
					value = value.Replace ("XMLHttpRequest", "dummy1");
					value = value.Replace ("eval", "dummy2");
					value = value.Replace ("ajax", "dummy3");
					value = value.Replace ("get", "dummy4");
                    var formula = value.Substring(4).TrimEnd(']').Replace("&gt;", ">").Replace("&lt;", "<").Replace("<grn>", "").Replace("</grn>", "").Replace("&nbsp;", " ").
						Replace("’", "'").Replace("“", "\"");
                    var replacement = string.Format(@"<b>Фрактальная музыка:</b>
    <small><pre>{1}</pre></small><button id='mb{0}'>Сгенерировать</button>
    <audio style='visibility:hidden;' controls='false' id='au{0}'></audio>", sp.GetHash().Value + musicNum, formula);
                    postScript += "document.getElementById('mb" + sp.GetHash().Value + musicNum +
                    "').onclick = function() { addFractalMusic(function(t){return " + formula +
                    ";}, 210*8000, 'au" + sp.GetHash().Value + musicNum + "');" +
                    "this.parentNode.removeChild(this);"
                    + "}\n";
                    pMessage = pMessage.Replace(value, replacement);
                }

                string numTag = (p.NumberTag == int.MaxValue ? "" : "<grn><sup>#" + p.GetHash().Value.ShortenHash() + "</sup></grn> ");
                bool hidden = false;

                if (_db.IsHidden(p.GetHash()))
                {
                    hidden = true;
                }

                string handler = "/expand/";
                bool corePost = false;

                if (p.GetHash().Value == NanoDB.RootHashValue || // root
                    p.ReplyTo.Value == NanoDB.RootHashValue || // root
                    p.GetHash().Value == NanoDB.CategoriesHashValue || // categories
                    p.ReplyTo.Value == NanoDB.CategoriesHashValue)     // categories
                {
                    handler = "/thread/";
                    corePost = true;
                }

                if (_db.Get(p.ReplyTo) != null &&
                    (_db.Get(p.ReplyTo).ReplyTo.Value == NanoDB.CategoriesHashValue ||
                    _db.Get(p.ReplyTo).ReplyTo.Value == NanoDB.RootHashValue))
                {
                    corePost = true;
                }

                Func<NanoPost,string> addRefs = pst => {
                    var children = _db.GetThreadPosts(pst.GetHash(), eraseDepth:false);
                    var refs1 = "<br/><div><small>";
                    int line = 0;
                    foreach (var ch in children)
                    {
                        if (ch.GetHash().Value != pst.GetHash().Value)
                        {
                            line += 1;
                            refs1 += "<a href='#" + ch.GetHash().Value + "'><i>&gt;&gt;" + ch.GetHash().Value.ShortenHash() + "</i></a>";
                            if (line > 5) 
                            {
                                line = 0;
                                refs1 += "</br>";
                            }
                        }
                    }
                    refs1 += "</small></div>";
                    return refs1;
                };

                if (_expand && first && !p.GetHash().Zero && !p.ReplyTo.Zero)
                {
                    string refs = "";

                    if (p.ReplyTo.Value != NanoDB.CategoriesHashValue &&
                        p.ReplyTo.Value != NanoDB.RootHashValue && _expand)
                    {
                        refs = addRefs(p);
                    }

                    sb.Append(
                        (
                            (numTag + pMessage + refs).Replace("\n", "<br/>").ToDiv("postinner", p.GetHash().Value) +
                            ("[Вверх]".ToRef((corePost ? "/thread/" : "/expand/") + p.ReplyTo.Value) +
                                //("[В закладки]").ToRef("/bookmark/" + p.GetHash().Value) +
                     ("<a onclick='show_reply(\""+p.GetHash().Value+"\")'>[Быстрый ответ]</a>")+("[Ответить]").ToRef("/reply/" + p.GetHash().Value)).ToDiv("", "")
                        ).ToDiv("post", ""));
                    first = false;
                    continue;
                }

                first = false;
                int answers = _db.CountAnswers(p.GetHash());
                string ans = "ответ";

                int a = answers % 100;
                if (a == 0 || a % 10 == 0 || (a > 10 && a < 20))
                    ans += "ов";
                else if (a % 10 >= 2 && a % 10 <= 4)
                    ans += "а";
                else if (a % 10 >= 5 && a % 10 <= 9)
                    ans += "ов";

                if (p.GetHash().Value == _db.RootHash.Value)
                {
                    sb.Append(
                        (
                            (@"    Добро пожаловать на Наноборду!
    Это корневой нанопост. 
    В целях тестирования на него можно было отвечать в предыдущих версиях. 
    Это немного засорило Главную. Рекомендуется почистить её у себя вручную.
    Негласное правило: отвечать нужно на конкретное сообщение, а не просто ""в тред"", полагаясь на то, что сообщение выше вашего будет таким же и у других - порядок попадания нанопостов к другим участникам сложно предсказать.
    Создавать тред желательно в соответствующей категории."
                            ).Strip().Replace("\n", "<br/>").ToDiv("postinner", p.GetHash().Value) +
                            //(("[Ответить]").ToRef("/reply/" + p.GetHash().Value)).ToDiv("", "") + 
                            ("[Развернуть всё (осторожно!)]").ToRef("/expand/f682830a470200d738d32c69e6c2b8a4").ToDiv("", "") +
                            ("[Категории]").ToRef("/thread/bdd4b5fc1b3a933367bc6830fef72a35").ToDiv("", "")
                        ).ToDiv("post main", ""));
                }
                else
                {
                    string refs = "";

                    if (p.ReplyTo.Value != NanoDB.CategoriesHashValue &&
                        p.ReplyTo.Value != NanoDB.RootHashValue && _expand)
                    {
                        refs = addRefs(p);
                    }

                     sb.Append(
                        (
                            ((_expand?("<a href='#"+p.ReplyTo.Value+"'><i>&gt;&gt;"+p.ReplyTo.Value.ShortenHash()+"</i></a><br/>"):"") + numTag+pMessage+refs).Replace("\n", "<br/>").ToStyledDiv("postinner", p.GetHash().Value, hidden?"visibility:hidden;height:0px;":"") +
                            ((answers > MinAnswers ? ("[" + answers + " " + ans + "]").ToRef(handler + p.GetHash().Value) : "") +
                                (p.GetHash().Value != "bdd4b5fc1b3a933367bc6830fef72a35" ?
                            (
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
                        ")) : "") +
                        //("[В закладки]").ToRef("/bookmark/" + p.GetHash().Value) +
                     ("<a onclick='show_reply(\""+p.GetHash().Value+"\")'>[Быстрый ответ]</a>")+("[Ответить]").ToRef("/reply/" + p.GetHash().Value)).ToDiv("", "")
                        ).ToStyledDiv("post", "", "position:relative;left:" + p.DepthTag * 20 + "px;"));
                }
            }

            sb.Append(s1.ToDiv("", ""));

            sw.Stop();

            sb.Append("<div><br>места:");
            var places = _places.Where(l => !l.StartsWith("#")).ToList();
            places.ForEach(p => sb.Append(string.Format("<br><a target='_blank' href='{0}'>{0}</a>"+
            "<a target='_blank' href='/del/{0}'>[-]</a>", p)));
            sb.Append("</div>");

            AddFooter(sb, sw.ElapsedMilliseconds, _db);

            var result = sb.ToString();

            /*
            if (!_expand)
                sb.Append("Развернуть".ToButton("", "", "window.location.href=window.location.toString().replace('thread','expand')").ToDiv("",""));
            else
                sb.Append("Обновить".ToButton("", "", "location.reload()").ToDiv("",""));
            */
            return new NanoHttpResponse(StatusCode.Ok, result.AddVideo().AddReply().ToHtmlBody(FractalMusicScript + PostScript(postScript)));
        }
    }
}
