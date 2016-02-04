using Newtonsoft.Json;
using NDB;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Text;

namespace NDB
{
    interface IPostDb
    {
        string[] GetAllHashes();
        bool PutPost(Post p);
        bool DeletePost(string hash);
        Post GetPost(string hash);
        Post GetNthPost(int n);
        int GetPostCount();
        Post[] GetReplies(string hash);
        void Flush();
    }
}