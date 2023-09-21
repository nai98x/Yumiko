using Newtonsoft.Json;
using System.Collections.Generic;

namespace Yumiko.Datatypes
{
    public class MediaUserListResponse
    {
        [JsonProperty("MediaListCollection")]
        public MediaListUserCollection MediaListCollection { get; set; }
    }
}
