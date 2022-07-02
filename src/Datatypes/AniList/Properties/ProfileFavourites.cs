using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class ProfileFavourites
    {
        [JsonProperty("anime")]
        public MediaConnection Anime { get; set; } = null!;

        [JsonProperty("manga")]
        public MediaConnection Manga { get; set; } = null!;

        [JsonProperty("characters")]
        public CharacterConnection Characters { get; set; } = null!;

        [JsonProperty("staff")]
        public StaffConnection Staff { get; set; } = null!;

        [JsonProperty("studios")]
        public StudioConnection Studios { get; set; } = null!;
    }
}
