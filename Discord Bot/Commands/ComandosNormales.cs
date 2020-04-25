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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class ComandosNormales : BaseCommandModule
    {
        private FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("ping")]
        [Description("Retorna Pong")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);
        }

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
                // poner en bold eli te ha elegido
                await ctx.Member.SetMuteAsync(true, "Le toco el eli dorado (MUTE)");
                await ctx.Channel.SendMessageAsync(chosenOne).ConfigureAwait(false);
                await Task.Delay(1000 * 60);
                await ctx.Member.SetMuteAsync(false, "Le toco el eli dorado (UNMUTE)");
                await ctx.Channel.SendMessageAsync(ctx.Member.Mention + " ha sido DESMUTEADISIMO").ConfigureAwait(false);
            }
        }

        [Command("math")]
        [Description("Hace la suma, resta, multiplicacion o division entre 2 numeros")]
        public async Task Add(CommandContext ctx,
            [Description("Primer numero")]float numberOne,
            [Description("Elegido utilizando + - * /")]string elegido,
            [Description("Segundo numero")]float numberTwo)
        {
            float res;
            switch (elegido) {
                case "+":
                    res = numberOne + numberTwo;
                    await ctx.Channel.SendMessageAsync(res.ToString()).ConfigureAwait(false);
                    break;
                case "-":
                    res = numberOne - numberTwo;
                    await ctx.Channel.SendMessageAsync(res.ToString()).ConfigureAwait(false);
                    break;
                case "*":
                    res = numberOne * numberTwo;
                    await ctx.Channel.SendMessageAsync(res.ToString()).ConfigureAwait(false);
                    break;
                case "/":
                    res = numberOne / numberTwo;
                    await ctx.Channel.SendMessageAsync(res.ToString()).ConfigureAwait(false);
                    break;
                default:
                    await ctx.Channel.SendMessageAsync("Escribi el operador bien hijo de la grandisima puta").ConfigureAwait(false);
                    break;
            }
        }

        [Command("pregunta")]
        [Description("SIS O NON")]
        public async Task Sisonon(CommandContext ctx, string mensaje)
        {
            Random rnd = new Random();
            int random = rnd.Next(2);
            switch (random)
            {
                case 0:
                    await ctx.Channel.SendMessageAsync("Pregunta: " + mensaje +" | Respuesta: NON" + " | Preguntado por: " + ctx.Member.Mention).ConfigureAwait(false);
                    break;
                case 1:
                    await ctx.Channel.SendMessageAsync("Pregunta: " + mensaje + " | Respuesta: SIS" + " | Preguntado por: " + ctx.Member.Mention).ConfigureAwait(false);
                    break;
                default:
                    await ctx.Channel.SendMessageAsync("Algo salió mal").ConfigureAwait(false);
                    break;
            }
        }

        [Command("response")]
        public async Task Response(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForReactionAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(message.Result.Emoji).ConfigureAwait(false);
        }

        [Command("encuesta")]
        public async Task Poll(CommandContext ctx, TimeSpan duration, params DiscordEmoji[] emojiOptions)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var options = emojiOptions.Select(x => x.ToString());

            var pollEmbed = new DiscordEmbedBuilder
            {
                Title = "Encuesta",
                Description = string.Join(" ", options)
            };

            var pollMessage = await ctx.Channel.SendMessageAsync(embed: pollEmbed).ConfigureAwait(false);

            foreach(var option in emojiOptions)
            {
                await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
            }

            var result = await interactivity.CollectReactionsAsync(pollMessage, duration).ConfigureAwait(false);
            var distinctResult = result.Distinct();


            var results = distinctResult.Select(x => $"{x.Emoji}: {x.Total}");

            await ctx.Channel.SendMessageAsync(string.Join("\n", results)).ConfigureAwait(false);
        }

        [Command("meme")]
        public async Task ImagenRandom(CommandContext ctx)
        {
            string url = funciones.GetImagenRandomMeme();
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Imagen posteada por: " + ctx.Member.DisplayName,
                ImageUrl = url
            }).ConfigureAwait(false);
        }

    }
}
