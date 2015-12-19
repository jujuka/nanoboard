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

        public const string Style = @"
body {
  font-family: 'Trebuchet MS', Trebuchet, sans-serif;
  background: #eee;
  line-height: 1.3em;
  font-size: 0.9em;
}

a {
  color: salmon;
  text-decoration: none;
  cursor: pointer;
  margin: 0 0.2em;
}

a:hover {
  color: darkorange;
  text-decoration: underline;
}

.postinner {
  max-height: 16em;
  overflow: auto;
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

div {
    display: inline-block;
    clear: both;
    float: left;
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

        public static string ToHtmlBody(this string s)
        {
            return string.Format(
                "<!DOCTYPE html><html><head><meta charset=\"UTF-8\"><style>{1}</style></head><body>{0}</body></html>", s, Style);
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

        public static string ToRefBlank(this string message, string url)
        {
            return string.Format("<a href='{0}' target='_blank'>{1}</a>", url, message);
        }
    }
    
}