using Discord_Bot;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YumikoBot
{
    public class JuegosSlashCommands : ApplicationCommandModule
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
            DiscordEmbed embebido;
            dynamic list;
            switch (juego)
            {
                case "personajes":
                    embebido = new DiscordEmbedBuilder
                    {
                        Title = "Adivina el personaje",
                        Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                        Color = funciones.GetColor()
                    }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embebido));
                    list = await funcionesJuegos.GetCharacters(context, settings, false);
                    await funcionesJuegos.JugarQuiz(context, "personaje", list, settings, interactivity);
                    break;
                case "animes":
                    embebido = new DiscordEmbedBuilder
                    {
                        Title = "Adivina el anime",
                        Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                        Color = funciones.GetColor()
                    }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embebido));
                    list = await funcionesJuegos.GetCharacters(context, settings, true);
                    await funcionesJuegos.JugarQuiz(context, "anime", list, settings, interactivity);
                    break;
                case "mangas":
                    embebido = new DiscordEmbedBuilder
                    {
                        Title = "Adivina el manga",
                        Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                        Color = funciones.GetColor()
                    }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embebido));
                    list = await funcionesJuegos.GetMedia(context, "MANGA", settings, false, false, false, false);
                    await funcionesJuegos.JugarQuiz(context, "manga", list, settings, interactivity);
                    break;
                case "tag":
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder {
                        Title = "Configura la partida",
                        Description = "Deberás elegir el tag para continuar",
                        Color = funciones.GetColor(),
                    }));
                    SettingsJuego settingsTag = await funcionesJuegos.InicializarJuego(context, interactivity, true, false);
                    settings.Ok = settingsTag.Ok;
                    if (settings.Ok)
                    {
                        settings.Tag = settingsTag.Tag;
                        settings.TagDesc = settingsTag.TagDesc;
                        embebido = new DiscordEmbedBuilder
                        {
                            Title = $"Adivina el tag",
                            Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                            Color = funciones.GetColor(),
                            Footer = funciones.GetFooter(ctx)
                        }.AddField("Rondas", $"{settings.Rondas}").AddField("Tag", $"{settings.Tag}");
                        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embebido));
                        settings.PorcentajeTag = 70;
                        list = await funcionesJuegos.GetMedia(context, "ANIME", settings, false, false, true, false);
                        int cantidadAnimes = list.Count;
                        if (cantidadAnimes > 0)
                        {
                            if (cantidadAnimes < settings.Rondas)
                            {
                                settings.Rondas = cantidadAnimes;
                                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                                {
                                    Color = DiscordColor.Yellow,
                                    Title = $"Rondas reducidas",
                                    Description = $"Se han reducido el numero de rondas a {settings.Rondas} ya que esta es la cantidad de animes con al menos un {settings.PorcentajeTag}% de {settings.Tag}",
                                }));
                            }
                            settings.Dificultad = settings.Tag;
                            await funcionesJuegos.JugarQuiz(context, "tag", list, settings, interactivity);
                        }
                        else
                        {
                            settings.Ok = false;
                            settings.MsgError = "No hay ningun anime con este tag con al menos 70%";
                        }
                    }
                    if (!settings.Ok)
                    {
                        var error = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder { 
                            Title = "Error",
                            Description = settings.MsgError,
                            Color = DiscordColor.Red
                        }));
                        await Task.Delay(3000);
                        await funciones.BorrarMensaje(context, error.Id);
                    }
                    break;
                case "estudio":
                    embebido = new DiscordEmbedBuilder
                    {
                        Title = "Adivina el estudio del anime",
                        Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                        Color = funciones.GetColor()
                    }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embebido));
                    list = await funcionesJuegos.GetMedia(context, "ANIME", settings, false, true, false, false);
                    await funcionesJuegos.JugarQuiz(context, "estudio", list, settings, interactivity);
                    break;
                case "protagonista":
                    embebido = new DiscordEmbedBuilder
                    {
                        Title = "Adivina el protagonista del anime",
                        Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                        Color = funciones.GetColor()
                    }.AddField("Rondas", $"{settings.Rondas}").AddField("Dificultad", $"{settings.Dificultad}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embebido));
                    list = await funcionesJuegos.GetMedia(context, "ANIME", settings, true, false, false, false);
                    await funcionesJuegos.JugarQuiz(context, "protagonista", list, settings, interactivity);
                    break;
                case "genero":
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                    SettingsJuego settingsGenero = await funcionesJuegos.InicializarJuego(context, interactivity, false, true);
                    settings.Ok = settingsGenero.Ok;
                    if (settings.Ok)
                    {
                        settings.Genero = settingsGenero.Genero;
                        settings.Dificultad = settingsGenero.Dificultad;
                        embebido = new DiscordEmbedBuilder
                        {
                            Title = $"Adivina el género",
                            Description = $"{ctx.User.Mention}, puedes escribir `cancelar` en cualquiera de las rondas si deseas terminar la partida.",
                            Color = funciones.GetColor()
                        }.AddField("Rondas", $"{settings.Rondas}").AddField("Género", $"{settings.Dificultad}");
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embebido));
                        list = await funcionesJuegos.GetMedia(context, "ANIME", settings, false, false, false, true);
                        await funcionesJuegos.JugarQuiz(context, "genero", list, settings, interactivity);
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

        [SlashCommand("higherorlower", "Juega una partida de higher or lower")]
        public async Task HighrOrLower(InteractionContext ctx,
        [Choice("Singleplayer", "Singleplayer")]
        [Choice("Multiplayer", "Multiplayer")]
        [Option("Gamemode", "Elige si quieres jugar en solitario o en compañía")] string gamemode)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder { 
                Title = "Higher or Lower",
                Description = $"**Modo de juego:** {gamemode}\n\n" +
                $"Elige el anime con mejor score para obtener la mejor puntuación.",
                Color = funciones.GetColor(),
            }));

            var context = funciones.GetContext(ctx);
            var interactivity = ctx.Client.GetInteractivity();

            int valorBase = funciones.GetNumeroRandom(1, 36); // Del 1-200 hasta 1800-2000
            var settings = new SettingsJuego
            {
                IterIni = valorBase,
                IterFin = valorBase + 4, // 200 animes seleccionados
            };
            var listaAux = await funcionesJuegos.GetMedia(context, "ANIME", settings, false, false, false, false);
            List<Anime> lista = new();
            foreach(var item in listaAux)
            {
                if(item.AvarageScore > -1)
                {
                    lista.Add(item);
                }
            }

            bool jugar = true;
            int puntuacion = 0;
            while (jugar && lista.Count >= 2)
            {
                int random1 = funciones.GetNumeroRandom(0, lista.Count - 1);
                int random2;
                do
                {
                    random2 = funciones.GetNumeroRandom(0, lista.Count - 1);
                } while (random1 == random2);

                var elegido1 = lista[random1];
                var elegido2 = lista[random2];

                var embed1 = new DiscordEmbedBuilder
                {
                    Description = $"**{elegido1.TitleRomaji}**",
                    ImageUrl = elegido1.Image,
                    Color = DiscordColor.Green
                };
                var embed2 = new DiscordEmbedBuilder
                {
                    Description = $"**{elegido2.TitleRomaji}**",
                    ImageUrl = elegido2.Image,
                    Color = DiscordColor.Red
                };

                DiscordButtonComponent button1 = new(ButtonStyle.Success, $"{elegido1.Id}", $"{funciones.NormalizarBoton(elegido1.TitleRomaji)}");
                DiscordButtonComponent button2 = new(ButtonStyle.Danger, $"{elegido2.Id}", $"{funciones.NormalizarBoton(elegido2.TitleRomaji)}");

                var msgElegir = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed1).AddEmbed(embed2).AddComponents(button1, button2).WithContent("¿Cuál tiene mejor puntuación?"));
                double timeout = 15;

                dynamic msgElegirInter;

                if(gamemode == "Singleplayer")
                {
                    msgElegirInter = await interactivity.WaitForButtonAsync(msgElegir, ctx.User, TimeSpan.FromSeconds(timeout));
                }
                else
                {
                    msgElegirInter = await interactivity.WaitForButtonAsync(msgElegir, TimeSpan.FromSeconds(timeout));
                }
                if (!msgElegirInter.TimedOut)
                {
                    DiscordMember acertador = (DiscordMember)msgElegirInter.Result.User;
                    Anime seleccionado, otro;
                    string idElegido = msgElegirInter.Result.Id;
                    if(elegido1.Id == int.Parse(idElegido))
                    {
                        seleccionado = elegido1;
                        otro = elegido2;
                    }
                    else{
                        seleccionado = elegido2;
                        otro = elegido1;
                    }
                    double puntajeSel, puntajeOtro;
                    puntajeSel = (double)seleccionado.AvarageScore / 10;
                    puntajeOtro = (double)otro.AvarageScore / 10;

                    if (seleccionado.AvarageScore >= otro.AvarageScore)
                    {
                        puntuacion++;
                        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                        {
                            Title = $"¡{acertador.DisplayName} le ha acertado!",
                            Description = $"**{seleccionado.TitleRomaji}** tiene **{puntajeSel}/10** de puntuación promedio mientras **{otro.TitleRomaji}** tiene **{puntajeOtro}/10**\n\nPuntuación: **{puntuacion}**",
                            Color = DiscordColor.Green
                        }.WithFooter($"Partida iniciada por {ctx.User.Username}#{ctx.User.Discriminator}", ctx.User.AvatarUrl)));
                    }
                    else
                    {
                        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                        {
                            Title = $"¡{acertador.DisplayName} le ha errado!",
                            Description = $"**¡Derrota!**\n\n**{seleccionado.TitleRomaji}** tiene menor puntuación promedio **({puntajeSel}/10)** que **{otro.TitleRomaji} ({puntajeOtro}/10)**\n\nPuntuación: **{puntuacion}**",
                            Color = DiscordColor.Red
                        }.WithFooter($"Partida iniciada por {ctx.User.Username}#{ctx.User.Discriminator}", ctx.User.AvatarUrl)));
                        jugar = false;
                    }
                }
                else
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder {
                        Title = "Derrota",
                        Description = $"Han pasado {timeout} segundos y no se ha contestado a tiempo",
                        Color = DiscordColor.Red
                    }.WithFooter($"Partida iniciada por {ctx.User.Username}#{ctx.User.Discriminator}", ctx.User.AvatarUrl)));
                    jugar = false;
                }

                lista.Remove(elegido1);
                lista.Remove(elegido2);
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

        [SlashCommand("deletestats", "Elimina tus estadisticas de todos los juegos en el servidor")]
        public async Task DeleteStats(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = funciones.GetContext(ctx);
            var interactivity = ctx.Client.GetInteractivity();

            string titulo = "Confirma si quieres eliminar tus estadisticas del servidor";
            string opciones = $"**Ten en cuenta que el borrado de estadisticas no se puede deshacer**";
            bool confirmar = await funciones.GetSiNoInteractivity(context, interactivity, titulo, opciones);
            if (confirmar)
            {
                await funcionesJuegos.EliminarEstadisticas(context);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Se han borrado tus estadisticas correctamente"));
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Se ha cancelado el proceso de borrar tus estadísticas"));
            }
        }
    }
}
