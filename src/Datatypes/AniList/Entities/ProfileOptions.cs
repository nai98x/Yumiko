using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class ProfileOptions
    {
        [JsonProperty("titleLanguage")]
        public string TitleLanguage { get; set; } = null!;

        [JsonProperty("displayAdultContent")]
        public bool DisplayAdultContent { get; set; }

        [JsonProperty("profileColor")]
        public string ProfileColor { get; set; } = null!;
    }
}
