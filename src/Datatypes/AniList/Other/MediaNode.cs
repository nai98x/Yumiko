﻿using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class MediaNode
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("type")]
        public MediaType Type { get; set; }

        [JsonProperty("title")]
        public MediaTitle Title { get; set; } = null!;

        [JsonProperty("siteUrl")]
        public Uri SiteUrl { get; set; } = null!;
    }
}
