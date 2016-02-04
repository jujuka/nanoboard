using Newtonsoft.Json;
using NDB;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Text;

namespace NDB
{
    class PostsReader
    {
        public Post[] Read(string pathToJson)
        {
            var json = File.ReadAllText(pathToJson);
            var posts = JsonConvert.DeserializeObject<Post[]>(json);
            return PostsValidator.Validate(posts);
        }
    }
}