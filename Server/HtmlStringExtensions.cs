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

namespace nboard
{
    static class HtmlStringExtensions
    {
        public const string Break = "<br/>";
        public const string Line = "<hr/>";

        static HtmlStringExtensions()
        {
            if (!File.Exists("style.css"))
            {
                File.WriteAllText("style.css", Style);
            }

            Style = File.ReadAllText("style.css");
        }

        public static string Style = @"
body {
  font-family: 'Trebuchet MS', Trebuchet, sans-serif;
  background: #eee;
  line-height: 1.3em;
  font-size: 0.9em;
  overflow-x: hidden;
}

a {
  color: salmon;
  text-decoration: none;
  cursor: pointer;
  margin: 0 0.2em;
}

sp {
  text-decoration: none;
  color: #bbb;
  background: #bbb;
}

g {
  color: green;
}

sp:hover {
  background: #ddd;
  color: #333;
}

a:hover {
  color: darkorange;
  text-decoration: underline;
}

.postinner {
  max-height: 48em;
  overflow: auto;
  max-width: 600px;
  margin-bottom: .5em;
}

.post {
  width: auto;
  border: 1px solid #ccc;
  border-radius: .5em;
  display: inline-block;
  white-space: wrap;
  background: #ddd;
  color: #333;
  margin: 0.25em;
  float: left;
  clear: both;
  padding: 0.7em;
}

img {
    max-width: 100px;
    max-height: 100px;
}

.fullimg {
    max-width: 100%;
    max-height: 100%;
}

.main {
  background: white;
}

div {
    display: inline-block;
    clear: both;
    float: left;
}

.head {
  background: #654;
  color: salmon;
  padding: 1em;
  width: 100%;
  margin-left: 0;
  margin-right: 0;
  margin-top: 0;
  position: absolute;
  top:0;
  left:0;
}

.noselect {
    -webkit-touch-callout: none;
    -webkit-user-select: none;
    -khtml-user-select: none;
    -moz-user-select: none;
    -ms-user-select: none;
    user-select: none;
    cursor: default;
}

.step {
  height:5em;
}

.big {
    font-size: 2em;
}

textarea
{
  font-size: 0.9em;
  width: 32em;
  height: 22em;
}

button
{
  font-size: 0.9em;
}
";

        public static string AddBreak(this string s)
        {
            return s + Break;
        }

        static int _id = 1;

        public static string StripInput(this string s)
        {
            s = s.Replace("<", "&lt;");
            s = s.Replace(">", "&gt;");
            return s;
        }

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

        public static string Strip(this string s, bool validateTags = false)
        {
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
            s = s.Replace("[sp]", "<sp>");
            s = s.Replace("[/sp]", "</sp>");
            s = s.Replace("[g]", "<g>");
            s = s.Replace("[/g]", "</g>");

            if (s.Contains("[img="))
            {
                s = s.Replace("]", "\" />");
            }

            string imgscript = "onclick='document.getElementById(this.id).classList.toggle(\"fullimg\")'";
            s = s.Replace("[img=", "<img id='imgid" + _id++ + "' " + imgscript + "src=\"data:image/jpg;base64,");
            s = s.Replace("[jpg=", "<img id='imgid" + _id++ + "' " + imgscript + "src=\"data:image/jpg;base64,");
            s = s.Replace("[png=", "<img id='imgid" + _id++ + "' " + imgscript + "src=\"data:image/png;base64,");
            s = s.Replace("[gif=", "<img id='imgid" + _id++ + "' " + imgscript + "src=\"data:image/gif;base64,");

            if (validateTags) return s.ValidateTags();
            return s;
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