using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace nboard
{
    static class NanoEncoding
    {
        static string charset = 
"?!\"#$%&'()*+,-./0123456789:;<=> @ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~"+
"ЎўЄєІіЇїАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюяčšžćśźńŭłČŠŽĆŚŽŽŬŁ" +
"№©«»±®Ґґ°™—“”’‘…–\n\r\t";

        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length];
            int i = 0;

            foreach (var c in str)
            {
                int iof = charset.IndexOf(c);
                if (iof == -1) bytes[i++] = 0;
                else bytes[i++] = (byte)iof;
            }

            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length];
            int i = 0;

            foreach (var b in bytes)
            {
                if (b > charset.Length) chars[i++] = '?';
                else chars[i++] = charset[b];
            }

            return new string(chars);
        }
    }
    
}