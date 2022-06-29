using Newtonsoft.Json;

namespace Yumiko.Datatypes
{
    public class ProfileFavourites
    {
        [JsonProperty("anime")]
        public NodesMedia Anime { get; set; } = null!;

        [JsonProperty("manga")]
        public NodesMedia Manga { get; set; } = null!;

        [JsonProperty("characters")]
        public NodesCharacter Characters { get; set; } = null!;

        [JsonProperty("staff")]
        public NodesStaff Staff { get; set; } = null!;

        [JsonProperty("studios")]
        public NodesStudios Studios { get; set; } = null!;
    }
}
