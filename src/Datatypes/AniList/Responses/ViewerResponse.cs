using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class ViewerResponse
    {
        [JsonProperty("Viewer")]
        public User? Viewer { get; set; }
    }
}
