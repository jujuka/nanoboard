using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace nboard
{
    static class SpamDetector
    {
        private static List<string> _regexps;

        public static int RuleCount
        {
            get
            {
                return _regexps.Count;
            }
        }

        static SpamDetector()
        {
            if (!File.Exists("spamfilter.txt"))
            {
                File.WriteAllText("spamfilter.txt", "");
            }

            _regexps = new List<string>(File.ReadAllLines("spamfilter.txt"));
        }

        public static bool IsSpam(string msg)
        {
            if (_regexps == null || _regexps.Count == 0)
                return false;

            while (true)
            {
                var matches = Regex.Matches(msg, "\\[img=[/A-z0-9+=]{16,64512}\\]");

                if (matches.Count > 0)
                {
                    msg = msg.Replace(matches[0].Value, "");
                }
                else
                {
                    break;
                }
            }

            foreach (var reg in _regexps)
            {
                if (Regex.IsMatch(msg, reg)) return true;
            }

            return false;
        }
    }
}