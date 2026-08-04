namespace Yumiko.Model.Entities.AnimeThemes
{
    public class AnithemeData
    {
        public AnimeAniTheme anime;
        public Animetheme theme;
        public AnimeThemeEntry song;
        public Video video;

        public AnithemeData(AnimeAniTheme anime, Animetheme theme, AnimeThemeEntry song, Video video)
        {
            this.anime = anime;
            this.theme = theme;
            this.song = song;
            this.video = video;
        }
    }
}
