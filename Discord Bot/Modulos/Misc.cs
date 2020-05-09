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

        [Command("say")]
        [Aliases("s")]
        [Description("El bot reenvia tu mensaje eliminandolo después")]
        public async Task Say(CommandContext ctx, [Description("Tu mensaje")]params string[] mensajes)
        {
            string mensaje = "";
            for (int i = 0; i < mensajes.Length; i++)
            {
                mensaje += mensajes[i] + " ";
            }

            await ctx.Message.DeleteAsync().ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync(mensaje).ConfigureAwait(false);
        }

        [Command("pregunta")]
        [Aliases("p", "question", "sisonon")]
        [Description("Responde con SIS O NON")]
        public async Task Sisonon(CommandContext ctx, [Description("La pregunta en cuestion")]params string[] pregunta)
        {
            string mensaje = "";
            for (int i = 0; i < pregunta.Length; i++)
            {
                mensaje += pregunta[i] + " ";
            }

            Random rnd = new Random();
            int random = rnd.Next(2);
            switch (random)
            {
                case 0:
                    await ctx.Channel.SendMessageAsync("Pregunta: " + mensaje + "| Respuesta: NON" + " | Preguntado por: " + ctx.Member.Mention).ConfigureAwait(false);
                    break;
                case 1:
                    await ctx.Channel.SendMessageAsync("Pregunta: " + mensaje + "| Respuesta: SIS" + " | Preguntado por: " + ctx.Member.Mention).ConfigureAwait(false);
                    break;
                default:
                    await ctx.Channel.SendMessageAsync("Algo salió mal").ConfigureAwait(false);
                    break;
            }
        }

        [Command("elegir")]
        [Aliases("e")]
        [Description("Elige entre varias opciones")]
        public async Task Elegir(CommandContext ctx, [Description("La pregunta en cuestion")]params string[] pregunta)
        {
            var interactivity = ctx.Client.GetInteractivity();
            string question = "";
            for (int i = 0; i < pregunta.Length; i++)
            {
                question += pregunta[i] + " ";
            }
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
                await ctx.Channel.SendMessageAsync("Pregunta: " + question + "\n\n" + options + "\n\nRespuesta: " + opciones[random] + "\n\nPreguntado por: " + ctx.User.Mention).ConfigureAwait(false);
            }
            else
            {
                await ctx.RespondAsync("No escribiste las opciones, sos una pija " + ctx.User.Mention);
            }
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
