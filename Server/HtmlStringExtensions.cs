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
using System.Text.RegularExpressions;
using System.Linq;

namespace nboard
{
    static class HtmlStringExtensions
    {
        private static List<string> _places;
        private static List<string> _allowed;

        private static string JQueryUiMinCss = File.ReadAllText("js"+Path.DirectorySeparatorChar+"jquery-ui.min.css");
        public static string JQueryMinJs = File.ReadAllText("js"+Path.DirectorySeparatorChar+"jquery.min.js");
        private static string JQueryUiMinJs = File.ReadAllText("js"+Path.DirectorySeparatorChar+"jquery-ui.min.js");

        public static List<string> UpdatePlaces()
        {
            if (File.Exists(Strings.Places))
                _places = File.ReadAllLines(Strings.Places).ToList();
            else
                _places = new List<string>();
            return _places;
        }

        public static List<string> UpdateAllowed()
        {
            if (File.Exists(Strings.Allowed))
                _allowed = File.ReadAllLines(Strings.Allowed).ToList();
            else
                _allowed = new List<string>();
            return _allowed;
        }

        public const string Break = "<br/>";
        public const string Line = "<hr/>";

        public static string Catalog = "";

        static HtmlStringExtensions()
        {
            UpdatePlaces();

            if (File.Exists("style.css"))
            {
                File.Delete("style.css");
            }

            var stylePath = "styles/Nano.css";

            if (File.Exists("setstyle.txt"))
            {
                stylePath = File.ReadAllText("setstyle.txt");
            }

            Style = File.ReadAllText(stylePath);

            if (File.Exists("categories.txt"))
            {
                var sb = new StringBuilder();
                var lines = File.ReadAllLines("categories.txt");
                sb.Append("<div style='font-size:80%;width:80%;margin-bottom:12px;margin-left:12px;'>Категории: [");
                for (int i = 0; i < lines.Length / 2; i++)
                {
                    string hash = lines[i*2];
                    string name = lines[i*2+1];
                    sb.Append(string.Format("<a href='/thread/{0}'>{1}</a>", hash, name));
                    if (i != lines.Length / 2 - 1) sb.Append("/");
                }
                sb.Append("]</div>");
                Catalog = sb.ToString();
            }
        }

        public static string Style;

        private static string NoStyle = @"
body {
  font-family: 'Trebuchet MS', Trebuchet, sans-serif;
  background: #eee;
  line-height: 1.3em;
  font-size: 0.9em;
  overflow-x: hidden;
  color: #333;
}
";

        public static string ShortenHash(this string s)
        {
            return s.Substring(0, 4) + ".." + s.Substring(28);
        }

        public static string AddBreak(this string s)
        {
            return s + Break;
        }

        public static string AddReply(this string s)
        {
            return s+@"
<style>.reply,reply-head,reply-footer{visibility: hidden;min-height: 120px;min-width: 250px;padding:4px 12px 12px 4px; position: fixed}.close{float:right;}.reply-head{width:100%;display: inline-block;}.reply-title{display: inline-block; cursor:default;}.reply-body{width:100%;resize: none;}.reply-footer{width:100%;}</style>
<div class =""reply post"" style=""box-shadow: 0px 0px 5px #000000;"">
    <div class=""reply-head""><div class=""reply-title""></div><a class=""close"" onclick=""$('.reply').css('visibility','hidden')"">[X]</a></div>
    <textarea class=""reply-body""></textarea>
    <div class=reply-footer>
<a onclick=surround_with_tag(""i"")><small>[<i>Курсив</i>]</small></a>
<a onclick=surround_with_tag(""b"")><small>[<b>Жирный</b>]</small></a>
<a onclick=surround_with_tag(""u"")><small>[<u>Подчерк.</u>]</small></a>
<a onclick=surround_with_tag(""s"")><small>[<s>Зачерк.</s>]</small></a>
<a onclick=surround_with_tag(""sp"")><small>[<sp>Спойлер</sp>]</small></a><br/>
<a target='_blank' href='/image'><small>[Base64-картинка]</small></a>
<a onclick=add_tag_to_reply(""[simg=]"")><small>[Картинка извне]</small></a>
<a onclick=add_tag_to_reply(""[svid=]"")><small>[Видео извне]</small></a>
<a onclick=add_tag_to_reply(""[fm=]"")><small>[Fractal music]</small></a><br/>
<button class=""send_bt"">Отправить</button>
    </div>
</div>
<script>

var h = window.location.href.toString();
if (h.includes('#')) {
h = h.substring(h.indexOf('#')+1);
var element = document.getElementById(h);
element.style.border = '1px dotted #333';
element.scrollIntoView({block: 'start', behavior: 'smooth'});
setTimeout(function(){element.scrollIntoView({block: 'start', behavior: 'smooth'});},500);
}

function send(path) {
    var x = new XMLHttpRequest();
    x.open('POST', '../write/'+path, true);
    x.onreadystatechange = function() {
        if (x.readyState != 4 || x.status != 200) return;
        var h = window.location.href.toString();
        if (h.includes('#')) h = h.substring(0, h.indexOf('#'));
        while (h.endsWith('#')) h = h.substring(0,h.length-1);
        h += '#' + x.responseText;
        window.location.href = h;
        location.reload();
    }
    x.send($('.reply-body').val());
    $('.reply').css('visibility','hidden');
    $('.reply-body').val('');
}
function add_tag_to_reply(tag) {
    var cursorPos = $('.reply-body').prop('selectionStart');
    v = $('.reply-body').val();
    var textBefore = v.substring(0,  cursorPos );
    var textAfter  = v.substring(cursorPos);
    $('.reply-body').val( textBefore+tag+textAfter );
}
function surround_with_tag(tag) {
    var cursorPos = $('.reply-body').prop('selectionStart');
    var end = $('.reply-body').prop('selectionEnd');
    v = $('.reply-body').val();
    var textBefore = v.substring(0,  cursorPos);
    var textInside = v.substring(cursorPos, end);
    var textAfter  = v.substring( end );
    $('.reply-body').val( textBefore+'['+tag+']'+textInside+'[/'+tag+']'+textAfter );
}
function show_reply(path) {
    $('.reply').css(""visibility"",""visible"");
    $('.reply-title').html('Ответ на: <a href=""#'+path+'"">&gt;&gt;'+path.substring(0,4)+'..'+path.substring(28)+'</a>');
    $('.send_bt').on(""click"", function() {
        send(path);
    })
}
// TODO: fix resize
/*
function fetch_respond_size() {
    $('.reply-body').height($('.reply').height()-60);
}
*/
$(function(){
    $('.reply').draggable();//.resizable({resize: fetch_respond_size})
});
</script>";
        }

        public static string AddVideo(this string s)
        {
            return s+@"<style>"+JQueryUiMinCss+"</style>"+"<script>"+JQueryMinJs+"</script>"+
            @"<script>"+JQueryUiMinJs+"</script>"+@"
<style>html,body,#container{height:100%}.vc{visibility: hidden;padding:5px;background-color:#f81;position:fixed;}</style>
<div class =""vc""><video title=""Не злоупотребляйте постингом картинок со сторонних ресурсов, давайте сохраним наноборду независимой!"" controls class =""vd""></div>
<script>
$(document).ready(function(){
    $('.vc').resizable({aspectRatio: true, resize: fetch_size}).draggable()
});
$('body:not(.svid)').on(""click"", function(e) {
if (!$(e.target).hasClass('vc') && !$(e.target).hasClass('vd')&& !$(e.target).hasClass('svid')){
      hide_vd();
    }
});
function show_vd(src) {
    $('.vc').css(""visibility"",""visible"")
    $('.vd').attr(""src"",src)
}
function hide_vd() {
    $('.vc').css(""visibility"",""hidden"")
    $('.vd')[0].pause()
}
function fetch_size() {
    $('.vd').width($('.vc').width())
}
</script>";
        }

        static int _id = 1;

        private static string ValidateTags(this string s)
        {
            var arr = s.Replace("<sp>", "<x>").Replace("</sp>", "</x>").ToCharArray();

            var open = new Dictionary<char, int>();
            open.Add('b', 0);
            open.Add('i', 0);
            open.Add('s', 0);
            open.Add('u', 0);
            open.Add('x', 0);
            open.Add('g', 0);

            var closed = new Dictionary<char, int>();
            closed.Add('b', 0);
            closed.Add('i', 0);
            closed.Add('s', 0);
            closed.Add('u', 0);
            closed.Add('x', 0);
            closed.Add('g', 0);

            char p3 = ' ';
            char p2 = ' ';
            char p = ' ';

            foreach (var a in arr)
            {
                if (a == '>')
                {
                    if (open.ContainsKey(p))
                    {
                        if (p2 == '<')
                        {
                            open[p] += 1;
                        }
                        else if (p2 == '/' && p3 == '<')
                        {
                            closed[p] += 1;
                        }
                    }
                }

                p3 = p2;
                p2 = p;
                p = a;
            }

            string check = "bisuxg";
            bool invalid = false;

            foreach (var c in check)
            {
                if (open[c] != closed[c])
                {
                    invalid = true;
                    break;
                }
            }

            if (invalid)
            {
                s = s.Replace("<", "&lt;");
                s = s.Replace(">", "&gt;");
            }

            return s;
        }

        public static string ReplaceFirst(this string text, string search, string replace)
        {
          int pos = text.IndexOf(search);
          if (pos < 0)
          {
            return text;
          }
          return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        private static string CreateBoardRefs(this string s)
        {
            s = Regex.Replace(s, "/(thread|expand)/[a-f0-9]{32}", "<a href='$0'>$0</a>");

            try
            {
                var matches = Regex.Matches(s, "(ADD|DEL|DELETE)(</b>|)\\s+https?://[^\\s]+");


                List<string> masks = new List<string>();
                List<string> origs = new List<string>();

                foreach (Match m in matches)
                {
                    masks.Add(Guid.NewGuid().ToString());
                    origs.Add(m.Value.Split(' ', '\t')[1]);
                    s = s.ReplaceFirst(origs.Last(), masks.Last());
                }

                for (int i = 0; i < masks.Count; i++)
                {
                    string v = masks[i];
                    string o = origs[i];
                    s = s.ReplaceFirst(v, o
                        + " <a target='_blank' href='/add/" + o + "'>[+]</a>"
                        + (_places.Contains(o)?"<i><sup>added</sup></i>":"")
                        + "<a target='_blank' href='/del/" + o + "'>[-]</a>");
                }
            }
            catch
            {
            }

            return s;
        }

        public static string GetHost(string path)
        {
            var url = new Uri(path);
            return url.Host;
        }

        public static bool IsAllowedHost(string path)
        {
            try
            {
                return _allowed.Contains(GetHost(path));
            }

            catch (System.UriFormatException)
            {
                return false;
            }
        }

        public static string Strip(this string s, bool validateTags = false)
        {
            s = s.Replace("'", "’");
            s = s.Replace("\"", "“");
            s = s.Replace("  ", "&nbsp; ");
            s = s.Replace("<", "&lt;");
            s = s.Replace(">", "&gt;");
            s = s.Replace("[u]", "<u>");
            s = s.Replace("[/u]", "</u>");
            s = s.Replace("[s]", "<s>");
            s = s.Replace("[/s]", "</s>");
            s = s.Replace("[b]", "<b>");
            s = s.Replace("[/b]", "</b>");
            s = s.Replace("[i]", "<i>");
            s = s.Replace("[I]", "<i>");
            s = s.Replace("[/I]", "</i>");
            s = s.Replace("[/i]", "</i>");
            s = s.Replace("[spoiler]", "<sp>");
            s = s.Replace("[/spoiler]", "</sp>");
            s = s.Replace("[sp]", "<sp>");
            s = s.Replace("[/sp]", "</sp>");
            s = s.Replace("[g]", "<g>");
            s = s.Replace("[/g]", "</g>");

            var matches = Regex.Matches(s, "\\[img=[/A-Za-z0-9+=]{16,64512}\\]");

            foreach (Match m in matches)
            {
                var v = m.Value;
                v = v.Substring(5, v.Length-6);
                s = s.Replace(m.Value, string.Format(
                    "<img id='imgid{0}' onclick='document.getElementById(this.id).classList.toggle(\"fullimg\")' src=\"data:image/jpg;base64,{1}\">",
                    _id++, v));
            }

            //image with src
            var matches2 = Regex.Matches(s, "\\[simg=[/A-Za-z0-9+=.:]{3,300}\\]");
            foreach (Match m in matches2)
            {
                var v = m.Value;
                v = v.Substring(6, v.Length-7);
                if (IsAllowedHost (v)){
                    s = s.Replace (m.Value, string.Format (
                        "<small>Источник: {2}</small><br><img title=\"Не злоупотребляйте постингом картинок со сторонних ресурсов, давайте сохраним наноборду независимой!\" id='imgid{0}' onclick='document.getElementById(this.id).classList.toggle(\"fullimg\")' src=\"{1}\">",
                        _id++, v,GetHost(v)));
                }
                else {
                    s = s.Replace (m.Value, string.Format (
                        "<small>Источник не разрешен (см. allowed.txt): {1}</small><br><a target='_blank' href=\"{0}\">{0}<a>",
                        v,GetHost(v)));
                }
            }
            //video with src
            var matches3 = Regex.Matches(s, "\\[svid=[/A-Za-z0-9+=.:]{3,300}\\]");
            foreach (Match m in matches3)
            {
                var v = m.Value;
                v = v.Substring(6, v.Length-7);
                if (IsAllowedHost (v)){
                    s = s.Replace (m.Value, string.Format (
                        "<small>Источник: {1}</small><br><a class=\"svid\" onclick=show_vd(\"{0}\")>[Показать видео]</a>",
                        v,GetHost(v)));
                }
                else {
                    s = s.Replace (m.Value, string.Format (
                        "<small>Источник не разрешен (см. allowed.txt): {1}</small><br><a target='_blank' href=\"{0}\">{0}<a>",
                        v,GetHost(v)));
                }
            }

            s = Regex.Replace(s, "&gt;[^\\n]*", "<grn>$0</grn>");

            s = s.CreateBoardRefs();

            if (validateTags) return s.ValidateTags();
            return s;
        }

        public static string ToNoStyleHtmlBody(this string s, string script = "")
        {
            return string.Format(
                "<!DOCTYPE html><html><head><meta charset=\"UTF-8\"><style>{1}</style><script>{2}</script></head><body>{0}</body></html>", s, NoStyle, script);
        }

        public static string ToHtmlBody(this string s, string script = "")
        {
            return string.Format(
                "<!DOCTYPE html><html><head><meta charset=\"UTF-8\"><style>{1}</style><script>{2}</script></head><body>{0}</body></html>", s, Style, script);
        }

        public static string ToHeader(this string s, int no)
        {
            return string.Format("<h{0}>{1}</h{0}>", no, s);
        }

        public static string ToBold(this string s)
        {
            return string.Format("<b>{0}</b>", s);
        }

        public static string ToPar(this string s)
        {
            return string.Format("<p>{0}</p>", s);
        }

        public static string ToElemIdClass(this string content, string @class, string id, string node)
        {
            return string.Format("<{3} id='{0}' class='{1}'>{2}</{3}>", id, @class, content, node);
        }

        public static string ToDiv(this string content, string @class, string id)
        {
            return ToElemIdClass(content, @class, id, "div");
        }

        public static string ToStyledDiv(this string content, string @class, string id, string style)
        {
            return string.Format("<{3} id='{0}' style='{4}' class='{1}'>{2}</{3}>", id, @class, content, "div", style);
        }

        public static string ToSpan(this string content, string @class, string id)
        {
            return ToElemIdClass(content, @class, id, "span");
        }

        public static string ToButton(this string content, string @class, string id, string onclick)
        {
            return string.Format("<{3} id='{0}' onclick=\"{4}\" class='{1}'>{2}</{3}>", id, @class, content, "button", onclick);
        }

        public static string ToInput(this string placeholder, string @class, string id, string type)
        {
            return string.Format("<{3} type='{4}' placeholder='{2}' id='{0}' class='{1}' />", id, @class, placeholder, "input", type);
        }

        public static string ToTextArea(this string content, string @class, string id)
        {
            return ToElemIdClass(content, @class, id, "textarea");
        }

        public static string ToRef(this string message, string url)
        {
            return string.Format("<a href='{0}'>{1}</a>", url, message);
        }

        public static string ToPostRef(this string message, string url)
        {
            return string.Format(
                "<a onclick=\"var x=new XMLHttpRequest();x.open('GET','{0}','true');x.send('');\">{1}</a>", url, message);
        }

        public static string ToRefBlank(this string message, string url)
        {
            return string.Format("<a href='{0}' target='_blank'>{1}</a>", url, message);
        }
    }
    
}
