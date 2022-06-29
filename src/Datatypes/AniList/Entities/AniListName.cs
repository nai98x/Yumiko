using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class AniListName
    {
        [JsonProperty("full")]
        public string Full { get; set; }
    }
}
