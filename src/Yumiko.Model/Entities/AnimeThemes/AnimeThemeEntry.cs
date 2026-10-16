
namespace Yumiko.Model.Entities.AnimeThemes
{
    public class AnimeThemeEntry
    {
        public long Id { get; set; }

        public long? Version { get; set; }

        public string Episodes { get; set; } = null!;

        public bool Nsfw { get; set; }

        public bool Spoiler { get; set; }

        public string Notes { get; set; } = null!;

        public List<Video> Videos { get; set; } = null!;
    }
}