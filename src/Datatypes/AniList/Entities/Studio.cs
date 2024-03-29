﻿using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class Studio
    {
        [JsonProperty("name")]
        public string Name { get; set; } = null!;

        [JsonProperty("siteUrl")]
        public Uri SiteUrl { get; set; } = null!;

        [JsonProperty("isAnimationStudio")]
        public bool? IsAnimationStudio { get; set; }
    }
}
