namespace Discord_Bot
{
    using System.Collections.Generic;

    public class Anime
    {
        public string TitleRomaji { get; set; }
        public string TitleEnglish { get; set; }
        public string Image { get; set; }
        public string SiteUrl { get; set; }
        public int Favoritos { get; set; }
        public List<string> Sinonimos { get; set; }
        public int Popularidad { get; set; }
        public List<Estudio> Estudios { get; set; }
        public List<Character> Personajes { get; set; }
    }
}
