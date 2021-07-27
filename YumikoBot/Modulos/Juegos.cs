using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using System;
using System.Configuration;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus;

namespace Discord_Bot.Modulos
{
    public class Juegos : BaseCommandModule 
    {
        private readonly FuncionesAuxiliares funciones = new();
        private readonly FuncionesJuegos funcionesJuegos = new();

        [Command("quiz"), Description("Empieza el juego de adivinar algo relacionado con el anime."), RequireGuild, Cooldown(1, 60, CooldownBucketType.Guild)]
        public async Task QuizGeneral(CommandContext ctx)
        {
            var context = funciones.GetContext(ctx);
            var interactivity = ctx.Client.GetInteractivity();

            DiscordComponentEmoji emote = new(DiscordEmoji.FromName(ctx.Client, ":game_die:"));
            DiscordButtonComponent buttonAleatorio = new(ButtonStyle.Primary, "0", string.Empty, emoji: emote);
            DiscordButtonComponent buttonPersonaje = new(ButtonStyle.Primary, "1", "Personaje");
            DiscordButtonComponent buttonAnime = new(ButtonStyle.Primary, "2", "Anime");
            DiscordButtonComponent buttonManga = new(ButtonStyle.Primary, "3", "Manga");
            DiscordButtonComponent buttonTag = new(ButtonStyle.Primary, "4", "Tag");
            DiscordButtonComponent buttonEstudio = new(ButtonStyle.Primary, "5", "Estudio");
            DiscordButtonComponent buttonProtagonista = new(ButtonStyle.Primary, "6", "Protagonista");
            DiscordButtonComponent buttonGenero = new(ButtonStyle.Primary, "7", "Genero");

            DiscordMessageBuilder mensaje = new()
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
                        await funcionesJuegos.QuizCharactersGlobal(context);
                        break;
                    case "2":
                        await funcionesJuegos.QuizAnimeGlobal(context);
                        break;
                    case "3":
                        await funcionesJuegos.QuizMangaGlobal(context);
                        break;
                    case "4":
                        await funcionesJuegos.QuizAnimeTagGlobal(context);
                        break;
                    case "5":
                        await funcionesJuegos.QuizStudioGlobal(context);
                        break;
                    case "6":
                        await funcionesJuegos.QuizProtagonistGlobal(context);
                        break;
                    case "7":
                        await funcionesJuegos.QuizGenreGlobal(context);
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

        [Command("ahorcado"), Description("Empieza el juego del ahorcado de algo relacionado con el anime."), RequireGuild]
        public async Task AhorcadoGeneral(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            DiscordComponentEmoji emote = new(DiscordEmoji.FromName(ctx.Client, ":game_die:"));
            DiscordButtonComponent buttonAleatorio = new(ButtonStyle.Primary, "0", string.Empty, emoji: emote);
            DiscordButtonComponent buttonPersonaje = new(ButtonStyle.Primary, "1", "Personaje");
            DiscordButtonComponent buttonAnime = new(ButtonStyle.Primary, "2", "Anime");

            DiscordMessageBuilder mensaje = new()
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
                    int random = funciones.GetNumeroRandom(1,2);
                    game = random.ToString();
                }
                if (msgElegir != null)
                    await funciones.BorrarMensaje(ctx, msgElegir.Id);
                int pag;
                switch (game)
                {
                    case "1":
                        pag = funciones.GetNumeroRandom(1, 5000);
                        Character personaje = await funciones.GetRandomCharacter(ctx, pag);
                        if (personaje != null)
                        {
                            await funcionesJuegos.JugarAhorcado(ctx, personaje, "personaje");
                        }
                        break;
                    case "2":
                        pag = funciones.GetNumeroRandom(1, 5000);
                        Anime anime = await funciones.GetRandomMedia(ctx, pag, "anime");
                        if (anime != null)
                        {
                            await funcionesJuegos.JugarAhorcado(ctx, anime, "anime");
                        }
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

        [Command("ranking"), Aliases("stats", "leaderboard"), Description("Estadisticas de un quiz."), RequireGuild]
        public async Task EstadisticasGenerales(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var context = funciones.GetContext(ctx);

            DiscordButtonComponent buttonPersonaje = new(ButtonStyle.Primary, "1", "Personaje");
            DiscordButtonComponent buttonAnime = new(ButtonStyle.Primary, "2", "Anime");
            DiscordButtonComponent buttonManga = new(ButtonStyle.Primary, "3", "Manga");
            DiscordButtonComponent buttonTag = new(ButtonStyle.Primary, "4", "Tag");
            DiscordButtonComponent buttonEstudio = new(ButtonStyle.Primary, "5", "Estudio");
            DiscordButtonComponent buttonProtagonista = new(ButtonStyle.Primary, "6", "Protagonista");
            DiscordButtonComponent buttonGenero = new(ButtonStyle.Primary, "7", "Genero");

            DiscordMessageBuilder mensaje = new()
            {
                Embed = new DiscordEmbedBuilder
                {
                    Title = "Elije las estadisticas que quieres ver",
                    Description = $"{ctx.User.Mention}, haz click en un boton para continuar"
                }
            };

            mensaje.AddComponents(buttonPersonaje, buttonAnime, buttonManga, buttonTag);
            mensaje.AddComponents(buttonEstudio, buttonProtagonista, buttonGenero);

            DiscordMessage msgElegir = await mensaje.SendAsync(ctx.Channel);
            var interJuego = await interactivity.WaitForButtonAsync(msgElegir, ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
            if (!interJuego.TimedOut)
            {
                var resultJuego = interJuego.Result;
                string estadisticaJuego = resultJuego.Id;
                if (msgElegir != null)
                    await funciones.BorrarMensaje(ctx, msgElegir.Id);
                DiscordEmbedBuilder builder;
                switch (estadisticaJuego)
                {
                    case "1":
                        builder = await funcionesJuegos.GetEstadisticas(context, "personaje");
                        await ctx.Channel.SendMessageAsync(embed: builder);
                        await funciones.ChequearVotoTopGG(context);
                        break;
                    case "2":
                        builder = await funcionesJuegos.GetEstadisticas(context, "anime");
                        await ctx.Channel.SendMessageAsync(embed: builder);
                        await funciones.ChequearVotoTopGG(context);
                        break;
                    case "3":
                        builder = await funcionesJuegos.GetEstadisticas(context, "manga");
                        await ctx.Channel.SendMessageAsync(embed: builder);
                        await funciones.ChequearVotoTopGG(context);
                        break;
                    case "4":
                        builder = await funcionesJuegos.GetEstadisticasTag(context);
                        var msg = await ctx.Channel.SendMessageAsync(embed: builder);
                        await funciones.ChequearVotoTopGG(context);
                        if (builder.Title == "Error")
                        {
                            await Task.Delay(5000);
                            await funciones.BorrarMensaje(ctx, msg.Id);
                        }
                        break;
                    case "5":
                        builder = await funcionesJuegos.GetEstadisticas(context, "estudio");
                        await ctx.Channel.SendMessageAsync(embed: builder);
                        await funciones.ChequearVotoTopGG(context);
                        break;
                    case "6":
                        builder = await funcionesJuegos.GetEstadisticas(context, "protagonista");
                        await ctx.Channel.SendMessageAsync(embed: builder);
                        await funciones.ChequearVotoTopGG(context);
                        break;
                    case "7":
                        var respuesta = await funcionesJuegos.ElegirGenero(context, interactivity);
                        if (respuesta.Ok)
                        {
                            builder = await funcionesJuegos.GetEstadisticasGenero(context, respuesta.Genero);
                            await ctx.Channel.SendMessageAsync(embed: builder);
                            await funciones.ChequearVotoTopGG(context);
                        }
                        break;
                }
            }
            else
            {
                var msgError = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "Tiempo agotado esperando las estadisticas del juego",
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

        [Command("eliminarestadisticas"), Description("Elimina las estadisticas de todos los juegos del servidor."), RequireGuild] // AGREGARLE BOTONES
        public async Task EliminarEstadisticas(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            string opcion;
            string opciones =
                $"**1-** Si\n" +
                $"**2-** No\n\n" +
                $"**Ten en cuenta que el borrado de estadisticas no se puede deshacer.**";
            var msgElegir = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Confirma si quieres eliminar tus estadisticas del servidor",
                Description = opciones,
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
                    case "1- si":
                    case "si":
                        await funcionesJuegos.EliminarEstadisticas(ctx);
                        break;
                    case "2":
                    case "2- no":
                    case "no":
                        break;
                    default:
                        var msgError = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "Error",
                            Description = "Opcion incorrecta",
                            Footer = funciones.GetFooter(ctx),
                            Color = DiscordColor.Red,
                        });
                        await Task.Delay(3000);
                        if (msgError != null)
                            await funciones.BorrarMensaje(ctx, msgError.Id);
                        break;
                }
            }
            if (msgElegir != null)
                await funciones.BorrarMensaje(ctx, msgElegir.Id);
            if (msgElegirInter.Result != null)
                await funciones.BorrarMensaje(ctx, msgElegirInter.Result.Id);
        }
    }
}
