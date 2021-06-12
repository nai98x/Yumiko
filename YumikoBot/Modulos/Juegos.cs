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
    public class Juegos : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        private readonly FuncionesJuegos funcionesJuegos = new FuncionesJuegos();
        private readonly GraphQLHttpClient graphQLClient = new GraphQLHttpClient("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        [Command("quiz"), Description("Empieza el juego de adivinar algo relacionado con el anime."), RequireGuild]
        public async Task Quiz(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            DiscordComponentEmoji emote = new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":game_die:"));
            DiscordButtonComponent buttonAleatorio = new DiscordButtonComponent(ButtonStyle.Primary, "0", string.Empty, emoji: emote);
            DiscordButtonComponent buttonPersonaje = new DiscordButtonComponent(ButtonStyle.Primary, "1", "Personaje");
            DiscordButtonComponent buttonAnime = new DiscordButtonComponent(ButtonStyle.Primary, "2", "Anime");
            DiscordButtonComponent buttonManga = new DiscordButtonComponent(ButtonStyle.Primary, "3", "Manga");
            DiscordButtonComponent buttonTag = new DiscordButtonComponent(ButtonStyle.Primary, "4", "Tag");
            DiscordButtonComponent buttonEstudio = new DiscordButtonComponent(ButtonStyle.Primary, "5", "Estudio");
            DiscordButtonComponent buttonProtagonista = new DiscordButtonComponent(ButtonStyle.Primary, "6", "Protagonista");
            DiscordButtonComponent buttonGenero = new DiscordButtonComponent(ButtonStyle.Primary, "7", "Genero");

            DiscordMessageBuilder mensaje = new DiscordMessageBuilder()
            {
                Embed = new DiscordEmbedBuilder
                {
                    Title = "Elije el tipo de juego",
                    Description = $"{ctx.User.Mention}, haz click en un boton para continuar"
                }
            };

            mensaje.AddComponents(buttonAleatorio, buttonPersonaje, buttonAnime, buttonManga);
            mensaje.AddComponents(buttonTag, buttonEstudio, buttonProtagonista, buttonGenero);

            DiscordMessage msgElegir = await mensaje.SendAsync(ctx.Channel);
            var interJuego = await interactivity.WaitForButtonAsync(msgElegir, ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
            if (!interJuego.TimedOut)
            {
                var resultJuego = interJuego.Result;
                string juego = resultJuego.Id;
                if (juego == "0")
                {
                    var dificultadNum = funciones.GetNumeroRandom(1, 7);
                    juego = dificultadNum.ToString();
                }
                if (msgElegir != null)
                    await funciones.BorrarMensaje(ctx, msgElegir.Id);
                switch (juego)
                {
                    case "1":
                        await QuizCharactersGlobal(ctx);
                        break;
                    case "2":
                        await QuizAnimeGlobal(ctx);
                        break;
                    case "3":
                        await QuizMangaGlobal(ctx);
                        break;
                    case "4":
                        await QuizAnimeTagGlobal(ctx);
                        break;
                    case "5":
                        await QuizStudioGlobal(ctx);
                        break;
                    case "6":
                        await QuizProtagonistGlobal(ctx);
                        break;
                    case "7":
                        await QuizGenreGlobal(ctx);
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

        [Command("quizC"), Aliases("adivinaelpersonaje", "characterquiz"), Description("Empieza el juego de adivina el personaje."), RequireGuild]
        public async Task QuizCharactersGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity, true, false, false);
            if (settings.Ok)
            {
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el personaje",
                    Description = $"Sesión iniciada por {ctx.User.Mention}\n\nPuedes escribir `cancelar` en cualquiera de las rondas para terminar la partida.",
                    Color = funciones.GetColor()
                }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                var characterList = await funcionesJuegos.GetCharacters(ctx, settings, false);
                await funcionesJuegos.Jugar(ctx, "personaje", characterList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        [Command("quizA"), Aliases("adivinaelanime", "animequiz"), Description("Empieza el juego de adivina el anime."), RequireGuild]
        public async Task QuizAnimeGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity, true, false, false);
            if (settings.Ok)
            {
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el anime",
                    Description = $"Sesión iniciada por {ctx.User.Mention}\n\nPuedes escribir `cancelar` en cualquiera de las rondas para terminar la partida.",
                    Color = funciones.GetColor()
                }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                var characterList = await funcionesJuegos.GetCharacters(ctx, settings, true);
                await funcionesJuegos.Jugar(ctx, "anime", characterList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        [Command("quizM"), Aliases("adivinaelmanga"), Description("Empieza el juego de adivina el manga."), RequireGuild]
        public async Task QuizMangaGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity, true, false, false);
            if (settings.Ok)
            {
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el manga",
                    Description = $"Sesión iniciada por {ctx.User.Mention}\n\nPuedes escribir `cancelar` en cualquiera de las rondas para terminar la partida.",
                    Color = funciones.GetColor()
                }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                var animeList = await funcionesJuegos.GetMedia(ctx, "MANGA", settings, false, false, false, false);
                await funcionesJuegos.Jugar(ctx, "manga", animeList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        [Command("quizT"), Aliases("adivinaeltag"), Description("Empieza el juego de adivina el anime de cierto tag."), RequireGuild]
        public async Task QuizAnimeTagGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity,false,true, false);
            if (settings.Ok)
            {
                settings.PorcentajeTag = 70;
                var animeList = await funcionesJuegos.GetMedia(ctx, "ANIME", settings, false, false, true, false);
                int cantidadAnimes = animeList.Count();
                if (cantidadAnimes > 0)
                {
                    if (cantidadAnimes < settings.Rondas)
                    {
                        settings.Rondas = cantidadAnimes;
                        await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Color = DiscordColor.Yellow,
                            Title = $"Rondas reducidas",
                            Description = $"Se han reducido el numero de rondas a {settings.Rondas} ya que esta es la cantidad de animes con al menos un {settings.PorcentajeTag}% de {settings.Tag}",
                        }).ConfigureAwait(false);
                    }
                    settings.Dificultad = settings.Tag;
                    await funcionesJuegos.Jugar(ctx, "tag", animeList, settings, interactivity);
                }
                else
                {
                    settings.Ok = false;
                    settings.MsgError = "No hay ningun anime con este tag con al menos 70%";
                }
            }
            if(!settings.Ok)
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(3000);
                await funciones.BorrarMensaje(ctx, error.Id);    
            }
        }

        [Command("quizS"), Aliases("adivinaelestudio"), Description("Empieza el juego de adivina el estudio."), RequireGuild]
        public async Task QuizStudioGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity, true, false, false);
            if (settings.Ok)
            {
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el estudio del anime",
                    Description = $"Sesión iniciada por {ctx.User.Mention}\n\nPuedes escribir `cancelar` en cualquiera de las rondas para terminar la partida.",
                    Color = funciones.GetColor()
                }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                var animeList = await funcionesJuegos.GetMedia(ctx, "ANIME", settings, false, true, false, false);
                await funcionesJuegos.Jugar(ctx, "estudio", animeList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        [Command("quizP"), Aliases("adivinaelprotagonista"), Description("Empieza el juego de adivina el protagonista."), RequireGuild]
        public async Task QuizProtagonistGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity, true, false, false);
            if (settings.Ok)
            {
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el protagonista del anime",
                    Description = $"Sesión iniciada por {ctx.User.Mention}\n\nPuedes escribir `cancelar` en cualquiera de las rondas para terminar la partida.",
                    Color = funciones.GetColor()
                }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                var animeList = await funcionesJuegos.GetMedia(ctx, "ANIME", settings, true, false, false, false);
                await funcionesJuegos.Jugar(ctx, "protagonista", animeList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        [Command("quizG"), Aliases("adivinaelgenero"), Description("Empieza el juego de adivina el género."), RequireGuild]
        public async Task QuizGenreGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await funcionesJuegos.InicializarJuego(ctx, interactivity, false, false, true);
            if (settings.Ok)
            {
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = $"Adivina el {settings.Genero}",
                    Description = $"Sesión iniciada por {ctx.User.Mention}\n\nPuedes escribir `cancelar` en cualquiera de las rondas para terminar la partida.",
                    Color = funciones.GetColor()
                }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                await ctx.Channel.SendMessageAsync(embed: embebido).ConfigureAwait(false);
                var animeList = await funcionesJuegos.GetMedia(ctx, "ANIME", settings, true, false, false, true);
                await funcionesJuegos.Jugar(ctx, "genero", animeList, settings, interactivity);
            }
            else
            {
                var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                await Task.Delay(5000);
                await funciones.BorrarMensaje(ctx, error.Id);
            }
        }

        [Command("ahorcado"), Description("Empieza el juego del ahorcado de algo relacionado con el anime."), RequireGuild]
        public async Task Ahorcado(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            string opcion;
            string juegos =
                $"**1-** Personaje\n" +
                $"**2-** Anime\n";
            var msgElegir = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Elige el tipo de juego para el ahorcado",
                Description = juegos,
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
            });
            var msgElegirInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGeneral"])));
            if (!msgElegirInter.TimedOut)
            {
                opcion = msgElegirInter.Result.Content;
                if (msgElegir != null)
                    await funciones.BorrarMensaje(ctx, msgElegir.Id);
                if (msgElegirInter.Result != null)
                    await funciones.BorrarMensaje(ctx, msgElegirInter.Result.Id);
                opcion = opcion.ToLower();
                switch (opcion)
                {
                    case "1":
                    case "1- Personaje":
                    case "Personaje":
                        await AhorcadoPersonaje(ctx);
                        break;
                    case "2":
                    case "2- Anime":
                    case "Anime":
                        await AhorcadoAnime(ctx);
                        break;
                    default:
                        var msgError = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "Error",
                            Description = "Opcion de juego incorrecta",
                            Footer = funciones.GetFooter(ctx),
                            Color = DiscordColor.Red,
                        });
                        await Task.Delay(3000);
                        if (msgError != null)
                            await funciones.BorrarMensaje(ctx, msgError.Id);
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
