
namespace Yumiko.Model.Entities.AnimeThemes
{
    public class Animetheme
    {
        public long Id { get; set; }

        public string Type { get; set; }

        public long? Sequence { get; set; }

        public object Group { get; set; }

        public string Slug { get; set; }

        public List<AnimeThemeEntry> Animethemeentries { get; set; }

        public string? GetSequence()
        {
            if (Sequence == null) return null;

            string s = Sequence.ToString()!;
            while (s.Length < 2) s = "0" + s;
            return s;
        }
    }
}