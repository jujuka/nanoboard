using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace nboard
{
    interface IMailer
    {
        void ReadInbox(NanoDB to);
        void FillOutbox(NanoDB from);
    }
}