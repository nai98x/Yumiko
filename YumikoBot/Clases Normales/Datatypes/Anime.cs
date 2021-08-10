using System.Collections.Generic;

namespace Discord_Bot
{
    public class Anime
    {
        public int Id { get; set; }
        public string TitleRomaji { get; set; }
        public string TitleEnglish { get; set; }
        public string TitleRomajiFormatted { get; set; }
        public string TitleEnglishFormatted { get; set; }
        public string Image { get; set; }
        public string SiteUrl { get; set; }
        public int AvarageScore { get; set; }
        public int Favoritos { get; set; }
        public List<string> Sinonimos { get; set; }
        public int Popularidad { get; set; }
        public List<Estudio> Estudios { get; set; }
        public List<Character> Personajes { get; set; }
    }
}
