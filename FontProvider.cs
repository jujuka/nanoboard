using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace nboard
{
    static class FontProvider
    {
        public static Font Get(float size)
        {
            return new Font(FontFamily.GenericSansSerif, size);
        }

        public static Font Get()
        {
            return Get(12);
        }
    }
    
}