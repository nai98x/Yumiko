using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class User
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = null!;

        [JsonProperty("siteUrl")]
        public Uri SiteUrl { get; set; } = null!;

        [JsonProperty("avatar")]
        public ProfileAvatar Avatar { get; set; } = null!;

        [JsonProperty("bannerImage")]
        public Uri? BannerImage { get; set; }

        [JsonProperty("options")]
        public ProfileOptions Options { get; set; } = null!;

        [JsonProperty("statistics")]
        public ProfileStatistics Statistics { get; set; } = null!;

        [JsonProperty("favourites")]
        public ProfileFavourites Favourites { get; set; } = null!;

        [JsonProperty("mediaListOptions")]
        public MediaListOptions MediaListOptions { get; set; } = null!;
    }
}
