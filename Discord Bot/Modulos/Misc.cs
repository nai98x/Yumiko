using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace Discord_Bot.Modulos
{
    public class Misc : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("ping")]
        [Description("Pong")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Color = new DiscordColor(78, 63, 96),
                Description = "🏓 Pong! `" + ctx.Client.Ping.ToString() + " ms" + "`"
            }).ConfigureAwait(false);
        }

        [Command("say")]
        [Aliases("s")]
        [Description("El bot reenvia tu mensaje eliminándolo después")]
        public async Task Say(CommandContext ctx, [RemainingText][Description("Tu mensaje")]string mensaje)
        {
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(mensaje).ConfigureAwait(false);
        }
        
        [Command("pregunta")]
        [Aliases("p", "question", "sisonon")]
        [Description("Responde con SIS O NON")]
        public async Task Sisonon(CommandContext ctx, [RemainingText][Description("La pregunta en cuestion")]string pregunta)
        {
            Random rnd = new Random();
            int random = rnd.Next(2);
            EmbedFooter footer = new EmbedFooter()
            {
                Text = "Preguntado por " + funciones.GetFooter(ctx),
                IconUrl = ctx.Member.AvatarUrl
            };
            switch (random)
            {
                case 0:
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Footer = footer,
                        Color = DiscordColor.Red,
                        Title = "¿SIS O NON?",
                        Description = "**Pregunta:** " + pregunta + "\n**Respuesta:** NON"
                    }).ConfigureAwait(false);
                    break;
                case 1:
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Footer = footer,
                        Color = DiscordColor.Green,
                        Title = "¿SIS O NON?",
                        Description = "**Pregunta:** " + pregunta + "\n**Respuesta:** SIS"
                    }).ConfigureAwait(false);
                    break;
            }
            await ctx.Message.DeleteAsync();
        }

        [Command("elegir")]
        [Aliases("e")]
        [Description("Elige entre varias opciones")]
        public async Task Elegir(CommandContext ctx, [RemainingText][Description("La pregunta en cuestion")]string pregunta)
        {
            var interactivity = ctx.Client.GetInteractivity();
            DiscordMessage mensajeBot = await ctx.Channel.SendMessageAsync("Ingrese las opciones separadas por un espacio").ConfigureAwait(false);
            var msg = await interactivity.WaitForMessageAsync(xm => xm.Author == ctx.User, TimeSpan.FromSeconds(60));
            if (!msg.TimedOut)
            {
                List<string> opciones = new List<string>();
                string msgResponse = msg.Result.Content;
                opciones = msgResponse.Split(" ").ToList();
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

                EmbedFooter footer = new EmbedFooter()
                {
                    Text = "Preguntado por " + funciones.GetFooter(ctx),
                    IconUrl = ctx.Member.AvatarUrl
                };
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                {
                    Footer = footer,
                    Color = new DiscordColor(78, 63, 96),
                    Title = "Pregunta",
                    Description = "**Pregunta:** " + pregunta + "\n\n" + options + "\n\n**Respuesta:** " + opciones[random]
                }).ConfigureAwait(false);
            }
            else
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync("No escribiste las opciones, sos una pija " + ctx.User.Mention);
            }
        }
        
        [Command("invite")]
        [Aliases("invitar")]
        [Description("Invitacón del bot para que se una a un server")]
        public async Task Invite(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("Puedes invitarme a un servidor con este link:\n" + ConfigurationManager.AppSettings["Invite"]);
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
        }
    }
}
