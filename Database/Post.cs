using Newtonsoft.Json;
using NDB;

namespace NDB
{
    class Post
    {
        [JsonProperty("hash")]
        public string hash;
        [JsonProperty("message")]
        public string message;
        [JsonProperty("replyTo")]
        public string replyto;

        public Post()
        {
        }

        public Post(string r, string m)
        {
            replyto = r;
            message = m;
            hash = HashCalculator.Calculate(r + m.FromB64());
        }
    }
}