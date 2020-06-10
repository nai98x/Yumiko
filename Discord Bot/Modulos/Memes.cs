using DSharpPlus.CommandsNext;
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
            if(waifuLevel < 25)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = footer,
                    Color = DiscordColor.Red,
                    Title = "Nivel de waifu",
                    Description = "El nivel de husbando de **" + nombre + "** conmigo es de **" + waifuLevel + "%**"
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
                    Description = "El nivel de husbando de **" + nombre + "** conmigo es de **" + waifuLevel + "%**"
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
                    Description = "El nivel de husbando de **" + nombre + "** conmigo es de **" + waifuLevel + "%**"
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
                    Description = "El nivel de husbando de **" + nombre + "** conmigo es de **" + waifuLevel + "%**"
                    //ImageUrl = url
                }).ConfigureAwait(false);
            }
            await ctx.Message.DeleteAsync();
        }
    }
}
