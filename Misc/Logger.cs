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
            try
            {
                File.AppendAllText(ErrLog, new string('#', 100) + "\r\n");
            }
            catch
            {
            }
        }

        public static void LogDrawLine()
        {
            try
            {
                File.AppendAllText(InfoLog, new string('#', 100) + "\r\n");
            }
            catch
            {
                //File.AppendAllText(InfoLog, new string('#', 100) + "\r\n");
            }
        }

        public static void LogError(string msg, params object[] objs)
        {
            try
            {
                File.AppendAllText(ErrLog, string.Format(msg + "\r\n", objs));
            }
            catch
            {
                //File.AppendAllText(ErrLog, string.Format(msg + "\r\n", objs));
            };
        }

        public static void Log(string msg, params object[] objs)
        {
            try
            {
                File.AppendAllText(InfoLog, string.Format(msg + "\r\n", objs));
            }
            catch
            {
                //File.AppendAllText(InfoLog, string.Format(msg + "\r\n", objs));
            }
        }
    }
    
}