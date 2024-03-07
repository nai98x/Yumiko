namespace Yumiko.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Yumiko.Datatypes;

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Not with D#+ Command classes")]
    public class Games : ApplicationCommandModule
    {
        public IConfigurationRoot Configuration { private get; set; } = null!;

        public override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
        {
            return Common.BeforeSlashExecutionAsync(ctx);
        }

        public override Task<bool> BeforeContextMenuExecutionAsync(ContextMenuContext ctx)
        {
            return Common.BeforeContextMenuExecutionAsync(ctx);
        }

        [SlashCommand("trivia", "Plays an anime trivia game")]
        [DescriptionLocalization(Localization.Spanish, "Juega una partida de trivia de anime")]
        public async Task Trivia(
        InteractionContext ctx,
        [Option("Gamemode", "The type of game you want to play")] Gamemode gamemode,
        [Option("Difficulty", "Choose the difficulty of the trivia")] Difficulty difficulty,
        [Option("Rounds", "Rounds to play (minimum is 1 y and maximum is 30)")][Minimum(1)][Maximum(30)] long rounds)
        {
            if (Singleton.GetInstance().GetCurrentTrivia(ctx.Guild.Id, ctx.Channel.Id) != null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .AsEphemeral(true)
                    .AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = translations.another_trivia_playing,
                        Description = translations.another_trivia_playing_desc,
                        Color = DiscordColor.Red,
                    }));
                return;
            }

            int iterIni, iterFin;
            switch (difficulty)
            {
                case Difficulty.Easy:
                    iterIni = 1;
                    iterFin = 10;
                    break;
                case Difficulty.Normal:
                    iterIni = 10;
                    iterFin = 30;
                    break;
                case Difficulty.Hard:
                    iterIni = 30;
                    iterFin = 60;
                    break;
                case Difficulty.Extreme:
                    iterIni = 60;
                    iterFin = 100;
                    break;
                default:
                    iterIni = 10;
                    iterFin = 30;
                    break;
            }

            var settings = new GameSettings
            {
                Rondas = (int)rounds,
                Difficulty = difficulty,
                IterIni = iterIni,
                IterFin = iterFin,
                Gamemode = gamemode,
                Ok = true,
            };

            double timeoutGames = ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGames);
            string topggToken = ConfigurationUtils.GetConfiguration<string>(Configuration, Configurations.TokenTopgg);

            var interactivity = ctx.Client.GetInteractivity();
            DiscordEmbed embed;
            dynamic list;
            switch (gamemode)
            {
                case Gamemode.Characters:
                    embed = new DiscordEmbedBuilder
                    {
                        Title = translations.guess_the_character,
                        Color = Constants.YumikoColor,
                    }.AddField(translations.rounds, $"{settings.Rondas}").AddField(translations.difficulty, $"{settings.Difficulty}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                    list = await GameServices.GetCharactersAsync(ctx, settings);
                    await GameServices.PlayTriviaAsync(ctx, timeoutGames, topggToken, list, settings);
                    break;
                case Gamemode.Animes:
                    embed = new DiscordEmbedBuilder
                    {
                        Title = translations.guess_the_anime,
                        Color = Constants.YumikoColor,
                    }.AddField(translations.rounds, $"{settings.Rondas}").AddField(translations.difficulty, $"{settings.Difficulty}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                    list = await GameServices.GetMediaAsync(ctx, MediaType.ANIME, settings, false, false, false, true, true);
                    await GameServices.PlayTriviaAsync(ctx, timeoutGames, topggToken, list, settings);
                    break;
                case Gamemode.Mangas:
                    embed = new DiscordEmbedBuilder
                    {
                        Title = translations.guess_the_manga,
                        Color = Constants.YumikoColor,
                    }.AddField(translations.rounds, $"{settings.Rondas}").AddField(translations.difficulty, $"{settings.Difficulty}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                    list = await GameServices.GetMediaAsync(ctx, MediaType.MANGA, settings, false, false, false, false, true);
                    await GameServices.PlayTriviaAsync(ctx, timeoutGames, topggToken, list, settings);

                    break;
                case Gamemode.Studios:
                    embed = new DiscordEmbedBuilder
                    {
                        Title = translations.guess_the_studio,
                        Color = Constants.YumikoColor,
                    }.AddField(translations.rounds, $"{settings.Rondas}").AddField(translations.difficulty, $"{settings.Difficulty}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                    list = await GameServices.GetMediaAsync(ctx, MediaType.ANIME, settings, false, true, false, false, true);
                    await GameServices.PlayTriviaAsync(ctx, timeoutGames, topggToken, list, settings);
                    break;
                case Gamemode.Protagonists:
                    embed = new DiscordEmbedBuilder
                    {
                        Title = translations.guess_the_protagonist,
                        Color = Constants.YumikoColor,
                    }.AddField(translations.rounds, $"{settings.Rondas}").AddField(translations.difficulty, $"{settings.Difficulty}");
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                    list = await GameServices.GetMediaAsync(ctx, MediaType.ANIME, settings, true, false, false, false, true);
                    await GameServices.PlayTriviaAsync(ctx, timeoutGames, topggToken, list, settings);
                    break;
                case Gamemode.Genres:
                    await ctx.DeferAsync();
                    GameSettings settingsGenero = await GameServices.ElegirGeneroAsync(ctx, ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGeneral), interactivity);
                    settings.Ok = settingsGenero.Ok;
                    if (settings.Ok)
                    {
                        settings.Genre = settingsGenero.Genre;
                        settings.Difficulty = settingsGenero.Difficulty;
                        embed = new DiscordEmbedBuilder
                        {
                            Title = translations.guess_the_genre,
                            Color = Constants.YumikoColor,
                        }.AddField(translations.rounds, $"{settings.Rondas}").AddField(translations.genre, $"{settings.Genre}");
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        list = await GameServices.GetMediaAsync(ctx, MediaType.ANIME, settings, false, false, true, true, true);
                        await GameServices.PlayTriviaAsync(ctx, timeoutGames, topggToken, list, settings);
                    }
                    else
                    {
                        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(settingsGenero.MsgError));
                    }

                    break;
            }
        }

        [SlashCommand("hangman", "Plays the hangman game")]
        [NameLocalization(Localization.Spanish, "ahorcado")]
        [DescriptionLocalization(Localization.Spanish, "Juega al juego del ahorcado")]
        public async Task HangmanAsync(
        InteractionContext ctx,
        [Option("Game", "If the game is about anime characters or anime titles")] HangmanGamemode gamemode)
        {
            await ctx.DeferAsync();
            int pag;
            switch (gamemode)
            {
                case HangmanGamemode.Characters:
                    pag = Common.GetRandomNumber(1, 10000);
                    CharacterOld? personaje = await Common.GetRandomCharacterAsync(ctx, pag);
                    if (personaje != null)
                    {
                        await GameServices.JugarAhorcadoAsync(ctx, personaje, gamemode);
                    }

                    break;
                case HangmanGamemode.Animes:
                    pag = Common.GetRandomNumber(1, 10000);
                    Anime? anime = await Common.GetRandomMediaAsync(ctx, pag, MediaType.ANIME);
                    if (anime != null)
                    {
                        await GameServices.JugarAhorcadoAsync(ctx, anime, gamemode);
                    }

                    break;
            }
        }

        [SlashCommand("higherorlower", "Plays a Higher or Lower game")]
        [DescriptionLocalization(Localization.Spanish, "Juega al juego de Higher or Lower")]
        public async Task HighrOrLower(InteractionContext ctx, [Option("Gamemode", "Higher or Lower gamemode")] GamemodeHoL gamemode)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = "Higher or Lower",
                Description = gamemode == GamemodeHoL.Score ? translations.higher_or_lower_desc : translations.higher_or_lower_desc_popularity,
                Color = Constants.YumikoColor,
            }));

            var interactivity = ctx.Client.GetInteractivity();
            DiscordEmbed? embedAux = null;
            List<Anime> lista = Singleton.GetInstance().GetCachedMedia();

            bool jugar = true;
            int puntuacion = 0;
            while (jugar && lista.Count >= 2)
            {
                int random1 = Common.GetRandomNumber(0, lista.Count - 1);
                int random2;
                do
                {
                    random2 = Common.GetRandomNumber(0, lista.Count - 1);
                }
                while (random1 == random2);

                var elegido1 = lista[random1];
                var elegido2 = lista[random2];

                DiscordButtonComponent button1 = new(ButtonStyle.Primary, $"{elegido1.Id}", $"{elegido1.TitleRomaji.NormalizeButton()}");
                DiscordButtonComponent button2 = new(ButtonStyle.Primary, $"{elegido2.Id}", $"{elegido2.TitleRomaji.NormalizeButton()}");
                DiscordButtonComponent buttonc = new(ButtonStyle.Danger, $"hol-cancel", translations.finish_game);

                var msgBuilder = new DiscordFollowupMessageBuilder().AddComponents(button1, button2, buttonc);

                if (elegido1.Image != null && elegido2.Image != null)
                {
                    var img = await Common.MergeImageAsync(elegido1.Image, elegido2.Image, 500, 375);
                    var imagen = Common.OverlapImage(img, File.ReadAllBytes(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "res", "Images", "frame-hol.png")), 500, 375);
                    msgBuilder.AddFile("image.png", imagen.ToMemoryStream());
                }

                if (embedAux != null)
                {
                    msgBuilder.AddEmbed(embedAux);
                }

                msgBuilder.AddEmbed(new DiscordEmbedBuilder
                {
                    Title = gamemode == GamemodeHoL.Score ? translations.which_one_has_better_score : translations.which_one_is_more_popular,
                    Color = Constants.YumikoColor,
                    ImageUrl = "attachment://image.png",
                });

                var msgElegir = await ctx.FollowUpAsync(msgBuilder);

                double timeout = 15;
                var msgElegirInter = await interactivity.WaitForButtonAsync(msgElegir, ctx.User, TimeSpan.FromSeconds(timeout));

                if (!msgElegirInter.TimedOut)
                {
                    if (msgElegirInter.Result.Id == "hol-cancel")
                    {
                        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder()
                        {
                            Title = translations.game_cancelled,
                            Color = DiscordColor.Red
                        }));
                        jugar = false;
                    }
                    else
                    {
                        Anime seleccionado, otro;
                        string idElegido = msgElegirInter.Result.Id;
                        if (elegido1.Id == int.Parse(idElegido))
                        {
                            seleccionado = elegido1;
                            otro = elegido2;
                        }
                        else
                        {
                            seleccionado = elegido2;
                            otro = elegido1;
                        }

                        double puntajeSel, puntajeOtro;
                        puntajeSel = (double)seleccionado.AvarageScore / 10;
                        puntajeOtro = (double)otro.AvarageScore / 10;
                        var tiempo = msgElegirInter.Result.Interaction.CreationTimestamp - msgElegir.CreationTimestamp;

                        if ((gamemode == GamemodeHoL.Score && seleccionado.AvarageScore >= otro.AvarageScore) || (gamemode == GamemodeHoL.Popularity && seleccionado.Favoritos >= otro.Favoritos))
                        {
                            puntuacion++;
                            embedAux = new DiscordEmbedBuilder
                            {
                                Title = string.Format(translations.guess_user, ctx.Member.DisplayName),
                                Description = gamemode == GamemodeHoL.Score ?
                                    $"{string.Format(translations.higher_or_lower_round_win, seleccionado.TitleRomaji, puntajeSel, otro.TitleRomaji, puntajeOtro)}\n\n{translations.score}: **{puntuacion}**" :
                                    $"{string.Format(translations.higher_or_lower_round_win_popularity, seleccionado.TitleRomaji, seleccionado.Favoritos, otro.TitleRomaji, otro.Favoritos)}\n\n{translations.score}: **{puntuacion}**",
                                Color = DiscordColor.Green,
                                Footer = new()
                                {
                                    IconUrl = ctx.Member.AvatarUrl,
                                    Text = $"{translations.time}: {tiempo.TotalSeconds}s",
                                },
                            };
                        }
                        else
                        {
                            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                            {
                                Title = string.Format(translations.miss_user_games, ctx.Member.DisplayName),
                                Description = gamemode == GamemodeHoL.Score ?
                                    $"**{translations.defeat}**\n\n{string.Format(translations.higher_or_lower_round_defeat, seleccionado.TitleRomaji, puntajeSel, otro.TitleRomaji, puntajeOtro)}\n\n{translations.score}: **{puntuacion}**" :
                                    $"**{translations.defeat}**\n\n{string.Format(translations.higher_or_lower_round_defeat_popularity, seleccionado.TitleRomaji, seleccionado.Favoritos, otro.TitleRomaji, otro.Favoritos)}\n\n{translations.score}: **{puntuacion}**",
                                Color = DiscordColor.Red,
                                Footer = new()
                                {
                                    IconUrl = ctx.Member.AvatarUrl,
                                    Text = $"{translations.time}: {tiempo.TotalSeconds}s",
                                },
                            }));
                            jugar = false;
                        }
                    }
                }
                else
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = translations.defeat,
                        Description = $"{string.Format(translations.no_answser_in_time, timeout)}\n\n{translations.score}: **{puntuacion}**",
                        Color = DiscordColor.Red,
                    }));
                    jugar = false;
                }

                lista.Remove(elegido1);
                lista.Remove(elegido2);

                // Vuelvo a rellenar la lista si se queda sin items
                if (lista.Count < 2 || stopwatch.Elapsed.TotalMinutes >= 14 /* 15 minutes is the limit for a single interaction */)
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = translations.victory,
                        Color = DiscordColor.Green,
                    }));
                    stopwatch.Stop();
                    jugar = false;
                }
            }

            if (puntuacion > 0)
            {
                bool record = await LeaderboardHoL.AddRegistroAsync(ctx.User.Id, ctx.Guild.Id, puntuacion);
                if (record)
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = translations.new_record,
                        Description = $"{string.Format(translations.new_record_desc, ctx.User.Mention)}\n\n{string.Format(translations.your_new_record_is, puntuacion)}",
                        Color = Constants.YumikoColor,
                    }));
                }
            }
        }

        [SlashCommand("tictactoe", "Starts a Tic-Tac-Toe game")]
        [NameLocalization(Localization.Spanish, "tateti")]
        [DescriptionLocalization(Localization.Spanish, "Juega al juego del Ta-Te-Ti")]
        public async Task TicTacToe(InteractionContext ctx, [Option("Player2", "The second player of the game")] DiscordUser player2)
        {
            var interactivity = ctx.Client.GetInteractivity();

            if (player2.IsBot)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).AddEmbed(new DiscordEmbedBuilder
                {
                    Title = translations.error,
                    Description = translations.cant_play_vs_bot,
                    Color = DiscordColor.Red,
                }));
                return;
            }

            await ctx.DeferAsync();

            Random rand = new();
            bool turnoPrimerJugador = Convert.ToBoolean(rand.NextDouble() >= 0.5);
            bool cambiarTurno = true;
            DiscordUser player1 = ctx.User;
            DiscordUser jugadorTurno = turnoPrimerJugador ? player1 : player2;
            DiscordColor colorTurno = turnoPrimerJugador ? DiscordColor.Green : DiscordColor.Red;
            DiscordWebhookBuilder whBuilder = new();

            DiscordButtonComponent botonA = new(ButtonStyle.Secondary, "a", "[ ]");
            DiscordButtonComponent botonB = new(ButtonStyle.Secondary, "b", "[ ]");
            DiscordButtonComponent botonC = new(ButtonStyle.Secondary, "c", "[ ]");
            DiscordButtonComponent botonD = new(ButtonStyle.Secondary, "d", "[ ]");
            DiscordButtonComponent botonE = new(ButtonStyle.Secondary, "e", "[ ]");
            DiscordButtonComponent botonF = new(ButtonStyle.Secondary, "f", "[ ]");
            DiscordButtonComponent botonG = new(ButtonStyle.Secondary, "g", "[ ]");
            DiscordButtonComponent botonH = new(ButtonStyle.Secondary, "h", "[ ]");
            DiscordButtonComponent botonI = new(ButtonStyle.Secondary, "i", "[ ]");

            List<DiscordComponent> row1 = new() { botonA, botonB, botonC };
            List<DiscordComponent> row2 = new() { botonD, botonE, botonF };
            List<DiscordComponent> row3 = new() { botonG, botonH, botonI };

            List<DiscordComponent> botones = new() { botonA, botonB, botonC, botonD, botonE, botonF, botonG, botonH, botonI };

            var embed = new DiscordEmbedBuilder
            {
                Title = translations.tictactoe,
                Color = colorTurno,
                Description = string.Format(translations.player_turn, jugadorTurno.Mention)
            };

            var builder = new DiscordFollowupMessageBuilder().AddEmbed(embed).AddComponents(row1).AddComponents(row2).AddComponents(row3);
            var mensaje = await ctx.FollowUpAsync(builder);

            bool terminada;
            do
            {
                jugadorTurno = turnoPrimerJugador ? player1 : player2;
                colorTurno = turnoPrimerJugador ? DiscordColor.Red : DiscordColor.Green;
                DiscordUser jugadorProxTurno = !turnoPrimerJugador ? player1 : player2;
                var interaction = await interactivity.WaitForButtonAsync(mensaje, jugadorTurno, TimeSpan.FromSeconds(30));

                if (!interaction.TimedOut)
                {
                    var result = interaction.Result;

                    var aux = GameServices.GetBotonesTicTacToe(result.Id, botones, turnoPrimerJugador);
                    botones = aux.Item1;
                    cambiarTurno = aux.Item2;
                    var listaBotones = botones.Chunk(3);

                    whBuilder.Clear();
                    whBuilder.ClearComponents();
                    foreach (var n in listaBotones)
                    {
                        whBuilder.AddComponents(n);
                    }

                    var resultPartidaTerminada = GameServices.PartidaTerminadaTicTacToe(botones, player1, player2);
                    terminada = resultPartidaTerminada.Item1;
                    DiscordUser? ganador = resultPartidaTerminada.Item2;

                    if (!terminada)
                    {
                        embed.Description = string.Format(translations.player_turn, jugadorProxTurno.Mention);
                        embed.Color = colorTurno;
                        whBuilder.AddEmbed(embed);
                        mensaje = await ctx.EditFollowupAsync(mensaje.Id, whBuilder);
                    }
                    else
                    {
                        var botonesAux = new List<DiscordButtonComponent>();
                        foreach (var boton in botones)
                        {
                            var button = (DiscordButtonComponent)boton;
                            var botonAux = button.Disable();
                            botonesAux.Add(botonAux);
                        }

                        listaBotones = botonesAux.Chunk(3);

                        whBuilder.Clear();
                        whBuilder.ClearComponents();
                        foreach (var n in listaBotones)
                        {
                            whBuilder.AddComponents(n);
                        }

                        if (ganador != null)
                        {
                            embed.Title = translations.we_have_a_winner;
                            embed.Description = string.Format(translations.user_won_the_game, ganador.Mention);
                            embed.Color = turnoPrimerJugador ? DiscordColor.Green : DiscordColor.Red;
                            whBuilder.AddEmbed(embed);
                            mensaje = await ctx.EditFollowupAsync(mensaje.Id, whBuilder);
                        }
                        else
                        {
                            embed.Title = translations.tie;
                            embed.Description = translations.tie_desc;
                            embed.Color = Constants.YumikoColor;
                            whBuilder.AddEmbed(embed);
                            mensaje = await ctx.EditFollowupAsync(mensaje.Id, whBuilder);
                        }
                    }
                }
                else
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = translations.game_cancelled,
                        Description = translations.no_click_button,
                        Color = DiscordColor.Red,
                    }));
                    terminada = true;
                }

                if (cambiarTurno)
                {
                    turnoPrimerJugador = !turnoPrimerJugador;
                }
            }
            while (!terminada);
        }
    }
}
