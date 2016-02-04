using System;
using System.IO;

namespace NDB
{
    static class FileUtil
    {
        public static int Append(string path, byte[] bytes)
        {
            long pos = 0;

            using (var stream = new FileStream(path, FileMode.Append))
            {
                pos = stream.Position;
                stream.Write(bytes, 0, bytes.Length);
            }

            return (int)pos;
        }

        public static void Write(string path, byte[] bytes, int offset)
        {
            using (Stream stream = new FileStream(path, FileMode.OpenOrCreate))
            {
                stream.Seek(offset, SeekOrigin.Begin);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        public static byte[] Read(string path, int offset, int length)
        {
            var bytes = new byte[length];
            using (Stream stream = new FileStream(path, FileMode.OpenOrCreate))
            {
                stream.Seek(offset, SeekOrigin.Begin);
                stream.Read(bytes, 0, length);
            }
            return bytes;
        }
    }    
}