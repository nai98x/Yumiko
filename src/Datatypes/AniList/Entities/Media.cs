using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class Media
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public MediaTitle Title { get; set; } = null!;

        [JsonProperty("synonyms")]
        public List<string>? Synonyms { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("siteUrl")]
        public Uri SiteUrl { get; set; } = null!;

        [JsonProperty("coverImage")]
        public MediaCoverImage CoverImage { get; set; } = null!;

        [JsonProperty("bannerImage")]
        public string? BannerImage { get; set; }

        [JsonProperty("format")]
        public MediaFormat? Format { get; set; }

        [JsonProperty("volumes")]
        public int? Volumes { get; set; }

        [JsonProperty("chapters")]
        public int? Chapters { get; set; }

        [JsonProperty("episodes")]
        public int? Episodes { get; set; }

        [JsonProperty("status")]
        public MediaStatus? Status { get; set; }

        [JsonProperty("meanScore")]
        public int? MeanScore { get; set; }

        [JsonProperty("genres")]
        public List<string>? Genres { get; set; }

        [JsonProperty("seasonYear")]
        public int? SeasonYear { get; set; }

        [JsonProperty("startDate")]
        public FuzzyDate StartDate { get; set; } = null!;

        [JsonProperty("endDate")]
        public FuzzyDate EndDate { get; set; } = null!;

        [JsonProperty("tags")]
        public List<Tag>? Tags { get; set; }

        [JsonProperty("studios")]
        public StudioConnection? Studios { get; set; }

        [JsonProperty("externalLinks")]
        public List<ExternalLink>? ExternalLinks { get; set; }

        [JsonProperty("isAdult")]
        public bool IsAdult { get; set; }
    }
}
