using System;
using System.Security.Cryptography;
using System.Text;

namespace nboard
{
    static class HashCalculator
    {
        public const int HashCrop = 16;

        public static Hash Calculate(string raw)
        {
            byte[] bhash = SHA256.Create().ComputeHash(NanoEncoding.GetBytes(raw));
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < HashCrop; i++)
            {
                sb.Append(bhash[i].ToString("x2"));
            }

            return new Hash(sb.ToString());
        }

        [Obsolete]
        public static Hash CalculateOld(string raw)
        {
            byte[] bhash = MD5.Create().ComputeHash(NanoEncoding.GetBytes(raw));
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < HashCrop; i++)
            {
                sb.Append(bhash[i].ToString("x2"));
            }

            return new Hash(sb.ToString());
        }
    }
}