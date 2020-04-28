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

namespace Discord_Bot.Commands
{
    public class ComandosNormales : BaseCommandModule
    {
        private FuncionesAuxiliares funciones = new FuncionesAuxiliares();

        [Command("ping")]
        [Description("Retorna Pong")]
        public async Task Ping(CommandContext ctx)
        {
            TimeSpan offset = ctx.Message.CreationTimestamp - DateTime.Now;
            double ms = offset.TotalSeconds;
            await ctx.Channel.SendMessageAsync("Pong! (" + ms.ToString() + " seg)").ConfigureAwait(false);
        }

        [Command("say")]
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
            await ctx.Message.DeleteAsync().ConfigureAwait(false);
        }
        /*
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
        }*/

        [Command("pregunta")]
        [Description("Responde con SIS O NON")]
        public async Task Sisonon(CommandContext ctx, [Description("La pregunta en cuestion")]params string[] mensajes)
        {
            string mensaje = "";
            for (int i = 0; i < mensajes.Length; i++)
            {
                mensaje += mensajes[i] + " ";
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
        [Description("Elige entre varias opciones")]
        public async Task Elegir(CommandContext ctx, [Description("Las opciones en cuestion")]params string[] opciones)
        {
            Random rnd = new Random();
            int random = rnd.Next(opciones.Length);

            string options= "Opciones:";
            foreach (string msj in opciones)
            {
                options += "\n   - " + msj;
            }
            await ctx.Channel.SendMessageAsync(options).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync("Respuesta: " + opciones[random]).ConfigureAwait(false);
        }

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

        [Command("clear")]
        [Description("Borra cierta cantidad de mensajes, requiere permisos")]
        public async Task Clear(CommandContext ctx, [Description("Cantidad de mensajes a borrar")]int cantidad)
        {
            if (funciones.TienePermisos(Permissions.ManageMessages, ctx.Member.Roles))
            {
                if (cantidad > 99)
                {
                    cantidad = 99;
                }
                await ctx.Channel.DeleteMessagesAsync(await ctx.Channel.GetMessagesAsync(cantidad + 1));
            }
            else
            {
                await ctx.Channel.SendMessageAsync("No tienes los permisos suficientes para realizar esta acción").ConfigureAwait(false);
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
    }
}
