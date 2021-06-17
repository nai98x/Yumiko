using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus.Entities;
using System;
using GraphQL.Client.Http;
using GraphQL;
using GraphQL.Client.Serializer.Newtonsoft;
using System.Linq;
using System.Configuration;
using DSharpPlus.Interactivity.Extensions;
using System.Text.RegularExpressions;

namespace Discord_Bot.Modulos
{
    public class Ahorcado : BaseCommandModule 
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        private readonly FuncionesJuegos funcionesJuegos = new FuncionesJuegos();

        [Command("ahorcado"), Description("Empieza el juego del ahorcado de algo relacionado con el anime."), RequireGuild]
        public async Task AhorcadoGeneral(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            DiscordComponentEmoji emote = new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":game_die:"));
            DiscordButtonComponent buttonAleatorio = new DiscordButtonComponent(ButtonStyle.Primary, "0", string.Empty, emoji: emote);
            DiscordButtonComponent buttonPersonaje = new DiscordButtonComponent(ButtonStyle.Primary, "1", "Personaje");
            DiscordButtonComponent buttonAnime = new DiscordButtonComponent(ButtonStyle.Primary, "2", "Anime");

            DiscordMessageBuilder mensaje = new DiscordMessageBuilder()
            {
                Embed = new DiscordEmbedBuilder
                {
                    Title = "Elije el tipo de juego",
                    Description = $"{ctx.User.Mention}, haz click en un boton para continuar"
                }
            };

            mensaje.AddComponents(buttonAleatorio, buttonPersonaje, buttonAnime);

            DiscordMessage msgElegir = await mensaje.SendAsync(ctx.Channel);
            var interGame = await interactivity.WaitForButtonAsync(msgElegir, ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
            if (!interGame.TimedOut)
            {
                var resultGame = interGame.Result;
                string game = resultGame.Id;
                if (game == "0")
                {
                    Random rnd = new Random();
                    int random = rnd.Next(2);
                    game = random.ToString();
                }
                if (msgElegir != null)
                    await funciones.BorrarMensaje(ctx, msgElegir.Id);
                switch (game)
                {
                    case "0":
                        await AhorcadoPersonaje(ctx);
                        break;
                    case "1":
                        await AhorcadoAnime(ctx);
                        break;
                }
            }
            else
            {
                var msgError = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "Tiempo agotado esperando el juego",
                    Footer = funciones.GetFooter(ctx),
                    Color = DiscordColor.Red,
                });
                await Task.Delay(3000);
                if (msgError != null)
                    await funciones.BorrarMensaje(ctx, msgError.Id);
                if (msgElegir != null)
                    await funciones.BorrarMensaje(ctx, msgElegir.Id);
            }
        }

        [Command("ahorcadoc"), Aliases("hangmanc"), Description("Empieza el juego del ahorcado con un personaje aleatorio."), RequireGuild]
        public async Task AhorcadoPersonaje(CommandContext ctx)
        {
            int pag = funciones.GetNumeroRandom(1, 5000);
            Character personaje = await funciones.GetRandomCharacter(ctx, pag);
            if (personaje != null)
            {
                await funcionesJuegos.JugarAhorcado(ctx, personaje, "personaje");
            }
        }

        [Command("ahorcadoa"), Aliases("hangmana"), Description("Empieza el juego del ahorcado con un anime aleatorio."), RequireGuild]
        public async Task AhorcadoAnime(CommandContext ctx)
        {
            int pag = funciones.GetNumeroRandom(1, 5000);
            Anime anime = await funciones.GetRandomMedia(ctx, pag, "anime");
            if (anime != null)
            {
                await funcionesJuegos.JugarAhorcado(ctx, anime, "anime");
            }
        }
    }
}
