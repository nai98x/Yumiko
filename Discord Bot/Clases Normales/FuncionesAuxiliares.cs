using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace Discord_Bot
{
    public class FuncionesAuxiliares
    {
        public string GetImagenRandomMeme(List<string> opciones)
        {
            Random rnd = new Random();
            return opciones[rnd.Next(opciones.Count)];
        }

        public string GetImagenRandomShip()
        {
            string[] opciones = new string[]
            {
                "https://i.imgur.com/nmXB1j3.gif",
                "https://i.imgur.com/apvPrPH.gif",
                "https://i.imgur.com/x3L3O3l.gif",
                "https://i.imgur.com/AgsLqLO.gif",
                "https://i.imgur.com/G4YLOal.gif",
                "https://i.imgur.com/gp3bj3R.gif",
                "https://i.imgur.com/EgmqF5t.gif",
                "https://i.imgur.com/aLSqypv.gif",
                "https://i.imgur.com/EI09P4S.gif"
            };
            Random rnd = new Random();
            return opciones[rnd.Next(opciones.Length -1)];
        }

        public string GetFooter(CommandContext ctx)
        {
            return ctx.Member.DisplayName + " (" + ctx.Member.Username + "#" + ctx.Member.Discriminator + ")";
        }

        public string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }

    public class Imagen
    {
        public string Url { get; set; }
        public DiscordUser Autor { get; set; }
    }

    public class name
    {
        public string full { get; set; }
    }

    public class image
    {
        public string large { get; set; }
    }

    public class Character
    {
        public name Name { get; set; }
        public image Image { get; set; }
    }

    public class Page
    {
        public List<Miki.Anilist.ICharacter> characters { get; set; }
    }

    public class Data
    {
        public Page data { get; set; }
    }
}
