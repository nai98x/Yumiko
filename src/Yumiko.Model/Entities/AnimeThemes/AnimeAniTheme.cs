
namespace Yumiko.Model.Entities.AnimeThemes
{
    public class AnimeAniTheme
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Slug { get; set; }

        public long Year { get; set; }

        public string Season { get; set; }

        public string Synopsis { get; set; }

        public List<Animetheme> Animethemes { get; set; }
    }
}
