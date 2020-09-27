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
        public List<Character> characters { get; set; }
    }

    public class Data
    {
        public Page data { get; set; }
    }
}
