
namespace Yumiko.Model.Entities.AnimeThemes
{
    public class Video
    {
        public long Id { get; set; }

        public string Basename { get; set; }

        public string Filename { get; set; }

        public string Path { get; set; }

        public long Size { get; set; }

        public string Mimetype { get; set; }

        public long Resolution { get; set; }

        public bool Nc { get; set; }

        public bool Subbed { get; set; }

        public bool Lyrics { get; set; }

        public bool Uncen { get; set; }

        public string Source { get; set; }

        public string Overlap { get; set; }

        public string Tags { get; set; }

        public string Link { get; set; }
    }
}
