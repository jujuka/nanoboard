using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Newtonsoft.Json;

namespace NServer
{
    class Configurator
    {
        private const string ConfigFileName = "config.json";
        public static readonly Configurator Instance = new Configurator();

        [JsonProperty("params")]
        private Dictionary<string, string> _keyValues = new Dictionary<string, string>();

        private Configurator()
        {
            if (File.Exists(ConfigFileName))
            {
                _keyValues = JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText(ConfigFileName));  
            }
        }

        public string GetValue(string key, string defaultValue)
        {
            if (_keyValues.ContainsKey(key)) 
                return _keyValues[key];
            _keyValues[key] = defaultValue;
            return defaultValue;
        }

        public void SetValue(string key, string value)
        {
            _keyValues[key] = value;
            var config = JsonConvert.SerializeObject(_keyValues);
            File.WriteAllText(ConfigFileName, config);
        }
    }
}