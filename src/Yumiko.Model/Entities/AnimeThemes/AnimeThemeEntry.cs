
namespace Yumiko.Model.Entities.AnimeThemes
{
    public class AnimeThemeEntry
    {
        public long Id { get; set; }

        public long? Version { get; set; }

        public string Episodes { get; set; }

        public bool Nsfw { get; set; }

        public bool Spoiler { get; set; }

        public string Notes { get; set; }

        public List<Video> Videos { get; set; }
    }
}