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
using DSharpPlus.VoiceNext;

namespace Discord_Bot.Modulos
{
    public class Misc : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("invite")]
        [Aliases("invitar")]
        [Description("Invitacón del bot para que se una a un server")]
        public async Task Invite(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("Puedes invitarme a un servidor con este link: " + "https://discordapp.com/api/oauth2/authorize?client_id=295182825521545218&scope=bot&permissions=1");
        }

        [Command("ping")]
        [Description("Pong")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.Channel.SendMessageAsync(ctx.Client.Ping.ToString() + " ms").ConfigureAwait(false);
        }

        [Command("say")]
        [Aliases("s")]
        [Description("El bot reenvia tu mensaje eliminándolo después")]
        public async Task Say(CommandContext ctx, [RemainingText][Description("Tu mensaje")]string mensaje)
        {
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(mensaje).ConfigureAwait(false);
        }

        [Command("tts")]
        [Description("Te habla la waifu")]
        public async Task TTS(CommandContext ctx, [RemainingText][Description("Tu mensaje")]string mensaje)
        {
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(mensaje, true).ConfigureAwait(false);
        }

        [Command("pregunta")]
        [Aliases("p", "question", "sisonon")]
        [Description("Responde con SIS O NON")]
        public async Task Sisonon(CommandContext ctx, [RemainingText][Description("La pregunta en cuestion")]string pregunta)
        {
            Random rnd = new Random();
            int random = rnd.Next(2);
            switch (random)
            {
                case 0:
                    await ctx.Channel.SendMessageAsync("**Pregunta:** " + pregunta + "\n**Respuesta:** NON"/* + "\n**Preguntado por:** " + ctx.Member.Mention*/).ConfigureAwait(false);
                    break;
                case 1:
                    await ctx.Channel.SendMessageAsync("**Pregunta:** " + pregunta + "\n**Respuesta:** SIS"/* + "\n**Preguntado por:** " + ctx.Member.Mention*/).ConfigureAwait(false);
                    break;
                default:
                    await ctx.Channel.SendMessageAsync("Algo salió mal").ConfigureAwait(false);
                    break;
            }
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
                string options = "Opciones:";
                foreach (string msj in opciones)
                {
                    options += "\n   - " + msj;
                }
                await ctx.Message.DeleteAsync().ConfigureAwait(false);
                await mensajeBot.DeleteAsync().ConfigureAwait(false);
                await msg.Result.DeleteAsync().ConfigureAwait(false);
                await ctx.TriggerTypingAsync();
                await ctx.Channel.SendMessageAsync("Pregunta: " + pregunta + "\n\n" + options + "\n\nRespuesta: " + opciones[random] + "\n\nPreguntado por: " + ctx.User.Mention).ConfigureAwait(false);
            }
            else
            {
                await ctx.TriggerTypingAsync();
                await ctx.RespondAsync("No escribiste las opciones, sos una pija " + ctx.User.Mention);
            }
        }

        [Command("donar")]
        [Aliases("support", "donate")]
        [Description("Enlace de donación uwu")]
        public async Task Donate(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("Puedes apoyarme a comprarme una dakimakura en: " + "https://www.paypal.me/marianoburguete");
        }

        /*
        [Command("tipeo")]
        public async Task WaitForCode(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var codebytes = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(codebytes);

            var code = BitConverter.ToString(codebytes).ToLower().Replace("-", "");

            await ctx.RespondAsync($"El primer que escriba el codigo ganará: `{code}`");

            var msg = await interactivity.WaitForMessageAsync(xm => xm.Content.Contains(code), TimeSpan.FromSeconds(60));
            if (!msg.TimedOut)
            {
                await ctx.RespondAsync($"El ganador es: {msg.Result.Author.Mention}");
            }
            else
            {
                await ctx.RespondAsync("Nadie escribió el codigo, son una pija");
            }
        }
        */
        /*
        [Command("response")]
        [Description("Responde una reacción con un emoji")]
        public async Task Response(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForReactionAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(message.Result.Emoji).ConfigureAwait(false);
        }*/
        /*
        [Command("encuesta")]
        [Description("Le mandas la duracion y unos cuantos emojis y con eso te hace una encuesta")]
        public async Task Poll(CommandContext ctx, [Description("Tiempo limite de la encuesta")]TimeSpan duracion, [Description("Emojis para encuesta")]params DiscordEmoji[] emojis)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var options = emojis.Select(x => x.ToString());

            var pollEmbed = new DiscordEmbedBuilder
            {
                Title = "Encuesta",
                Description = string.Join(" ", options)
            };

            var pollMessage = await ctx.Channel.SendMessageAsync(embed: pollEmbed).ConfigureAwait(false);

            foreach (var option in emojis)
            {
                await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
            }

            var result = await interactivity.CollectReactionsAsync(pollMessage, duracion).ConfigureAwait(false);
            var distinctResult = result.Distinct();


            var results = distinctResult.Select(x => $"{x.Emoji}: {x.Total}");

            await ctx.Channel.SendMessageAsync(string.Join("\n", results)).ConfigureAwait(false);
        }*/
    }
}
