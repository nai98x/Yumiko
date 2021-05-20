using System.Collections.Generic;

namespace Discord_Bot
{
    public class Character
    {
        public string NameFirst { get; set; }
        public string NameLast { get; set; }
        public string NameFull { get; set; }
        public string Image { get; set; }
        public string SiteUrl { get; set; }
        public List<Anime> Animes { get; set; }
        public int Favoritos { get; set; }
        public int Popularidad { get; set; }
        public Anime AnimePrincipal { get; set; }
    }
}
