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

        [Command("say"), Aliases("s"), Description("Yumiko habla en el chat."), Hidden]
        public async Task Say(CommandContext ctx, [Description("Mensaje para replicar")][RemainingText] string mensaje = null)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("sayO"), Description("Yumiko habla en el chat."), Hidden, RequireOwner]
        public async Task SayO(CommandContext ctx, ulong guildId, ulong channelId, [Description("Mensaje para replicar")][RemainingText] string mensaje)
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
                            Text = $"Enviado por {ctx.User.Username}"
                        },
                        Title = "Mensaje del administrador",
                        Description = mensaje
                    }).ConfigureAwait(false);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("Canal no encontrado");
                }
            }
            else
            {
                await ctx.Channel.SendMessageAsync("Servidor no encontrado");
            }
        }

        [Command("pregunta"), Aliases("p", "question", "siono"), Description("Responde con SI O NO."), Hidden]
        public async Task Sisonon(CommandContext ctx, [Description("La pregunta que le quieres hacer")][RemainingText] string pregunta)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("elegir"), Aliases("e"), Description("Elige entre varias opciones."), Hidden]
        public async Task Elegir(CommandContext ctx, [Description("La pregunta de la cuál se eligirá una respuesta")][RemainingText] string pregunta)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("emote"), Aliases("emoji"), Description("Muestra un emote en grande."), RequireGuild]
        public async Task Emote(CommandContext ctx, [Description("El emote para agrandar")] DiscordEmoji emote = null)
        {
            if (emote != null)
            {
                if (emote.Id != 0)
                {
                    string animado = emote.IsAnimated switch
                    {
                        true => "Si",
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
                    }.AddField("Nombre", $"{emote.Name}", true)
                    .AddField("Identificador", $"{emote.Id}", true)
                    .AddField("Animado", $"{animado}", true)
                    .AddField("Fecha de creación", $"{funciones.UppercaseFirst(dia)} {emote.CreationTimestamp.Day} de {mes} del {emote.CreationTimestamp.Year}")
                    );
                }
                else
                {
                    await ctx.Channel.SendMessageAsync(emote).ConfigureAwait(false);
                }
            }
            else
                {
                DiscordMessage msgError = await ctx.Channel.SendMessageAsync("Debes pasar un emote").ConfigureAwait(false);
                await Task.Delay(3000);
                await funciones.BorrarMensaje(ctx, msgError.Id);
            }
        }

        [Command("sticker"), Description("Muestra un sticker."), RequireGuild]
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
                    DiscordMessage msgError = await ctx.Channel.SendMessageAsync("Debes pasar un sticker").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await funciones.BorrarMensaje(ctx, msgError.Id);
                    break;
                default:
                    DiscordMessage msgError1 = await ctx.Channel.SendMessageAsync("Solo puedes pasar un sticker").ConfigureAwait(false);
                    await Task.Delay(3000);
                    await funciones.BorrarMensaje(ctx, msgError1.Id);
                    break;
            }
        }

        [Command("avatar"), Description("Muestra el avatar de un usuario."), RequireGuild, Hidden]
        public async Task Avatar(CommandContext ctx, DiscordMember usuario = null)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("waifu"), Description("Te dice mi nivel de waifu hacia un usuario."), Hidden]
        public async Task Waifu(CommandContext ctx, [Description("Miembro para ver su nivel de waifu, si se deja vacío lo hace de tí")] DiscordMember miembro = null)
        {
            await funciones.MovidoASlashCommand(ctx);
        }

        [Command("love"), Description("Muestra el porcentaje de amor entre dos usuarios."), RequireGuild, Hidden]
        public async Task Love(CommandContext ctx, DiscordMember user1 = null, DiscordMember user2 = null)
        {
            await funciones.MovidoASlashCommand(ctx);
        }
    }
}
