using Discord_Bot;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace YumikoBot
{
    public class JuegosSlashCommands : SlashCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new();
        private readonly FuncionesJuegos funcionesJuegos = new();

        [SlashCommand("quiz", "Empieza un juego de adivinar algo de anime")]
        public async Task Quiz(InteractionContext ctx,
        [Choice("Adivina el personaje", "personajes")]
        [Choice("Adivina el anime", "animes")]
        [Choice("Adivina el manga", "mangas")]
        [Choice("Adivina el tag", "tag")]
        [Choice("Adivina el estudio", "estudio")]
        [Choice("Adivina el protagonista", "protagonista")]
        [Choice("Adivina el genero", "genero")]
        [Option("Juego", "El tipo de quiz que quieres iniciar")] string juego,
        [Choice("Fácil", "Fácil")]
        [Choice("Media", "Media")]
        [Choice("Dificil", "Dificil")]
        [Choice("Extremo", "Extremo")]
        [Option("Dificultad", "Elige que tan dificl quieres que sea el quiz")] string dificultad,
        [Option("Rondas", "La cantidad de rondas que tendrá el quiz")] long rondas)
        {
            int iterIni, iterFin;
            switch (dificultad)
            {
                case "Fácil":
                    iterIni = 1;
                    iterFin = 10;
                    break;
                case "Media":
                    iterIni = 10;
                    iterFin = 30;
                    break;
                case "Dificil":
                    iterIni = 30;
                    iterFin = 60;
                    break;
                case "Extremo":
                    iterIni = 60;
                    iterFin = 100;
                    break;
                default:
                    iterIni = 10;
                    iterFin = 30;
                    break;
            }

            if (rondas <= 0 )
            {
                rondas = 10;
            }
            if (rondas > 100)
            {
                rondas = 100;
            }
            var settings = new SettingsJuego
            {
                Rondas = (int)rondas,
                Dificultad = dificultad,
                IterIni = iterIni,
                IterFin = iterFin,
                Ok = true
            };

            var context = funciones.GetContext(ctx);
            var interactivity = ctx.Client.GetInteractivity();
            switch (juego)
            {
                case "personajes":
                    DiscordEmbed embebido = new DiscordEmbedBuilder
                    {
                        Title = "Adivina el personaje",
                        Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                        Color = funciones.GetColor()
                    }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embebido));
                    var characterList = await funcionesJuegos.GetCharacters(context, settings, false);
                    await funcionesJuegos.Jugar(context, "personaje", characterList, settings, interactivity);
                    break;
                case "animes":
                    DiscordEmbed embebido2 = new DiscordEmbedBuilder
                    {
                        Title = "Adivina el anime",
                        Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                        Color = funciones.GetColor()
                    }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embebido2));
                    var characterList2 = await funcionesJuegos.GetCharacters(context, settings, true);
                    await funcionesJuegos.Jugar(context, "anime", characterList2, settings, interactivity);
                    break;
                case "mangas":
                    DiscordEmbed embebido3 = new DiscordEmbedBuilder
                    {
                        Title = "Adivina el manga",
                        Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                        Color = funciones.GetColor()
                    }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embebido3));
                    var animeList = await funcionesJuegos.GetMedia(context, "MANGA", settings, false, false, false, false);
                    await funcionesJuegos.Jugar(context, "manga", animeList, settings, interactivity);
                    break;
                case "tag":
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    {
                        IsEphemeral = true,
                        Content = $"¡Haz iniciado un nuevo quiz!"
                    });
                    SettingsJuego settingsTag = await funcionesJuegos.InicializarJuego(context, interactivity, true, false);
                    settings.Ok = settingsTag.Ok;
                    if (settings.Ok)
                    {
                        settings.Tag = settingsTag.Tag;
                        settings.TagDesc = settingsTag.TagDesc;
                        DiscordEmbed embebido4 = new DiscordEmbedBuilder
                        {
                            Title = $"Adivina el tag",
                            Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                            Color = funciones.GetColor(),
                            Footer = funciones.GetFooter(ctx)
                        }.AddField("Rondas", $"{settings.Rondas}").AddField("Tag", $"{settings.Tag}");
                        await ctx.Channel.SendMessageAsync(embebido4);
                        settings.PorcentajeTag = 70;
                        var animeList2 = await funcionesJuegos.GetMedia(context, "ANIME", settings, false, false, true, false);
                        int cantidadAnimes = animeList2.Count;
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
                            await funcionesJuegos.Jugar(context, "tag", animeList2, settings, interactivity);
                        }
                        else
                        {
                            settings.Ok = false;
                            settings.MsgError = "No hay ningun anime con este tag con al menos 70%";
                        }
                    }
                    if (!settings.Ok)
                    {
                        var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                        await Task.Delay(3000);
                        await funciones.BorrarMensaje(context, error.Id);
                    }
                    break;
                case "estudio":
                    DiscordEmbed embebido5 = new DiscordEmbedBuilder
                    {
                        Title = "Adivina el estudio del anime",
                        Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                        Color = funciones.GetColor()
                    }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embebido5));
                    var animeList3 = await funcionesJuegos.GetMedia(context, "ANIME", settings, false, true, false, false);
                    await funcionesJuegos.Jugar(context, "estudio", animeList3, settings, interactivity);
                    break;
                case "protagonista":
                    DiscordEmbed embebido6 = new DiscordEmbedBuilder
                    {
                        Title = "Adivina el protagonista del anime",
                        Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                        Color = funciones.GetColor()
                    }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embebido6));
                    var animeList4 = await funcionesJuegos.GetMedia(context, "ANIME", settings, true, false, false, false);
                    await funcionesJuegos.Jugar(context, "protagonista", animeList4, settings, interactivity);
                    break;
                case "genero":
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                    SettingsJuego settingsGenero = await funcionesJuegos.InicializarJuego(context, interactivity, false, true);
                    settings.Ok = settingsGenero.Ok;
                    if (settings.Ok)
                    {
                        settings.Genero = settingsGenero.Genero;
                        settings.Dificultad = settingsGenero.Dificultad;
                        DiscordEmbed embebido7 = new DiscordEmbedBuilder
                        {
                            Title = $"Adivina el género",
                            Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                            Color = funciones.GetColor()
                        }.AddField("Rondas", $"{settings.Rondas}").AddField("Género", $"{settings.Dificultad}");
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embebido7));
                        var animeList5 = await funcionesJuegos.GetMedia(context, "ANIME", settings, false, false, false, true);
                        await funcionesJuegos.Jugar(context, "genero", animeList5, settings, interactivity);
                    }
                    else
                    {
                        var error = await ctx.Channel.SendMessageAsync(settings.MsgError).ConfigureAwait(false);
                        await Task.Delay(5000);
                        await funciones.BorrarMensaje(context, error.Id);
                    }
                    break;
            }
        }

        [SlashCommand("ahorcado", "Juega una partida del ahorcado adivinando algo relacionado al anime")]
        public async Task Ahorcado(InteractionContext ctx,
        [Choice("Personajes", "personaje")]
        [Choice("Animes", "anime")]
        [Option("Juego", "El tipo de ahorcado")] string juego)
        {
            var context = funciones.GetContext(ctx);
            int pag;
            switch (juego)
            {
                case "personaje":
                    pag = funciones.GetNumeroRandom(1, 5000);
                    Character personaje = await funciones.GetRandomCharacter(context, pag);
                    if (personaje != null)
                    {
                        await funcionesJuegos.JugarAhorcado(ctx, personaje, "personaje");
                    }
                    break;
                case "anime":
                    pag = funciones.GetNumeroRandom(1, 5000);
                    Anime anime = await funciones.GetRandomMedia(context, pag, "anime");
                    if (anime != null)
                    {
                        await funcionesJuegos.JugarAhorcado(ctx, anime, "anime");
                    }
                    break;
            }
        }

        [SlashCommand("stats", "Busca las estadisticas de un quiz")]
        public async Task Stats(InteractionContext ctx,
        [Choice("Adivina el personaje", "personajes")]
        [Choice("Adivina el anime", "animes")]
        [Choice("Adivina el manga", "mangas")]
        [Choice("Adivina el tag", "tag")]
        [Choice("Adivina el estudio", "estudio")]
        [Choice("Adivina el protagonista", "protagonista")]
        [Choice("Adivina el genero", "genero")]
        [Option("Juego", "El tipo de quiz que quieres ver las estadisticas")] string juego)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = funciones.GetContext(ctx);
            var interactivity = ctx.Client.GetInteractivity();
            DiscordEmbedBuilder builder;
            switch (juego)
            {
                case "personajes":
                    builder = await funcionesJuegos.GetEstadisticas(context, "personaje");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                    await funciones.ChequearVotoTopGG(context);
                    break;
                case "animes":
                    builder = await funcionesJuegos.GetEstadisticas(context, "anime");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                    await funciones.ChequearVotoTopGG(context);
                    break;
                case "mangas":
                    builder = await funcionesJuegos.GetEstadisticas(context, "manga");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                    await funciones.ChequearVotoTopGG(context);
                    break;
                case "tag":
                    await ctx.DeleteResponseAsync();
                    builder = await funcionesJuegos.GetEstadisticasTag(context);
                    var msg = await ctx.Channel.SendMessageAsync(embed: builder);
                    await funciones.ChequearVotoTopGG(context);
                    if (builder.Title == "Error")
                    {
                        await Task.Delay(5000);
                        await funciones.BorrarMensaje(context, msg.Id);
                    }
                    break;
                case "estudio":
                    builder = await funcionesJuegos.GetEstadisticas(context, "estudio");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                    await funciones.ChequearVotoTopGG(context);
                    break;
                case "protagonista":
                    builder = await funcionesJuegos.GetEstadisticas(context, "protagonista");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                    await funciones.ChequearVotoTopGG(context);
                    break;
                case "genero":
                    var respuesta = await funcionesJuegos.ElegirGenero(context, interactivity);
                    if (respuesta.Ok)
                    {
                        builder = await funcionesJuegos.GetEstadisticasGenero(context, respuesta.Genero);
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                        await funciones.ChequearVotoTopGG(context);
                    }
                    break;
            }
        }
    }
}
