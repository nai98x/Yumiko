using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class CharacterName
    {
        [JsonProperty("full")]
        public string Full { get; set; }
    }
}
