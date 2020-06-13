﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
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
            EmbedFooter footer = new EmbedFooter()
            {
                Text = "Invocado por " + funciones.GetFooter(ctx),
                IconUrl = ctx.Member.AvatarUrl
            };
            await ctx.TriggerTypingAsync();
            if (opcionRandom != "DORADO")
            {
                DiscordGuildEmoji emoji = await ctx.Guild.GetEmojiAsync(424965118900830238);
                
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Eli Acher Weitz",
                    Color = new DiscordColor(78, 63, 96),
                    Footer = footer,
                    Description = emoji + " Eli'n " + opcionRandom + " " + emoji,
                }).ConfigureAwait(false);
            }
            else
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Eli Acher Weitz",
                    Color = DiscordColor.Gold,
                    Footer = footer,
                    ImageUrl = "https://i.imgur.com/Y84LdKx.png",
                    Description = "TE HA SALIDO UN ELI DORADO",
                }).ConfigureAwait(false);
                await ctx.RespondAsync(ctx.Member.Mention + " se fue MUTEADISIMO por 1 minuto").ConfigureAwait(false);
                await ctx.Member.SetMuteAsync(true, "Le toco el eli dorado (MUTE)");
                await Task.Delay(1000 * 60);
                await ctx.Member.SetMuteAsync(false, "Le toco el eli dorado (UNMUTE)");
                await ctx.RespondAsync(ctx.Member.Mention + " ha sido DESMUTEADISIMO").ConfigureAwait(false);
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
                Text = "Imagen posteada por " + funciones.GetFooter(ctx),
                IconUrl = ctx.Member.AvatarUrl
            };
            await ctx.TriggerTypingAsync();
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Footer = footer,
                ImageUrl = url,
                Color = new DiscordColor(78, 63, 96)
            }).ConfigureAwait(false);
        }

        [Command("waifu")]
        [Description("Te digo si soy tu waifu")]
        [Cooldown(1, 300, CooldownBucketType.User)]
        public async Task Waifu(CommandContext ctx, DiscordMember miembro = null)
        {
            string nombre;
            if (miembro == null)
                nombre = ctx.Member.DisplayName;
            else
                nombre = miembro.DisplayName;

            Random rnd = new Random();
            int waifuLevel = rnd.Next(101);
            EmbedFooter footer = new EmbedFooter()
            {
                Text = "Preguntado por " + funciones.GetFooter(ctx),
                IconUrl = ctx.Member.AvatarUrl
            };
            await ctx.TriggerTypingAsync();
            if (waifuLevel < 25)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = footer,
                    Color = DiscordColor.Red,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\nMe pego un tiro antes de tocarte, virgen de mierda.",
                    ImageUrl = "https://i.imgur.com/BOxbruw.png"
                }).ConfigureAwait(false);
            }
            if (waifuLevel >= 25 && waifuLevel < 50)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = footer,
                    Color = DiscordColor.Orange,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\nMe das asquito, mejor me alejo de vos.",
                    ImageUrl = "https://i.imgur.com/ys2HoiL.jpg"
                }).ConfigureAwait(false);
            }
            if (waifuLevel >= 50 && waifuLevel < 75)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = footer,
                    Color = DiscordColor.Yellow,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\nNo estás mal, pero no tenes posibilidades conmigo.",
                    ImageUrl = "https://i.imgur.com/jCfDrGa.png"
                }).ConfigureAwait(false);
            }
            if (waifuLevel >= 75 && waifuLevel < 100)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = footer,
                    Color = DiscordColor.Green,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\nSoy tu waifu, podes hacer lo que quieras conmigo.",
                    ImageUrl = "https://i.imgur.com/dhXR8mV.png"
                }).ConfigureAwait(false);
            }
            if (waifuLevel == 100)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = footer,
                    Color = DiscordColor.Blue,
                    Title = "Nivel de waifu",
                    Description = "Mi amor hacia **" + nombre + "** es de **" + waifuLevel + "%**\nNo puedo parar de pensar en cojer con vos.",
                    ImageUrl = "https://i.imgur.com/b5g1LEP.png"
                }).ConfigureAwait(false);
            }
            await ctx.Message.DeleteAsync();
        }

        [Command("Love")]
        [Description("Te digo el nivel de amor entre dos usuarios")]
        [Cooldown(1, 300, CooldownBucketType.User)]
        public async Task Love(CommandContext ctx, DiscordUser primero = null, DiscordUser segundo = null)
        {
            if(primero == null || segundo == null)
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync($"Debes especificar los dos usuarios, {ctx.Member.DisplayName} baka").ConfigureAwait(false);
            }

            Random rnd = new Random();
            int waifuLevel = rnd.Next(101);
            EmbedFooter footer = new EmbedFooter()
            {
                Text = "Preguntado por " + funciones.GetFooter(ctx),
                IconUrl = ctx.Member.AvatarUrl
            };

            string frase;
            if (waifuLevel < 25)
                frase = "Ustedes dos se suicidan con una lija antes de verse";
            if (waifuLevel >= 25 && waifuLevel < 50)
                frase = "Mejor que estén lejos, no son el uno para el otro";
            if (waifuLevel >= 50 && waifuLevel < 75)
                frase = "Casi pero no";
            if (waifuLevel >= 75 && waifuLevel < 100)
                frase = "Shippeo intenso incomming";
            else
                frase = "PUEDEN COJER YA? GRACIAS";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Footer = footer,
                Color = DiscordColor.Blue,
                Title = "Amor",
                Description = $"El nivel de atracción entre {primero.Username} y {segundo.Username} es de {waifuLevel}%\n{frase}",
                ImageUrl = "https://i.imgur.com/b5g1LEP.png"
            }).ConfigureAwait(false);
            await ctx.Message.DeleteAsync();
        }
    }
}
