using Newtonsoft.Json;
using NDB;
using System.Collections.Generic;
using System;

namespace NDB
{
    class Index
    {
        [JsonProperty("indexes")]
        public DbPostRef[] indexes;
    }
    
}