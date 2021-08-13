using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class Interactuar : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new();

        [Command("sayO"), Description("Administrator message by Yumiko."), Hidden, RequireOwner]
        public async Task SayO(CommandContext ctx, ulong guildId, ulong channelId, [Description("Mssage to send")][RemainingText] string mensaje)
        {
            var guild = await ctx.Client.GetGuildAsync(guildId);
            if (guild != null)
            {
                var channel = guild.GetChannel(channelId);
                if (channel != null && funciones.ChequearPermisoYumiko(ctx, DSharpPlus.Permissions.SendMessages))
                {
                    await channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                        Color = funciones.GetColor(),
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            IconUrl = ctx.User.AvatarUrl,
                            Text = $"Sent by {ctx.User.Username}"
                        },
                        Title = "Message from Yumiko's owner",
                        Description = mensaje
                    }).ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("Channel not found");
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("Guild not found");
            }
        }

        [Command("emote"), Aliases("emoji"), Description("Shows an emote."), RequireGuild]
        public async Task Emote(CommandContext ctx, [Description("Emote")] DiscordEmoji emote = null)
        {
            if (emote != null)
            {
                if (emote.Id != 0)
                {
                    string animado = emote.IsAnimated switch
                    {
                        true => "Yes",
                        false => "No",
                    };
                    string dia = emote.CreationTimestamp.ToString("dddd", CultureInfo.CreateSpecificCulture("es"));
                    string mes = emote.CreationTimestamp.ToString("MMMM", CultureInfo.CreateSpecificCulture("es"));

                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Footer = funciones.GetFooter(ctx),
                        Color = funciones.GetColor(),
                        Title = "Emote",
                        ImageUrl = emote.Url
                    }.AddField("Name", $"{emote.Name}", true)
                    .AddField("Id", $"{emote.Id}", true)
                    .AddField("Animated", $"{animado}", true)
                    .AddField("Creation date", $"{funciones.UppercaseFirst(dia)} {emote.CreationTimestamp.Day} de {mes} del {emote.CreationTimestamp.Year}")
                    );
                }
                else
                {
                    await ctx.Channel.SendMessageAsync(emote).ConfigureAwait(false);
                }
            }
            else
                {
                DiscordMessage msgError = await ctx.Channel.SendMessageAsync("You must pass an emotee").ConfigureAwait(false);
                await Task.Delay(3000);
                await funciones.BorrarMensaje(ctx, msgError.Id);
            }
        }

        [Command("sticker"), Description("Shows a sticker."), RequireGuild]
        public async Task Sticker(CommandContext ctx)
        {
            int cantStickers = ctx.Message.Stickers.Count;
            switch (cantStickers)
            {
                case 1:
                    var sticker = ctx.Message.Stickers.ElementAt(0);
                    await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = sticker.Name,
                        Url = sticker.StickerUrl,
                        ImageUrl = sticker.StickerUrl,
                        Color = funciones.GetColor(),
                        Footer = funciones.GetFooter(ctx)
                    });
                    break;
                case 0:
                    DiscordMessage msgError = await ctx.Channel.SendMessageAsync("You must pass a sticker").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await funciones.BorrarMensaje(ctx, msgError.Id);
                    break;
                default:
                    DiscordMessage msgError1 = await ctx.Channel.SendMessageAsync("You can only pass one sticker").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await funciones.BorrarMensaje(ctx, msgError1.Id);
                    break;
            }
        }
    }
}
