using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class ProfileResponse
    {
        [JsonProperty("User")]
        public User? User { get; set; }
    }
}
