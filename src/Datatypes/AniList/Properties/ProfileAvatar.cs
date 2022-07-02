using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class ProfileAvatar
    {
        [JsonProperty("medium")]
        public Uri Medium { get; set; } = null!;
    }
}
