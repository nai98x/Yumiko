
namespace Yumiko.Model.Entities.AnimeThemes
{
    public class AnimeAniTheme
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;

        public string Slug { get; set; } = null!;

        public long Year { get; set; }

        public string Season { get; set; } = null!;

        public string Synopsis { get; set; } = null!;

        public List<Animetheme> Animethemes { get; set; } = null!;
    }
}
