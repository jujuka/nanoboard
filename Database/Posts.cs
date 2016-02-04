using Newtonsoft.Json;
using NDB;

namespace NDB
{
    class Posts
    {
        [JsonProperty("posts")]
        public Post[] posts;
    }
}