using System.Collections.Generic;

namespace Discord_Bot
{
    public class Anime
    {
        public string TitleRomaji { get; set; }
        public string TitleEnglish { get; set; }
        public string Image { get; set; }
        public string SiteUrl { get; set; }
        public int Favoritos { get; set; }
        public List<string> Sinonimos { get; set; }
    }
}
