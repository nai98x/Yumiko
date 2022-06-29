using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class UserMediaMini
    {
        [JsonProperty("name")]
        public string Name { get; set; } = null!;

        [JsonProperty("avatar")]
        public ProfileAvatar Avatar { get; set; } = null!;

        [JsonProperty("mediaListOptions")]
        public MediaListOptions MediaListOptions { get; set; } = null!;
    }
}
