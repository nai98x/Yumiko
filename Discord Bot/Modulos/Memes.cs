using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using Org.BouncyCastle.Crypto.Digests;
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

        [Command("waifu")]
        [Description("Te digo si soy tu waifu")]
        public async Task Waifu(CommandContext ctx)
        {
            Random rnd = new Random();
            int random = rnd.Next(2);
            int waifuLevel;
            if(ctx.User.Id == 198212314892075009) // Yo
            {
                if (random == 0) 
                    waifuLevel = rnd.Next(101); // 0 al 100
                else
                    waifuLevel = 50 + rnd.Next(51); // 50 al 100
            }
            else
            {
                if (random == 0) 
                    waifuLevel = rnd.Next(101); // 0 al 100
                else
                    waifuLevel = rnd.Next(50); // 0 al 49
            }

            EmbedFooter footer = new EmbedFooter()
            {
                Text = "Preguntado por " + ctx.Member.DisplayName + " (" + ctx.Member.Username + "#" + ctx.Member.Discriminator + ")"
            };
            if(waifuLevel < 25)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = footer,
                    Color = DiscordColor.Red,
                    Title = "Nivel de waifu",
                    Description = "Tu nivel de husbando conmigo es de **" + waifuLevel + "%**"
                    //ImageUrl = url
                }).ConfigureAwait(false);
            }
            if(waifuLevel >= 25 && waifuLevel < 50)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = footer,
                    Color = DiscordColor.Orange,
                    Title = "Nivel de waifu",
                    Description = "Tu nivel de husbando conmigo es de **" + waifuLevel + "%**"
                    //ImageUrl = url
                }).ConfigureAwait(false);
            }
            if(waifuLevel >= 50 && waifuLevel < 75)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = footer,
                    Color = DiscordColor.Yellow,
                    Title = "Nivel de waifu",
                    Description = "Tu nivel de husbando conmigo es de **" + waifuLevel + "%**"
                    //ImageUrl = url
                }).ConfigureAwait(false);
            }
            if (waifuLevel >= 75)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = footer,
                    Color = DiscordColor.Green,
                    Title = "Nivel de waifu",
                    Description = "Tu nivel de husbando conmigo es de **" + waifuLevel + "%**"
                    //ImageUrl = url
                }).ConfigureAwait(false);
            }
            await ctx.Message.DeleteAsync();
        }


    }
}
