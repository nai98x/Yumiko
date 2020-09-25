using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

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
    }

    public class Imagen
    {
        public string url { get; set; }
        public DiscordUser autor { get; set; }
    }
}
