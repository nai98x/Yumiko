using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class CharacterImage
    {
        [JsonProperty("large")]
        public string? Large { get; set; }
    }
}
