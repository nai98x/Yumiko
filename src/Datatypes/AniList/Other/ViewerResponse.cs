using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class ViewerResponse
    {
        [JsonProperty("Viewer")]
        public Profile? Viewer { get; set; }
    }
}
