using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public class FunCommands : BaseCommandModule
    {
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
            "postor", "estable", "teligente", "festado", "parable"};

            Random rnd = new Random();
            int random = rnd.Next(options.Length);
            string chosenOne;

            if (options[random] != "DORADO")
            {
                chosenOne = "Eli'n " + options[random] + " | Invocado por: " + ctx.Member.Mention;
            }
            else
            {
                chosenOne = "Te ha salido un Eli DORADOU!! " + ctx.Member.DisplayName + " ha sido MUTEADOVICH";
                // poner en bold eli te ha elegido
                await ctx.Member.SetMuteAsync(true, "Le toco el eli baneado");
            }

            await ctx.Channel.SendMessageAsync(chosenOne).ConfigureAwait(false);
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
    }
}
