﻿using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class NodesStudios
    {
        [JsonProperty("nodes")]
        public List<Studio>? Nodes { get; set; }
    }
}