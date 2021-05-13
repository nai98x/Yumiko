using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YumikoBot
{
    public struct ConfigJson
    {
        [JsonProperty("token_prod")]
        public string TokenProd { get; private set; }

        [JsonProperty("token_test")]
        public string TokenTest { get; private set; }

        [JsonProperty("firestore_url")]
        public string Firestore_url { get; private set; }

        [JsonProperty("firestore_secret")]
        public string Firestore_secret { get; private set; }

        [JsonProperty("topgg_token")]
        public string TopGG_token { get; private set; }
    }
}
