using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class ProfileResponse
    {
        [JsonProperty("User")]
        public Profile? User { get; set; }
    }
}
