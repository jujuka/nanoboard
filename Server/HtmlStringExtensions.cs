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
        public const string Break = "<br/>";
        public const string Line = "<hr/>";

        public static string Catalog = "";

        static HtmlStringExtensions()
        {
            if (File.Exists("style.css"))
            {
                File.Delete("style.css");
            }

            Style = File.ReadAllText("styles/Nano.css");

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

        public static string AddBreak(this string s)
        {
            return s + Break;
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

            var matches = Regex.Matches(s, "\\[img=[/A-z0-9+=]{16,32768}\\]");

            foreach (Match m in matches)
            {
                var v = m.Value;
                v = v.Substring(5, v.Length-6);
                s = s.Replace(m.Value, string.Format(
                    "<img id='imgid{0}' onclick='document.getElementById(this.id).classList.toggle(\"fullimg\")' src=\"data:image/jpg;base64,{1}\">",
                    _id++, v));
            }

            s = Regex.Replace(s, "&gt;[^\\n]*", "<grn>$0</grn>");

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