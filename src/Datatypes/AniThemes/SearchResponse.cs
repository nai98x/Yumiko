using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class SearchResponse
    {
        [JsonProperty("search")]
        public Search Search { get; set; }
    }
}
