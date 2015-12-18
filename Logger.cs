using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace nboard
{
    static class Logger
    {
        private const string InfoLog = "info.log";
        private const string ErrLog = "error.log";

        public static void LogErrorDrawLine()
        {
            File.AppendAllText(ErrLog, new string('#', 100) + "\n");
        }

        public static void LogDrawLine()
        {
            File.AppendAllText(InfoLog, new string('#', 100) + "\n");
        }

        public static void LogError(string msg, params object[] objs)
        {
            File.AppendAllText(ErrLog, string.Format(msg + "\n", objs));
        }

        public static void Log(string msg, params object[] objs)
        {
            File.AppendAllText(InfoLog, string.Format(msg + "\n", objs));
        }
    }
    
}