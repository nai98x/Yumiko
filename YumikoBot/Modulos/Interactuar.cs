using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Modulos
{
    public class Interactuar : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("say"), Aliases("s"), Description("Yumiko habla en el chat.")]
        public async Task Say(CommandContext ctx, [Description("Mensaje para replicar")][RemainingText]string mensaje)
        {
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(mensaje).ConfigureAwait(false);
        }

        [Command("pregunta"), Aliases("p", "question", "sisonon"), Description("Responde con SIS O NON.")]
        public async Task Sisonon(CommandContext ctx, [Description("La pregunta que le quieres hacer")][RemainingText]string pregunta)
        {
            Random rnd = new Random();
            int random = rnd.Next(2);
            switch (random)
            {
                case 0:
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Footer = funciones.GetFooter(ctx),
                        Color = DiscordColor.Red,
                        Title = "¿SIS O NON?",
                        Description = "**Pregunta:** " + pregunta + "\n**Respuesta:** NON"
                    }).ConfigureAwait(false);
                    break;
                case 1:
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Footer = funciones.GetFooter(ctx),
                        Color = DiscordColor.Green,
                        Title = "¿SIS O NON?",
                        Description = "**Pregunta:** " + pregunta + "\n**Respuesta:** SIS"
                    }).ConfigureAwait(false);
                    break;
            }
            await ctx.Message.DeleteAsync();
        }

        [Command("elegir"), Aliases("e"), Description("Elige entre varias opciones.")]
        public async Task Elegir(CommandContext ctx, [Description("La pregunta de la cuál se eligirá una respuesta")][RemainingText]string pregunta)
        {
            var interactivity = ctx.Client.GetInteractivity();
            DiscordMessage mensajeBot = await ctx.Channel.SendMessageAsync("Ingrese las opciones separadas por comas").ConfigureAwait(false);
            var msg = await interactivity.WaitForMessageAsync(xm => xm.Author == ctx.User, TimeSpan.FromSeconds(60));
            if (!msg.TimedOut)
            {
                List<string> opciones = new List<string>();
                string msgResponse = msg.Result.Content;
                opciones = msgResponse.Split(',').ToList();
                Random rnd = new Random();
                int random = rnd.Next(opciones.Count);
                string options = "**Opciones:**";
                foreach (string msj in opciones)
                {
                    options += "\n   - " + msj;
                }
                await ctx.Message.DeleteAsync().ConfigureAwait(false);
                await mensajeBot.DeleteAsync().ConfigureAwait(false);
                await msg.Result.DeleteAsync().ConfigureAwait(false);

                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    Title = "Pregunta",
                    Description = "** " + pregunta + "**\n\n" + options + "\n\n**Respuesta:** " + opciones[random]
                }).ConfigureAwait(false);
            }
            else
            {
                await ctx.RespondAsync("No escribiste las opciones onii-chan" + ctx.User.Mention);
            }
        }

        [Command("emote"), Aliases("emoji"), Description("Muestra un emote en grande."), RequireGuild]
        public async Task Emote(CommandContext ctx, [Description("El emote para agrandar")] DiscordEmoji emote)
        {
            if (emote != null)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor(),
                    Title = emote.Name,
                    ImageUrl = emote.Url
                });
            }
            else
            {
                DiscordMessage msgError = await ctx.RespondAsync("Debes pasar un emote").ConfigureAwait(false);
                await Task.Delay(3000);
                await msgError.DeleteAsync("Auto borrado de yumiko").ConfigureAwait(false);
            }
            await ctx.Message.DeleteAsync("Auto borrado de yumiko");
        }

        [Command("avatar"), Description("Muestra el avatar de un usuario."), RequireGuild]
        public async Task Avatar(CommandContext ctx, DiscordUser usuario = null)
        {
            if (usuario == null)
            {
                usuario = ctx.User;
            }
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Title = $"Avatar de {usuario.Username}#{usuario.Discriminator}",
                ImageUrl = usuario.AvatarUrl,
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor()
            }).ConfigureAwait(false);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
        }
    }
}
