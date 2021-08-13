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
        [Choice("Characters", "characters")]
        [Choice("Animes", "animes")]
        [Choice("Mangas", "mangas")]
        [Choice("Tags", "tag")]
        [Choice("Studios", "studio")]
        [Choice("Protagonists", "protagonist")]
        [Choice("Genres", "genre")]
        [Option("Game", "The kind of quiz you want to play")] string juego,
        [Choice("Easy", "Fácil")]
        [Choice("Medium", "Media")]
        [Choice("Hard", "Dificil")]
        [Choice("Extreme", "Extremo")]
        [Option("Difficulty", "The difficulty of the quiz")] string dificultad,
        [Option("Rounds", "The number of rounds the quiz will take")] long rondas)
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
                case "characters":
                    embebido = new DiscordEmbedBuilder
                    {
                        Title = "Guess the character",
                        Description = $"{ctx.User.Mention}, you can type `cancel` in any of the rounds if you want to end the game.",
                        Color = funciones.GetColor()
                    }.AddField("Rounds", $"{settings.Rondas}").AddField("Difficulty", $"{settings.Dificultad}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embebido));
                    list = await funcionesJuegos.GetCharacters(context, settings, false);
                    await funcionesJuegos.JugarQuiz(context, "personaje", list, settings, interactivity);
                    break;
                case "animes":
                    embebido = new DiscordEmbedBuilder
                    {
                        Title = "Guess the anime",
                        Description = $"{ctx.User.Mention}, you can type `cancel` in any of the rounds if you want to end the game.",
                        Color = funciones.GetColor()
                    }.AddField("Rounds", $"{settings.Rondas}").AddField("Difficulty", $"{settings.Dificultad}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embebido));
                    list = await funcionesJuegos.GetCharacters(context, settings, true);
                    await funcionesJuegos.JugarQuiz(context, "anime", list, settings, interactivity);
                    break;
                case "mangas":
                    embebido = new DiscordEmbedBuilder
                    {
                        Title = "Guess the manga",
                        Description = $"{ctx.User.Mention}, you can type `cancel` in any of the rounds if you want to end the game.",
                        Color = funciones.GetColor()
                    }.AddField("Rounds", $"{settings.Rondas}").AddField("Difficulty", $"{settings.Dificultad}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embebido));
                    list = await funcionesJuegos.GetMedia(context, "MANGA", settings, false, false, false, false);
                    await funcionesJuegos.JugarQuiz(context, "manga", list, settings, interactivity);
                    break;
                case "tag":
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder {
                        Title = "Set up the quiz",
                        Description = "You must choose the tag to continue",
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
                            Title = $"Guess the tag",
                            Description = $"{ctx.User.Mention}, you can type `cancel` in any of the rounds if you want to end the game.",
                            Color = funciones.GetColor(),
                            Footer = funciones.GetFooter(ctx)
                        }.AddField("Rounds", $"{settings.Rondas}").AddField("Tag", $"{settings.Tag}");
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
                                    Title = $"Reduced rounds",
                                    Description = $"The number of rounds has been reduced to {settings.Rondas} since this is the amount of anime with at least {settings.PorcentajeTag}% of {settings.Tag}",
                                }));
                            }
                            settings.Dificultad = settings.Tag;
                            await funcionesJuegos.JugarQuiz(context, "tag", list, settings, interactivity);
                        }
                        else
                        {
                            settings.Ok = false;
                            settings.MsgError = $"There is no anime with at least 70 of {settings.Tag}%";
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
                case "studio":
                    embebido = new DiscordEmbedBuilder
                    {
                        Title = "Guess the studio",
                        Description = $"{ctx.User.Mention}, you can type `cancel` in any of the rounds if you want to end the game.",
                        Color = funciones.GetColor()
                    }.AddField("Rounds", $"{settings.Rondas}").AddField("Difficulty", $"{settings.Dificultad}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embebido));
                    list = await funcionesJuegos.GetMedia(context, "ANIME", settings, false, true, false, false);
                    await funcionesJuegos.JugarQuiz(context, "estudio", list, settings, interactivity);
                    break;
                case "protagonist":
                    embebido = new DiscordEmbedBuilder
                    {
                        Title = "Guess the protagonist",
                        Description = $"{ctx.User.Mention}, you can type `cancel` in any of the rounds if you want to end the game.",
                        Color = funciones.GetColor()
                    }.AddField("Rounds", $"{settings.Rondas}").AddField("Difficulty", $"{settings.Dificultad}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embebido));
                    list = await funcionesJuegos.GetMedia(context, "ANIME", settings, true, false, false, false);
                    await funcionesJuegos.JugarQuiz(context, "protagonista", list, settings, interactivity);
                    break;
                case "genre":
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                    SettingsJuego settingsGenero = await funcionesJuegos.InicializarJuego(context, interactivity, false, true);
                    settings.Ok = settingsGenero.Ok;
                    if (settings.Ok)
                    {
                        settings.Genero = settingsGenero.Genero;
                        settings.Dificultad = settingsGenero.Dificultad;
                        embebido = new DiscordEmbedBuilder
                        {
                            Title = $"Guess the genre",
                            Description = $"{ctx.User.Mention}, you can type `cancel` in any of the rounds if you want to end the game.",
                            Color = funciones.GetColor()
                        }.AddField("Rounds", $"{settings.Rondas}").AddField("Genre", $"{settings.Dificultad}");
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

        [SlashCommand("hangman", "Play a game of hangman by guessing something related to anime")]
        public async Task Ahorcado(InteractionContext ctx,
        [Choice("Characters", "character")]
        [Choice("Animes", "anime")]
        [Option("Game", "The gamemode")] string juego)
        {
            var context = funciones.GetContext(ctx);
            int pag;
            switch (juego)
            {
                case "character":
                    pag = funciones.GetNumeroRandom(1, 5000);
                    Character personaje = await funciones.GetRandomCharacter(context, pag);
                    if (personaje != null)
                    {
                        await funcionesJuegos.JugarAhorcado(ctx, personaje, "character");
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

        [SlashCommand("higherorlower", "Play a game of higher or lower")]
        public async Task HighrOrLower(InteractionContext ctx,
        [Choice("Singleplayer", "Singleplayer")]
        [Choice("Multiplayer", "Multiplayer")]
        [Option("Gamemode", "Choose if you want to play alone or with your friends")] string gamemode)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder { 
                Title = "Higher or Lower",
                Description = $"**Gamemode:** {gamemode}\n\n" +
                $"Choose the anime with the best avarage score.",
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

                var msgElegir = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed1).AddEmbed(embed2).AddComponents(button1, button2).WithContent("Which has the highest score?"));
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
                            Title = $"{acertador.DisplayName} has guessed it!",
                            Description = $"**{seleccionado.TitleRomaji}** hava an avarage score of **{puntajeSel}/10** while **{otro.TitleRomaji}** has **{puntajeOtro}/10**\n\nScore: **{puntuacion}**",
                            Color = DiscordColor.Green
                        }.WithFooter($"Game started by {ctx.User.Username}#{ctx.User.Discriminator}", ctx.User.AvatarUrl)));
                    }
                    else
                    {
                        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                        {
                            Title = $"Defeat!",
                            Description = $"**{acertador.DisplayName} has failed!**\n\n**{seleccionado.TitleRomaji}** has lower avarage score **({puntajeSel}/10)** than **{otro.TitleRomaji} ({puntajeOtro}/10)**\n\nScore: **{puntuacion}**",
                            Color = DiscordColor.Red
                        }.WithFooter($"Game started by {ctx.User.Username}#{ctx.User.Discriminator}", ctx.User.AvatarUrl)));
                        jugar = false;
                    }
                }
                else
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder {
                        Title = "Defeat!",
                        Description = $"{timeout} seconds have passed and it was not answered in time",
                        Color = DiscordColor.Red
                    }.WithFooter($"Game started by {ctx.User.Username}#{ctx.User.Discriminator}", ctx.User.AvatarUrl)));
                    jugar = false;
                }

                lista.Remove(elegido1);
                lista.Remove(elegido2);
            }
        }

        [SlashCommand("stats", "Search the statistics of a quiz ")]
        public async Task Stats(InteractionContext ctx,
        [Choice("Characters", "characters")]
        [Choice("Animes", "animes")]
        [Choice("Mangas", "mangas")]
        [Choice("Tags", "tag")]
        [Choice("Studios", "studio")]
        [Choice("Protagonists", "protagonist")]
        [Choice("Genres", "genre")]
        [Option("Game", "The kind of quiz you want to play")] string juego)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = funciones.GetContext(ctx);
            var interactivity = ctx.Client.GetInteractivity();
            DiscordEmbedBuilder builder;
            switch (juego)
            {
                case "characters":
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
                case "studio":
                    builder = await funcionesJuegos.GetEstadisticas(context, "estudio");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                    await funciones.ChequearVotoTopGG(context);
                    break;
                case "protagonist":
                    builder = await funcionesJuegos.GetEstadisticas(context, "protagonista");
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                    await funciones.ChequearVotoTopGG(context);
                    break;
                case "genre":
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

        [SlashCommand("deletestats", "Delete your statistics from all games on the guild")]
        public async Task DeleteStats(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = funciones.GetContext(ctx);
            var interactivity = ctx.Client.GetInteractivity();

            string titulo = "Confirm if you want to delete your statistics from the guild";
            string opciones = $"**Please note that statistics deletion cannot be undone**";
            bool confirmar = await funciones.GetSiNoInteractivity(context, interactivity, titulo, opciones);
            if (confirmar)
            {
                await funcionesJuegos.EliminarEstadisticas(context);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Your stats have been cleared successfully"));
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"The process of clearing your statistics has been canceled"));
            }
        }
    }
}
