using Newtonsoft.Json;

namespace NDB
{
    class DbPostRef
    {
        [JsonProperty("h")]
        public string hash;
        [JsonProperty("r")]
        public string replyTo;
        [JsonProperty("o")]
        public int offset;
        [JsonProperty("l")]
        public int length;
        [JsonProperty("d")]
        public bool deleted;
        [JsonProperty("f")]
        public string file;
    }
}