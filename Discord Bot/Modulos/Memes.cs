using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace Discord_Bot.Modulos
{
    public class Memes : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("eli")]
        [Description("Legendary meme")]
        public async Task Eli(CommandContext ctx)
        {

            string opcionRandom = funciones.GetEliRandom();
            string chosenOne;

            if (opcionRandom != "DORADO")
            {
                chosenOne = "Eli'n " + opcionRandom + " | Invocado por: " + ctx.Member.Mention;
                await ctx.Channel.SendMessageAsync(chosenOne).ConfigureAwait(false);
            }
            else
            {
                chosenOne = "Te ha salido un Eli DORADOU!! " + ctx.Member.Mention + " se fue MUTEADISIMO por 1 minuto";
                await ctx.Member.SetMuteAsync(true, "Le toco el eli dorado (MUTE)");
                await ctx.Channel.SendMessageAsync(chosenOne).ConfigureAwait(false);
                await Task.Delay(1000 * 60);
                await ctx.Member.SetMuteAsync(false, "Le toco el eli dorado (UNMUTE)");
                await ctx.Channel.SendMessageAsync(ctx.Member.Mention + " ha sido DESMUTEADISIMO").ConfigureAwait(false);
            }
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
        }

        [Command("meme")]
        [Description("It's a fucking meme")]
        public async Task ImagenRandom(CommandContext ctx)
        {
            string url = funciones.GetImagenRandomMeme();
            EmbedFooter footer = new EmbedFooter()
            {
                Text = "Imagen posteada por " + ctx.Member.DisplayName
            };
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Footer = footer,
                ImageUrl = url
            }).ConfigureAwait(false);
        }
    }
}
