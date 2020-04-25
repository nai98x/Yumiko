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
    public class FunCommands : BaseCommandModule
    {
        private string GetImagenRandom()
        {
            string[] options = new[] { 
                "https://static.wixstatic.com/media/1e9fdf_b9ea2c7f507c45b3ba3771012fc846c5.png/v1/fill/w_580,h_543,al_c,lg_1,q_85/1e9fdf_b9ea2c7f507c45b3ba3771012fc846c5.webp",
                "https://static.wixstatic.com/media/1e9fdf_f6590db0097546adabaeb430d1b576fb.png/v1/fill/w_580,h_417,al_c,q_85/1e9fdf_f6590db0097546adabaeb430d1b576fb.webp",
                "https://static.wixstatic.com/media/1e9fdf_5b71c799b7a8462db416ee320b10004e.jpg/v1/fill/w_580,h_362,al_c,q_80/1e9fdf_5b71c799b7a8462db416ee320b10004e.webp",
                "https://static.wixstatic.com/media/1e9fdf_ced8bdb6ddfe4781b7ed44ff11b87531.png/v1/fill/w_580,h_488,al_c,q_85/1e9fdf_ced8bdb6ddfe4781b7ed44ff11b87531.webp",
                "https://static.wixstatic.com/media/1e9fdf_0c57c48e84594767b40e0192fdbddcf7.jpg/v1/fill/w_580,h_512,al_c,lg_1,q_80/1e9fdf_0c57c48e84594767b40e0192fdbddcf7.webp",
                "https://static.wixstatic.com/media/1e9fdf_c3c917ac524b48c1912894dc816d8e25.png/v1/fill/w_580,h_327,al_c,lg_1,q_85/1e9fdf_c3c917ac524b48c1912894dc816d8e25.webp",
                "https://static.wixstatic.com/media/1e9fdf_9562e8f85e7b4687a2765f7600c9cdc9.png/v1/fill/w_580,h_383,al_c,q_85/1e9fdf_9562e8f85e7b4687a2765f7600c9cdc9.webp",
                "https://static.wixstatic.com/media/1e9fdf_758acfbca72e46ecaa0e4b3afc0ef9ac.png/v1/fill/w_580,h_773,al_c,q_90/1e9fdf_758acfbca72e46ecaa0e4b3afc0ef9ac.webp",
                "https://static.wixstatic.com/media/1e9fdf_3df69a0441c84a5ab6fe9d235b1ab922.png/v1/fill/w_580,h_303,al_c,lg_1,q_85/1e9fdf_3df69a0441c84a5ab6fe9d235b1ab922.webp",
                "https://static.wixstatic.com/media/1e9fdf_aa2f2236f9b24b249d90741734b073f0.png/v1/fill/w_480,h_597,al_c,lg_1,q_85/1e9fdf_aa2f2236f9b24b249d90741734b073f0.webp",
                "https://static.wixstatic.com/media/1e9fdf_a21af803a6fe409caa5d0d9e968a4e77.jpg/v1/fill/w_580,h_768,al_c,q_85/1e9fdf_a21af803a6fe409caa5d0d9e968a4e77.webp",
                "https://static.wixstatic.com/media/1e9fdf_d2da2775d1f24b8ead0bf2ca2268054e.png/v1/fill/w_600,h_338,al_c,lg_1,q_85/1e9fdf_d2da2775d1f24b8ead0bf2ca2268054e.webp",
                "https://static.wixstatic.com/media/1e9fdf_64ce1af185ac4556b37b80d0b1908d75~mv2.png/v1/fill/w_480,h_720,al_c,q_85/1e9fdf_64ce1af185ac4556b37b80d0b1908d75~mv2.webp",
                "https://static.wixstatic.com/media/1e9fdf_99f7e06baa8a403e83b9ceece77734e2~mv2.jpg/v1/fill/w_600,h_681,al_c,q_85,usm_0.66_1.00_0.01/1e9fdf_99f7e06baa8a403e83b9ceece77734e2~mv2.webp",
                "https://static.wixstatic.com/media/1e9fdf_8ded154c00554dd4991c96904d8dfe35~mv2.png/v1/fill/w_600,h_600,al_c,q_85,usm_0.66_1.00_0.01/1e9fdf_8ded154c00554dd4991c96904d8dfe35~mv2.webp",
            //    "",
            };

            Random rnd = new Random();
            int random = rnd.Next(options.Length);
            return options[random];
        }

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
            string[] options = new [] {"DORADO", "geniero", "dividuo", "dignado", "deciso", "fradotado", "coherente", "penetrable", "cestuoso", "accesible", "cognito",
            "adaptado", "centivado", "nombrable", "sensato", "moral", "falible", "enarrable", "putado", "tratable", "deseado", "contagiable", "clusivo", "fumable",
            "postor", "estable", "teligente", "festado", "parable", "oportuno", "cauto", "comodo", "cendios", "conforme", "advertido", "conveniente", "incivilizado",
            "capacitado", "centivado", "comodo"};

            Random rnd = new Random();
            int random = rnd.Next(options.Length);
            string chosenOne;

            if (options[random] != "DORADO")
            {
                chosenOne = "Eli'n " + options[random] + " | Invocado por: " + ctx.Member.Mention;
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
            string url = GetImagenRandom();
            await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Imagen posteada por: " + ctx.Member.DisplayName,
                ImageUrl = url
            }).ConfigureAwait(false);
        }

    }
}
