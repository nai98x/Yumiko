namespace Yumiko.Services
{
    using GraphQL;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public static class GameServices
    {
        private static readonly GraphQLHttpClient GraphQlClient = new("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        public static async Task GetResultadosAsync(InteractionContext ctx, string topggToken, List<GameUser> participantes, int rondas, GameSettings settings)
        {
            string resultados;
            if (settings.Gamemode == Gamemode.Genres)
            {
                resultados = $"{settings.Gamemode.GetName().UppercaseFirst()}: {Formatter.Bold(settings.Difficulty.GetName())}\n\n";
            }
            else
            {
                resultados = $"{translations.difficulty}: {Formatter.Bold(settings.Difficulty.GetName())}\n\n";
            }

            participantes.Sort((x, y) => y.Puntaje.CompareTo(x.Puntaje));
            int tot = 0;
            int pos = 0;
            int lastScore = 0;
            foreach (GameUser uj in participantes)
            {
                if (lastScore != uj.Puntaje)
                {
                    pos++;
                }

                int porcentaje = (uj.Puntaje * 100) / rondas;
                switch (pos)
                {
                    case 1:
                        DiscordEmoji emoji1 = DiscordEmoji.FromName(ctx.Client, ":first_place:");
                        resultados += $"{emoji1} - {uj.Usuario.Mention}: {uj.Puntaje} {translations.guesses} ({porcentaje}%)\n";
                        break;
                    case 2:
                        DiscordEmoji emoji2 = DiscordEmoji.FromName(ctx.Client, ":second_place:");
                        resultados += $"{emoji2} - {uj.Usuario.Mention}: {uj.Puntaje} {translations.guesses} ({porcentaje}%)\n";
                        break;
                    case 3:
                        DiscordEmoji emoji3 = DiscordEmoji.FromName(ctx.Client, ":third_place:");
                        resultados += $"{emoji3} - {uj.Usuario.Mention}: {uj.Puntaje} {translations.guesses} ({porcentaje}%)\n";
                        break;
                    default:
                        resultados += $"{Formatter.Bold($"#{pos}")} - {uj.Usuario.Mention}: {uj.Puntaje} {translations.guesses} ({porcentaje}%)\n";
                        break;
                }

                lastScore = uj.Puntaje;
                tot += uj.Puntaje;
                await LeaderboardQuiz.AddRegistroAsync((long)ctx.Guild.Id, long.Parse(uj.Usuario.Id.ToString()), settings.Difficulty.ToSpanish(), uj.Puntaje, rondas, settings.Gamemode.ToSpanish());
            }

            resultados += $"\n{Formatter.Bold($"{translations.total} ({tot}/{rondas})")}";

            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = $"{translations.results} - {translations.guess_the} {settings.Gamemode.GetName()}",
                Description = resultados,
                Color = Constants.YumikoColor,
                Footer = new()
                {
                    Text = translations.see_stats,
                },
            }));
            await Common.ChequearVotoTopGGAsync(ctx, topggToken);
        }

        public static async Task PlayTriviaAsync(InteractionContext ctx, double timeoutGames, string topggToken, dynamic lista, GameSettings settings)
        {
            DiscordEmbedBuilder? embedAux = null;
            if (settings.Rondas > lista.Count)
            {
                settings.Rondas = lista.Count;
            }

            if (settings.Gamemode == Gamemode.Genres && settings.Genre == null)
            {
                await Common.GrabarLogErrorAsync(ctx, "PlayTriviaAsync - Genres with no settings.Genre assigned");
                return;
            }

            string juegoMostrar;
            if (ctx.Interaction.Locale!.StartsWith("es"))
            {
                juegoMostrar = settings.Gamemode.ToSpanish();
            }
            else
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                juegoMostrar = settings.Gamemode switch
                {
                    Gamemode.Genres => settings.Genre,
                    Gamemode.Studios => $"{translations.studio} {translations.from_the_anime}",
                    _ => settings.Gamemode.GetName().Remove(settings.Gamemode.GetName().Length - 1, 1),
                };
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }

            List<GameUser> participantes = new();
            int lastRonda;
            QuizRound quizRound;

            var singleton = Singleton.GetInstance();
            Quiz? match = singleton.GetCurrentTrivia(ctx.Guild.Id, ctx.Channel.Id);
            if (match != null)
            {
                return;
            }
            else
            {
                singleton.CurrentTrivias.Add(new Quiz
                {
                    GuildId = ctx.Guild.Id,
                    ChannelId = ctx.Channel.Id,
                    TimeoutTotal = timeoutGames,
                    Title = juegoMostrar,
                    CreatedBy = ctx.User,
                });
            }

            for (int ronda = 1; ronda <= settings.Rondas; ronda++)
            {
                quizRound = new();
                lastRonda = ronda;
                int random = Common.GetNumeroRandom(0, lista.Count - 1);
                var guid = Guid.NewGuid();
                dynamic elegido = lista[random];
                lista.Remove(lista[random]);
                DiscordEmoji corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                DiscordEmbedBuilder embedAcertar;
                embedAcertar = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Gold,
                    Title = $"{translations.guess_the} {juegoMostrar}",
                    Description = $"{translations.round} {ronda} {translations.of} {settings.Rondas}",
                    ImageUrl = elegido.Image,
                    Footer = new()
                    {
                        Text = $"{elegido.Favoritos} {corazon}",
                    },
                };

                var builder = new DiscordFollowupMessageBuilder();
                if (embedAux != null)
                {
                    builder.AddEmbed(embedAux);
                }

                builder.AddEmbed(embedAcertar);
                builder.AddComponents(
                    new DiscordButtonComponent(ButtonStyle.Primary, $"quiz-modal-{guid}", translations.guess),
                    new DiscordButtonComponent(ButtonStyle.Danger, $"quiz-cancel-{guid}", translations.finish_game));

                var msgAcertar = await ctx.FollowUpAsync(builder);
                string desc = string.Empty;
                string elegidoNom = string.Empty;

                switch (settings.Gamemode)
                {
                    case Gamemode.Characters:
                        desc = string.Format(translations.the_character_is, Formatter.Bold($"[{elegido.NameFull}]({elegido.SiteUrl})"), $"[{elegido.AnimePrincipal.TitleRomaji}]({elegido.AnimePrincipal.SiteUrl})");
                        elegidoNom = elegido.NameFull;
                        break;
                    case Gamemode.Animes:
                        desc = string.Format(translations.the_anime_is, Formatter.Bold($"[{elegido.TitleRomaji}]({elegido.SiteUrl})"));
                        if (elegido.TitleEnglish != null)
                        {
                            desc += $"\n{translations.in_english}: `{elegido.TitleEnglish}`";
                        }

                        elegidoNom = elegido.TitleRomaji;
                        break;
                    case Gamemode.Mangas:
                        desc = string.Format(translations.the_manga_is, Formatter.Bold($"[{elegido.TitleRomaji}]({elegido.SiteUrl})"));
                        if (elegido.TitleEnglish != null)
                        {
                            desc += $"\n{translations.in_english}: `{elegido.TitleEnglish}`";
                        }

                        elegidoNom = elegido.TitleRomaji;
                        break;
                    case Gamemode.Studios:
                        desc = string.Format(translations.the_studios_of_are, $"[{elegido.TitleRomaji}]({elegido.SiteUrl})") + "\n";
                        foreach (var studio in elegido.Estudios)
                        {
                            desc += $"- {Formatter.Bold($"[{studio.Nombre}]({studio.SiteUrl})")}\n";
                        }

                        Anime aux = (Anime)elegido;
                        elegidoNom = aux.Estudios!.First().Nombre;
                        break;
                    case Gamemode.Protagonists:
                        desc = string.Format(translations.the_protagonists_of_are, $"[{elegido.TitleRomaji}]({elegido.SiteUrl})") + "\n";
                        foreach (var personaje in elegido.Personajes)
                        {
                            desc += $"- {Formatter.Bold($"[{personaje.NameFull}]({personaje.SiteUrl})")}\n";
                        }

                        elegidoNom = elegido.Personajes[0].NameFull;
                        break;
                    case Gamemode.Genres:
                        desc = string.Format(translations.the_anime_is, $"[{elegido.TitleRomaji}]({elegido.SiteUrl})");
                        if (elegido.TitleEnglish != null)
                        {
                            desc += $"\n{translations.in_english}: `{elegido.TitleEnglish}`";
                        }

                        if (elegido.MediaRelacionada.Count > 0)
                        {
                            desc += $"\n\n{translations.related}:\n";
                            foreach (Anime anim in elegido.MediaRelacionada)
                            {
                                desc += $"- {Formatter.Bold($"[{anim.TitleRomaji}]({anim.SiteUrl})")} ({anim.Tipo?.UppercaseFirst()})\n";
                            }
                        }

                        elegidoNom = elegido.TitleRomaji;
                        break;
                    default:
                        throw new ArgumentException($"Programming error");
                }

                quizRound.Matches = SetupMatchesTrivia(settings.Gamemode, elegido);
                quizRound.Matches = quizRound.Matches.Select(x => x.ToLower()).ToList();
                singleton.UpdateCurrentRoundTrivia(ctx.Guild.Id, ctx.Channel.Id, quizRound);
                desc = Common.NormalizarDescription(desc);

                for (int cont = 0; cont <= timeoutGames * 10; cont++)
                {
                    var partida = singleton.GetCurrentTrivia(ctx.Guild.Id, ctx.Channel.Id);
                    if (partida == null)
                    {
                        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder()
                        {
                            Title = translations.error,
                            Description = translations.no_active_game,
                            Color = DiscordColor.Red,
                        }));
                        return;
                    }

                    var rondaActual = partida.CurrentRound;

                    if (partida.Canceled)
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Title = translations.game_cancelled,
                            Description = desc,
                            Color = DiscordColor.Red,
                        };

                        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
                        await GetResultadosAsync(ctx, topggToken, participantes, lastRonda, settings);
                        singleton.RemoveCurrentTrivia(ctx.Guild.Id, ctx.Channel.Id);
                        return;
                    }

                    if (rondaActual.Guessed && rondaActual.Guesser != null)
                    {
                        DiscordUser acertador = rondaActual.Guesser;
                        GameUser? usr = participantes.Find(x => x.Usuario == rondaActual.Guesser);
                        if (usr != null)
                        {
                            usr.Puntaje++;
                        }
                        else
                        {
                            participantes.Add(new()
                            {
                                Usuario = rondaActual.Guesser,
                                Puntaje = 1,
                            });
                        }

                        var tiempo = rondaActual.GuessTime - msgAcertar.CreationTimestamp;

                        embedAux = new DiscordEmbedBuilder
                        {
                            Title = string.Format(translations.user_has_guessed, acertador.FullName()),
                            Description = $"{desc}",
                            Color = DiscordColor.Green,
                            Footer = new()
                            {
                                IconUrl = acertador.AvatarUrl,
                                Text = $"{translations.time}: {tiempo.TotalSeconds}s",
                            },
                        };

                        break;
                    }

                    if (!rondaActual.Guessed && (cont == (timeoutGames * 10) / 2))
                    {
                        // Hora de hacer un tip
                        var rnd = new Random();
                        var stringRes = string.Empty;
                        int cantCaracteres = elegidoNom.Length / 3;
                        var indexes = Enumerable.Range(0, elegidoNom.Length - 1).ToList();
                        indexes.Shuffle(rnd);
                        indexes = indexes.Take(cantCaracteres).ToList();

                        for (int contador = 0; contador < elegidoNom.Length; contador++)
                        {
                            char caracterActual = elegidoNom[contador];
                            if (caracterActual != ' ')
                            {
                                if (indexes.Contains(contador))
                                {
                                    stringRes += $"{caracterActual} ";
                                }
                                else
                                {
                                    stringRes += $"_ ";
                                }
                            }
                            else
                            {
                                stringRes += $"  ";
                            }
                        }

                        await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                        {
                            Title = translations.hint,
                            Description = $"{Formatter.InlineCode(stringRes)}",
                            Color = Constants.YumikoColor,
                        }));
                    }

                    await Task.Delay(100);
                }

                var rondaActualTimedOut = singleton.GetCurrentTrivia(ctx.Guild.Id, ctx.Channel.Id)?.CurrentRound;
                if (rondaActualTimedOut != null && !rondaActualTimedOut.Guessed)
                {
                    embedAux = new DiscordEmbedBuilder
                    {
                        Title = translations.nobody_has_guessed,
                        Description = desc,
                        Color = DiscordColor.Red,
                    };
                }
            }

            if (embedAux != null)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embedAux));
            }

            await GetResultadosAsync(ctx, topggToken, participantes, settings.Rondas, settings);
            singleton.RemoveCurrentTrivia(ctx.Guild.Id, ctx.Channel.Id);
        }

        public static async Task<string> GetEstadisticasDificultadAsync(InteractionContext ctx, string tipoStats, string dificultad)
        {
            var res = await LeaderboardQuiz.GetLeaderboardAsync(ctx, dificultad, tipoStats);
            string stats = string.Empty;
            int pos = 0;
            int lastScore = 0;
            int lastPartidas = 0;
            DiscordEmoji emoji;
            foreach (var jugador in res)
            {
                if ((jugador.RondasTotales / jugador.PartidasTotales) >= 2)
                {
                    long x = jugador.UserId;
                    ulong id = (ulong)x;
                    try
                    {
                        DiscordMember miembro = await ctx.Guild.GetMemberAsync(id);
                        if (lastScore != jugador.PorcentajeAciertos || (lastScore == jugador.PorcentajeAciertos && lastPartidas != jugador.PartidasTotales))
                        {
                            pos++;
                        }

                        switch (pos)
                        {
                            case 1:
                                emoji = DiscordEmoji.FromName(ctx.Client, ":first_place:");
                                stats += $"{emoji} - {miembro.Mention} - {translations.guesses}: {Formatter.Bold($"{jugador.PorcentajeAciertos}%")} - {translations.games}: {Formatter.Bold($"{jugador.PartidasTotales}")}\n";
                                break;
                            case 2:
                                emoji = DiscordEmoji.FromName(ctx.Client, ":second_place:");
                                stats += $"{emoji} - {miembro.Mention} - {translations.guesses}: {Formatter.Bold($"{jugador.PorcentajeAciertos}%")} - {translations.games}: {Formatter.Bold($"{jugador.PartidasTotales}")}\n";
                                break;
                            case 3:
                                emoji = DiscordEmoji.FromName(ctx.Client, ":third_place:");
                                stats += $"{emoji} - {miembro.Mention} - {translations.guesses}: {Formatter.Bold($"{jugador.PorcentajeAciertos}%")} - {translations.games}: {Formatter.Bold($"{jugador.PartidasTotales}")}\n";
                                break;
                            default:
                                stats += $"{Formatter.Bold($"#{pos}")} - {miembro.Mention} - {translations.guesses}: {Formatter.Bold($"{jugador.PorcentajeAciertos}%")} - {translations.games}: {Formatter.Bold($"{jugador.PartidasTotales}")}\n";
                                break;
                        }

                        lastScore = jugador.PorcentajeAciertos;
                        lastPartidas = jugador.PartidasTotales;
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return stats;
        }

        public static async Task<DiscordEmbedBuilder> GetEstadisticasAsync(InteractionContext ctx, Gamemode gamemode)
        {
            string game = gamemode.ToSpanish();

            string facil = Common.NormalizarField(await GetEstadisticasDificultadAsync(ctx, game, "Fácil"));
            string media = Common.NormalizarField(await GetEstadisticasDificultadAsync(ctx, game, "Media"));
            string dificil = Common.NormalizarField(await GetEstadisticasDificultadAsync(ctx, game, "Dificil"));
            string extremo = Common.NormalizarField(await GetEstadisticasDificultadAsync(ctx, game, "Extremo"));

            string juegoMostrar;
            if (ctx.Interaction.Locale!.StartsWith("es"))
            {
                juegoMostrar = gamemode.ToSpanish();
            }
            else
            {
                juegoMostrar = $"{translations.guess_the} {gamemode.GetName().ToLower()}";
            }

            var builder = CrearEmbedStats($"{translations.stats} - {juegoMostrar}", facil, media, dificil, extremo);
            return builder;
        }

        public static async Task<DiscordEmbedBuilder> GetEstadisticasGeneroAsync(InteractionContext ctx, string genero)
        {
            string stats = await GetEstadisticasDificultadAsync(ctx, "genero", genero);

            return new DiscordEmbedBuilder
            {
                Title = $"{translations.stats} - {translations.guess_the} {genero}",
                Color = Constants.YumikoColor,
                Description = stats,
            };
        }

        public static DiscordEmbedBuilder CrearEmbedStats(string titulo, string facil, string media, string dificil, string extremo)
        {
            var builder = new DiscordEmbedBuilder
            {
                Title = titulo,
                Color = Constants.YumikoColor,
            };

            if (!string.IsNullOrEmpty(facil))
            {
                builder.AddField(Difficulty.Easy.GetName(), facil);
            }

            if (!string.IsNullOrEmpty(media))
            {
                builder.AddField(Difficulty.Normal.GetName(), media);
            }

            if (!string.IsNullOrEmpty(dificil))
            {
                builder.AddField(Difficulty.Hard.GetName(), dificil);
            }

            if (!string.IsNullOrEmpty(extremo))
            {
                builder.AddField(Difficulty.Extreme.GetName(), extremo);
            }

            return builder;
        }

        public static async Task<DiscordEmbedBuilder> GetUserTriviaStats(InteractionContext ctx, DiscordUser user)
        {
            var builder = new DiscordEmbedBuilder();
            var stats = await LeaderboardQuiz.GetStatsUserAsync(ctx.Guild.Id, user.Id);
            if (stats != null && stats.Count > 0)
            {
                string desc = string.Empty;
                foreach (var stat in stats)
                {
                    if (stat.Stats.Count > 0)
                    {
                        desc += $"**{translations.guess_the} {GetGamemodeLocalizatedSingular(stat.Gamemode).ToLower()}:**\n";
                        foreach (var s in stat.Stats)
                        {
                            desc += $"{translations.difficulty}: {Formatter.Bold(s.Dificultad)} - {translations.guesses}: {Formatter.Bold($"{s.PorcentajeAciertos}%")} - {translations.games}: {Formatter.Bold($"{s.PartidasTotales}")}\n";
                        }
                        desc += "\n";
                    }
                }
                if (!string.IsNullOrEmpty(desc))
                {
                    builder.Title = string.Format(translations.user_game_stats, user.FullName());
                    builder.Description = desc;
                    builder.Color = Constants.YumikoColor;
                }
            }
            else
            {
                builder.Title = string.Format(translations.user_game_stats, user.FullName());
                builder.Description = translations.no_stats_available;
                builder.Color = DiscordColor.Red;
            }

            return builder;
        }

        public static async Task<DiscordEmbedBuilder> GetUserTriviaGenreStats(InteractionContext ctx, ulong userId)
        {
            var builder = new DiscordEmbedBuilder();
            var stats = await LeaderboardQuiz.GetGenreStatsUserAsync(ctx.Guild.Id, userId);

            if (stats != null && stats.Count > 0)
            {
                string desc = string.Empty;
                foreach (var stat in stats)
                {
                    desc += $"{Formatter.Bold(stat.Dificultad)} - {translations.guesses}: {Formatter.Bold($"{stat.PorcentajeAciertos}%")} - {translations.games}: {Formatter.Bold($"{stat.PartidasTotales}")}\n";
                }

                builder.Title = translations.genres;
                builder.Description = desc;
                builder.Color = Constants.YumikoColor;
            }
            else
            {
                builder.Title = translations.genres;
                builder.Description = translations.no_stats_available;
                builder.Color = DiscordColor.Red;
            }

            return builder;
        }

        public static async Task<DiscordEmbedBuilder> GetUserHoLStats(InteractionContext ctx, ulong userId)
        {
            var builder = new DiscordEmbedBuilder();
            var stats = await LeaderboardHoL.GetStatsUserAsync(ctx.Guild.Id, userId);
            builder.Title = "Higher or Lower";
            if (stats != null)
            {
                builder.Description = $"{translations.score}: {Formatter.Bold($"{stats.puntuacion}")}\n";
                builder.Color = Constants.YumikoColor;
            }
            else
            {
                builder.Description = translations.no_stats_available;
                builder.Color = DiscordColor.Red;
            }

            return builder;
        }

        public static async Task<DiscordEmbedBuilder> GetEstadisticasHoLAsync(InteractionContext ctx)
        {
            var res = await LeaderboardHoL.GetLeaderboardAsync(ctx);
            string stats = string.Empty;
            int pos = 0;
            int lastScore = 0;
            DiscordEmoji emoji;
            foreach (var jugador in res)
            {
                long x = jugador.user_id;
                ulong id = (ulong)x;
                try
                {
                    DiscordMember miembro = await ctx.Guild.GetMemberAsync(id);
                    if (lastScore != jugador.puntuacion)
                    {
                        pos++;
                    }

                    if (pos > 10)
                    {
                        break;
                    }

                    switch (pos)
                    {
                        case 1:
                            emoji = DiscordEmoji.FromName(ctx.Client, ":first_place:");
                            stats += $"{emoji} - {miembro.Mention} - {translations.score}: {Formatter.Bold($"{jugador.puntuacion}")}\n";
                            break;
                        case 2:
                            emoji = DiscordEmoji.FromName(ctx.Client, ":second_place:");
                            stats += $"{emoji} - {miembro.Mention} - {translations.score}: {Formatter.Bold($"{jugador.puntuacion}")}\n";
                            break;
                        case 3:
                            emoji = DiscordEmoji.FromName(ctx.Client, ":third_place:");
                            stats += $"{emoji} - {miembro.Mention} - {translations.score}: {Formatter.Bold($"{jugador.puntuacion}")}\n";
                            break;
                        default:
                            stats += $"{Formatter.Bold($"#{pos}")} - {miembro.Mention} - {translations.score}: {Formatter.Bold($"{jugador.puntuacion}")}\n";
                            break;
                    }

                    lastScore = jugador.puntuacion;
                }
                catch (Exception)
                {
                }
            }

            return new DiscordEmbedBuilder()
            {
                Title = $"{translations.stats} - Higher or Lower",
                Description = stats,
                Color = Constants.YumikoColor,
            };
        }

        public static async Task EliminarEstadisticasAsync(InteractionContext ctx)
        {
            await LeaderboardQuiz.EliminarEstadisticasAsync(ctx, Gamemode.Characters.ToSpanish());
            await LeaderboardQuiz.EliminarEstadisticasAsync(ctx, Gamemode.Animes.ToSpanish());
            await LeaderboardQuiz.EliminarEstadisticasAsync(ctx, Gamemode.Mangas.ToSpanish());
            await LeaderboardQuiz.EliminarEstadisticasAsync(ctx, Gamemode.Studios.ToSpanish());
            await LeaderboardQuiz.EliminarEstadisticasAsync(ctx, Gamemode.Protagonists.ToSpanish());
            await LeaderboardHoL.EliminarEstadisticasAsync(ctx);
        }

        public static async Task<List<Anime>> GetMediaAsync(InteractionContext ctx, MediaType type, GameSettings settings, bool personajes, bool estudios, bool genero, bool mediaRelacionada)
        {
            List<Anime> animeList = new();
            DiscordMessage mensaje = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = $"{translations.retrieving} {type.GetName().ToLower()}s",
                Description = translations.please_wait,
                Color = Constants.YumikoColor,
                Footer = new()
                {
                    IconUrl = Constants.AnilistAvatarUrl,
                    Text = $"{translations.retrieved_from} AniList",
                },
            }));
            string mediaFiltros;
            if (genero)
            {
                if (ctx.Channel.IsNSFW)
                {
                    mediaFiltros = $"type: ANIME, sort: POPULARITY_DESC, genre: \"{settings.Genre}\", format_not_in:[MUSIC]";
                }
                else
                {
                    mediaFiltros = $"type: ANIME, sort: POPULARITY_DESC, genre: \"{settings.Genre}\", isAdult:false, format_not_in:[MUSIC]";
                }
            }
            else
            {
                if (ctx.Channel.IsNSFW)
                {
                    mediaFiltros = $"type: {type.GetName()}, sort: FAVOURITES_DESC, format_not_in:[MUSIC]";
                }
                else
                {
                    mediaFiltros = $"type: {type.GetName()}, sort: FAVOURITES_DESC, isAdult:false, format_not_in:[MUSIC]";
                }
            }

            string query = "query($pagina : Int){" +
                    "   Page(page: $pagina){" +
                   $"       media({mediaFiltros}){{" +
                    "           id," +
                    "           siteUrl," +
                    "           type," +
                    "           favourites," +
                    "           title{" +
                    "               romaji," +
                    "               english" +
                    "           }," +
                    "           averageScore," +
                    "           synonyms," +
                    "           coverImage{" +
                    "               large" +
                    "           }," +
                    "           characters(role: MAIN){" +
                    "               nodes{" +
                    "                   name{" +
                    "                       first," +
                    "                       last," +
                    "                       full" +
                    "                   }," +
                    "                   siteUrl," +
                    "                   favourites," +
                    "               }" +
                    "           }," +
                    "           studios{" +
                    "               nodes{" +
                    "                   name," +
                    "                   siteUrl," +
                    "                   favourites," +
                    "                   isAnimationStudio" +
                    "               }" +
                    "           }," +
                    "           relations{" +
                    "               edges{" +
                    "                   relationType," +
                    "                   node{" +
                    "                       id," +
                    "                       type," +
                    "                       siteUrl," +
                    "                       title{" +
                    "                           romaji," +
                    "                           english" +
                    "                       }," +
                    "                       synonyms" +
                    "                   }" +
                    "               }" +
                    "           }" +
                    "       }," +
                    "       pageInfo{" +
                    "           hasNextPage," +
                    "           lastPage" +
                    "       }" +
                    "   }" +
                    "}";

            int random1;
            int random2;

            if (genero)
            {
                var dataPre = await GraphQlClient.SendQueryAsync<dynamic>(new GraphQLRequest
                {
                    Query = query,
                    Variables = new
                    {
                        pagina = 1,
                    },
                });

                string lastPageStr = dataPre.Data.Page.pageInfo.lastPage;
                int lastPage = int.Parse(lastPageStr);

                settings.IterIni = 0;

                if (lastPage < 3)
                {
                    settings.IterFin = 1;
                }
                else
                {
                    settings.IterFin = Common.GetNumeroRandom(1, 9);
                    if (lastPage < settings.IterFin)
                    {
                        settings.IterFin = lastPage;
                    }
                }
            }

            if (settings.IterIni != settings.IterFin)
            {
                random1 = Common.GetNumeroRandom(settings.IterIni, settings.IterFin);
                do
                {
                    random2 = Common.GetNumeroRandom(settings.IterIni, settings.IterFin);
                }
                while (random1 == random2);
            }
            else
            {
                random1 = 0;
                random2 = 0;
            }

            bool firstloop = true, secondloop = true;

            if (random1 == 0 && random2 == 0)
            {
                secondloop = false;
            }

            string hasNextValue;
            int iter = 0;
            do
            {
                int i = 0;
                if (!firstloop && secondloop)
                {
                    secondloop = false;
                    i = random2;
                }

                if (firstloop)
                {
                    firstloop = false;
                    i = random1;
                }

                var request = new GraphQLRequest
                {
                    Query = query,
                    Variables = new
                    {
                        pagina = i,
                    },
                };
                try
                {
                    var data = await GraphQlClient.SendQueryAsync<dynamic>(request);
                    iter++;
                    hasNextValue = data.Data.Page.pageInfo.hasNextPage;
                    foreach (var x in data.Data.Page.media)
                    {
                        string titleEnglish = x.title.english;
                        string titleRomaji = x.title.romaji;
                        string id = x.id;
                        string scoreStr = x.averageScore;
                        bool tieneScore = int.TryParse(scoreStr, out int score);
                        if (!tieneScore)
                        {
                            score = -1;
                        }

                        Anime anim = new()
                        {
                            Id = int.Parse(id),
                            Image = x.coverImage.large,
                            TitleEnglish = titleEnglish,
                            TitleRomaji = titleRomaji,
                            TitleEnglishFormatted = Common.QuitarCaracteresEspeciales(titleEnglish),
                            TitleRomajiFormatted = Common.QuitarCaracteresEspeciales(titleRomaji),
                            SiteUrl = x.siteUrl,
                            Tipo = x.type,

                            Favoritos = x.favourites,
                            AvarageScore = score,
                            Estudios = new List<Studio>(),
                            Sinonimos = new List<string>(),
                            Personajes = new List<Character>(),
                            MediaRelacionada = new List<Anime>(),
                        };
                        foreach (var syn in x.synonyms)
                        {
                            string value = syn.Value;
                            string bien = Common.QuitarCaracteresEspeciales(value);
                            anim.Sinonimos.Add(bien);
                        }

                        if (personajes)
                        {
                            foreach (var character in x.characters.nodes)
                            {
                                anim.Personajes.Add(new Character()
                                {
                                    NameFull = character.name.full,
                                    NameFirst = character.name.first,
                                    NameLast = character.name.last,
                                    SiteUrl = character.siteUrl,
                                    Favoritos = character.favourites,
                                });
                            }
                        }

                        if (estudios)
                        {
                            foreach (var estudio in x.studios.nodes)
                            {
                                if (estudio.isAnimationStudio == "true")
                                {
                                    anim.Estudios.Add(new Studio()
                                    {
                                        Nombre = estudio.name,
                                        SiteUrl = estudio.siteUrl,
                                        Favoritos = estudio.favourites,
                                    });
                                }
                            }
                        }

                        if (mediaRelacionada)
                        {
                            foreach (var relation in x.relations.edges)
                            {
                                var animeR = relation.node;

                                string titleEnglishR = animeR.title.english;
                                string titleRomajiR = animeR.title.romaji;

                                Anime relationed = new()
                                {
                                    Relacion = relation.relationType,
                                    Id = animeR.id,
                                    SiteUrl = animeR.siteUrl,
                                    TitleEnglish = titleEnglishR,
                                    TitleRomaji = titleRomajiR,
                                    TitleEnglishFormatted = Common.QuitarCaracteresEspeciales(titleEnglishR),
                                    TitleRomajiFormatted = Common.QuitarCaracteresEspeciales(titleRomajiR),
                                    Sinonimos = new List<string>(),
                                    Tipo = animeR.type,
                                };

                                foreach (var synn in animeR.synonyms)
                                {
                                    string valueR = synn.Value;
                                    string bienR = Common.QuitarCaracteresEspeciales(valueR);
                                    relationed.Sinonimos.Add(bienR);
                                }

                                anim.MediaRelacionada.Add(relationed);
                            }
                        }

                        if ((!estudios && !personajes) || (estudios && anim.Estudios.Count > 0) || (personajes && anim.Personajes.Count > 0))
                        {
                            animeList.Add(anim);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Common.GrabarLogErrorAsync(ctx, $"{ex.Message}");
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{translations.unknown_error}: {ex.Message}"));
                    return animeList;
                }
            }
            while (hasNextValue.ToLower() == "true" && (firstloop || secondloop));
            await ctx.DeleteFollowupAsync(mensaje.Id);
            return animeList;
        }

        public static async Task<List<Character>> GetCharactersAsync(InteractionContext ctx, GameSettings settings)
        {
            var characterList = new List<Character>();
            DiscordMessage mensaje = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = $"{translations.retrieving} {translations.characters}",
                Description = translations.please_wait,
                Color = Constants.YumikoColor,
                Footer = new()
                {
                    IconUrl = Constants.AnilistAvatarUrl,
                    Text = $"{translations.retrieved_from} AniList",
                },
            }));
            string query = "query($pagina : Int){" +
                        "   Page(page: $pagina){" +
                        "       characters(sort: FAVOURITES_DESC){" +
                        "           siteUrl," +
                        "           favourites," +
                        "           name{" +
                        "               first," +
                        "               last," +
                        "               full" +
                        "           }," +
                        "           image{" +
                        "               large" +
                        "           }," +
                        "           media(sort: POPULARITY_DESC, perPage: 1){" +
                        "               nodes{" +
                        "                   title{" +
                        "                       romaji" +
                        "                   }," +
                        "                   siteUrl" +
                        "               }" +
                        "           }" +
                        "       }," +
                        "       pageInfo{" +
                        "           hasNextPage" +
                        "       }" +
                        "   }" +
                        "}";

            int random1 = Common.GetNumeroRandom(settings.IterIni, settings.IterFin);
            int random2;
            do
            {
                random2 = Common.GetNumeroRandom(settings.IterIni, settings.IterFin);
            }
            while (random1 == random2);
            bool firstloop = true, secondloop = true;

            string hasNextValue;
            do
            {
                int i = 0;
                if (!firstloop && secondloop)
                {
                    secondloop = false;
                    i = random2;
                }

                if (firstloop)
                {
                    firstloop = false;
                    i = random1;
                }

                var request = new GraphQLRequest
                {
                    Query = query,
                    Variables = new
                    {
                        pagina = i,
                    },
                };
                try
                {
                    string titleMedia = string.Empty, siteUrlMedia = string.Empty;
                    var data = await GraphQlClient.SendQueryAsync<dynamic>(request);
                    hasNextValue = data.Data.Page.pageInfo.hasNextPage;
                    foreach (var x in data.Data.Page.characters)
                    {
                        foreach (var m in x.media.nodes)
                        {
                            titleMedia = m.title.romaji;
                            siteUrlMedia = m.siteUrl;
                        }

                        Character c = new()
                        {
                            Image = x.image.large,
                            NameFirst = x.name.first,
                            NameLast = x.name.last,
                            NameFull = x.name.full,
                            SiteUrl = x.siteUrl,
                            Favoritos = x.favourites,
                            AnimePrincipal = new Anime()
                            {
                                TitleRomaji = titleMedia,
                                SiteUrl = siteUrlMedia,
                            },
                        };
                        characterList.Add(c);
                    }
                }
                catch (Exception ex)
                {
                    await Common.GrabarLogErrorAsync(ctx, $"Unknown error in GetCharacters: {ex.Message}");
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{translations.unknown_error}: {ex.Message}"));
                    return characterList;
                }
            }
            while (hasNextValue.ToLower() == "true" && (firstloop || secondloop));
            await ctx.DeleteFollowupAsync(mensaje.Id);
            return characterList;
        }

        public static async Task JugarAhorcadoAsync(InteractionContext ctx, dynamic elegido, HangmanGamemode gamemode)
        {
            var interactivity = ctx.Client.GetInteractivity();
            int errores = 0;
            string nameFull = string.Empty;
            nameFull = gamemode switch
            {
                HangmanGamemode.Characters => elegido.NameFull.ToLower().Trim(),
                HangmanGamemode.Animes => elegido.TitleRomaji.ToLower().Trim(),
                _ => throw new ArgumentException("Programming error"),
            };
            string nameFullParsed = Regex.Replace(nameFull, @"\s+", " ");
            char[] charsPj = nameFullParsed.ToCharArray();
            var charsEsp = charsPj.Distinct();
            var chars = charsEsp.ToList();
            chars.Remove((char)32);
            List<CaracterBool> caracteres = new();
            List<string> letrasUsadas = new();
            foreach (char c in chars)
            {
                if (char.IsLetter(c))
                {
                    caracteres.Add(new CaracterBool()
                    {
                        Caracter = c,
                        Acertado = false,
                    });
                }
                else
                {
                    caracteres.Add(new CaracterBool()
                    {
                        Caracter = c,
                        Acertado = true,
                    });
                }
            }

            bool partidaTerminada = false;
            List<GameUser> participantes = new();
            DiscordMessage msgAcertar;
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"{translations.hangman} ({gamemode.GetName()})",
                Description = translations.type_a_letter,
                Color = Constants.YumikoColor,
            };
            DiscordUser ganador = ctx.Member;
            string titRonda;
            DiscordColor colRonda;
            int rondas = 0;
            int rondasPerdidas = 0;
            string desc1 = string.Empty;
            desc1 = gamemode switch
            {
                HangmanGamemode.Characters => $"{translations.the_character_is} [{elegido.NameFull}]({elegido.SiteUrl}) {translations.from_the_anime} [{elegido.AnimePrincipal.TitleRomaji}]({elegido.AnimePrincipal.SiteUrl})",
                HangmanGamemode.Animes => $"{translations.the_anime_is} [{elegido.TitleRomaji}]({elegido.SiteUrl})",
                _ => throw new ArgumentException("Programming error"),
            };
            do
            {
                var guid = Guid.NewGuid();
                msgAcertar = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                    .AddComponents(
                        new DiscordButtonComponent(ButtonStyle.Primary, $"modal-letter-{guid}", translations.type_a_letter),
                        new DiscordButtonComponent(ButtonStyle.Success, $"modal-guess-{guid}", translations.guess),
                        new DiscordButtonComponent(ButtonStyle.Danger, $"cancel-{guid}", translations.finish_game))
                    .AddEmbed(embedBuilder));

                var resultBtn = await interactivity.WaitForButtonAsync(msgAcertar, TimeSpan.FromSeconds(30));

                if (!resultBtn.TimedOut)
                {
                    var btnInteraction = resultBtn.Result.Interaction;

                    if (resultBtn.Result.Id.StartsWith("cancel-"))
                    {
                        errores = 6;
                        partidaTerminada = true;
                    }
                    else
                    {
                        string modalId = $"modal-{btnInteraction.Id}";

                        var modal = new DiscordInteractionResponseBuilder()
                            .WithCustomId(modalId)
                            .WithTitle(translations.hangman)
                            .AddComponents(new TextInputComponent(label: translations.guess, customId: "value"));

                        await btnInteraction.CreateResponseAsync(InteractionResponseType.Modal, modal);

                        var modalResponse = await interactivity.WaitForModalAsync(modalId, btnInteraction.User);

                        if (!modalResponse.TimedOut)
                        {
                            var modalInteraction = modalResponse.Result.Interaction;
                            await modalInteraction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            var value = modalResponse.Result.Values["value"];
                            var tiempo = modalInteraction.CreationTimestamp - msgAcertar.CreationTimestamp;

                            if (resultBtn.Result.Id.StartsWith("modal-letter-"))
                            {
                                var acierto = caracteres.Find(x => x.Caracter.ToString() == value.ToLower().Trim());
                                if (acierto != null)
                                {
                                    acierto.Acertado = true;
                                    titRonda = $"{string.Format(translations.user_has_guessed, modalInteraction.User.FullName())}";
                                    colRonda = DiscordColor.Green;
                                    GameUser? usr = participantes.Find(x => x.Usuario.Id == modalInteraction.User.Id);
                                    if (usr != null)
                                    {
                                        usr.Puntaje++;
                                    }
                                    else
                                    {
                                        participantes.Add(new GameUser()
                                        {
                                            Usuario = modalInteraction.User,
                                            Puntaje = 1,
                                        });
                                    }
                                }
                                else
                                {
                                    errores++;
                                    rondasPerdidas++;
                                    titRonda = string.Format(translations.user_made_a_mistake, modalInteraction.User.FullName());
                                    colRonda = DiscordColor.Red;
                                    GameUser? usr = participantes.Find(x => x.Usuario.Id == modalInteraction.User.Id);
                                    if (usr == null)
                                    {
                                        participantes.Add(new GameUser()
                                        {
                                            Usuario = modalInteraction.User,
                                            Puntaje = 0,
                                        });
                                    }
                                }

                                var letraUsada = letrasUsadas.Find(x => x.ToLower().Trim() == value.ToLower().Trim());
                                if (letraUsada == null)
                                {
                                    letrasUsadas.Add(value.ToLower().Trim());
                                }

                                string letras = "`";
                                foreach (var c in charsPj)
                                {
                                    var registro = caracteres.Find(x => x.Caracter == c);

                                    // Por los espacios del string original
                                    if (registro != null)
                                    {
                                        if (registro.Acertado)
                                        {
                                            letras += c + " ";
                                        }
                                        else
                                        {
                                            letras += "_ ";
                                        }
                                    }
                                    else
                                    {
                                        letras += "` `";
                                    }
                                }

                                letras += "`\n\n";
                                string desc = string.Empty;
                                desc += letras;
                                desc += GetErroresAhorcado(errores);
                                desc += $"\n{Formatter.Bold($"{translations.letters_used}:")}\n";
                                foreach (var cc in letrasUsadas)
                                {
                                    desc += $"{Formatter.InlineCode(cc)} ";
                                }

                                embedBuilder = new DiscordEmbedBuilder()
                                {
                                    Title = titRonda,
                                    Description = desc,
                                    Color = colRonda,
                                    Footer = new()
                                    {
                                        Text = $"{translations.time}: {tiempo.TotalSeconds}s",
                                        IconUrl = modalInteraction.User.AvatarUrl,
                                    },
                                };
                            }
                            else if (resultBtn.Result.Id.StartsWith("modal-guess-"))
                            {
                                if ((gamemode == HangmanGamemode.Characters && value.ToLower().Trim() == elegido.NameFull.ToLower().Trim()) || (gamemode == HangmanGamemode.Animes && value.ToLower().Trim() == elegido.TitleRomaji.ToLower().Trim()))
                                {
                                    int aciertos = 0;
                                    foreach (var ccc in caracteres)
                                    {
                                        if (!ccc.Acertado)
                                        {
                                            ccc.Acertado = true;
                                            aciertos++;
                                        }
                                    }

                                    GameUser? usr = participantes.Find(x => x.Usuario == modalInteraction.User);
                                    if (usr != null)
                                    {
                                        usr.Puntaje += aciertos;
                                    }
                                    else
                                    {
                                        participantes.Add(new()
                                        {
                                            Usuario = modalInteraction.User,
                                            Puntaje = aciertos,
                                        });
                                    }

                                    ganador = modalInteraction.User;
                                    partidaTerminada = true;
                                }
                            }
                        }
                        else
                        {
                            errores++;
                            embedBuilder = new DiscordEmbedBuilder()
                            {
                                Title = translations.did_not_write_on_time,
                                Description = translations.write_any_letter_to_continue,
                                Color = DiscordColor.Red,
                            };
                        }
                    }
                }
                else
                {
                    errores++;
                    embedBuilder = new DiscordEmbedBuilder()
                    {
                        Title = translations.did_not_press_button_to_write,
                        Description = translations.write_any_letter_to_continue,
                        Color = DiscordColor.Red,
                    };
                }

                rondas++;
                if (errores >= 6 || (caracteres.Find(x => x.Acertado == false) == null))
                {
                    partidaTerminada = true;
                }
            }
            while (!partidaTerminada);

            if (errores == 6)
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = translations.defeat,
                    Description = $"{desc1}",
                    ImageUrl = elegido.Image,
                    Color = DiscordColor.Red,
                }));
            }
            else
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder()
                {
                    Title = translations.victory,
                    Description = $"{desc1}\n\n" +
                    $"{translations.winner}: {ganador.Mention}",
                    ImageUrl = elegido.Image,
                    Color = DiscordColor.Green,
                }).AddMentions(Mentions.All));
            }
        }

        public static async Task<GameSettings> ElegirGeneroAsync(InteractionContext ctx, double timeoutGeneral, InteractivityExtension interactivity)
        {
            string query =
                    "query{" +
                    "   GenreCollection" +
                    "}";
            var request = new GraphQLRequest
            {
                Query = query,
            };
            List<string> generos = new();
            try
            {
                var data = await GraphQlClient.SendQueryAsync<dynamic>(request);
                foreach (var x in data.Data.GenreCollection)
                {
                    string nombre = x;
                    if (ctx.Channel.IsNSFW || (!ctx.Channel.IsNSFW && nombre.ToLower().Trim() != "hentai"))
                    {
                        generos.Add(nombre);
                    }
                }
            }
            catch (Exception ex)
            {
                await Common.GrabarLogErrorAsync(ctx, $"{ex.Message}");
                return new()
                {
                    Ok = false,
                    MsgError = translations.unknown_error,
                };
            }

            int iter = 0;
            List<DiscordSelectComponentOption> options = new();
            string customId = "dropdownGetElegidoGenero";

            foreach (var nomGenero in generos)
            {
                if (iter >= 25)
                {
                    break;
                }

                iter++;
                options.Add(new DiscordSelectComponentOption(Common.NormalizarBoton(nomGenero), $"{iter}"));
            }

            var dropdown = new DiscordSelectComponent(customId, translations.select_a_genre, options);

            var embed = new DiscordEmbedBuilder
            {
                Color = Constants.YumikoColor,
                Title = translations.choose_an_option,
            };

            DiscordMessage msgGenero = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddComponents(dropdown).AddEmbed(embed));

            var interGenero = await interactivity.WaitForSelectAsync(msgGenero, ctx.User, customId, TimeSpan.FromSeconds(timeoutGeneral));
            if (!interGenero.TimedOut)
            {
                var resultado = interGenero.Result;
                int id = int.Parse(resultado.Values[0]);
                string generoElegido = generos[id - 1];

                return new()
                {
                    Genre = generoElegido,
                    Ok = true,
                };
            }
            else
            {
                await ctx.DeleteResponseAsync();
                return new()
                {
                    Ok = false,
                    MsgError = translations.timed_out_choosing_genre,
                };
            }
        }

        public static (List<DiscordComponent>, bool) GetBotonesTicTacToe(string idClickeado, List<DiscordComponent> listaBotones, bool primerJugador)
        {
            List<DiscordComponent> aux = new();
            bool siguienteTurno = true;

            foreach (DiscordComponent obj in listaBotones)
            {
                if (obj.CustomId == idClickeado)
                {
                    if (primerJugador)
                    {
                        aux.Add(new DiscordButtonComponent(ButtonStyle.Success, idClickeado, "[X]", true));
                    }
                    else
                    {
                        aux.Add(new DiscordButtonComponent(ButtonStyle.Danger, idClickeado, "[O]", true));
                    }
                }
                else
                {
                    aux.Add(obj);
                }
            }

            return (aux, siguienteTurno);
        }

        public static (bool, DiscordUser?) PartidaTerminadaTicTacToe(List<DiscordComponent> listaBotones, DiscordUser jugador1, DiscordUser jugador2)
        {
            bool ganadorPlayer1 = CheckearGanadorTicTacToe(listaBotones, ButtonStyle.Success);
            if (ganadorPlayer1)
            {
                return (true, jugador1);
            }

            bool ganadorPlayer2 = CheckearGanadorTicTacToe(listaBotones, ButtonStyle.Danger);
            if (ganadorPlayer2)
            {
                return (true, jugador2);
            }

            if (!CheckearMovimientosPosiblesTicTacToe(listaBotones))
            {
                return (true, null);
            }

            return (false, null);
        }

        private static string GetErroresAhorcado(int errores)
        {
            string desc = string.Empty;

            switch (errores)
            {
                case 0:
                    desc += ". ┌─────┐\n" +
                    ".┃...............┋\n" +
                    ".┃...............┋\n" +
                    ".┃\n" +
                    ".┃\n" +
                    ".┃\n" +
                    "/-\\" +
                    "\n";
                    break;
                case 1:
                    desc += ". ┌─────┐\n" +
                    ".┃...............┋\n" +
                    ".┃...............┋\n" +
                    ".┃.............:dizzy_face: \n" +
                    ".┃\n" +
                    ".┃\n" +
                    "/-\\" +
                    "\n";
                    break;
                case 2:
                    desc += ". ┌─────┐\n" +
                    ".┃...............┋\n" +
                    ".┃...............┋\n" +
                    ".┃.............:dizzy_face: \n" +
                    ".┃............./\n" +
                    ".┃\n" +
                    "/-\\" +
                    "\n";
                    break;
                case 3:
                    desc += ". ┌─────┐\n" +
                    ".┃...............┋\n" +
                    ".┃...............┋\n" +
                    ".┃.............:dizzy_face: \n" +
                    ".┃............./ |\n" +
                    ".┃\n" +
                    "/-\\" +
                    "\n";
                    break;
                case 4:
                    desc += ". ┌─────┐\n" +
                    ".┃...............┋\n" +
                    ".┃...............┋\n" +
                    ".┃.............:dizzy_face: \n" +
                    ".┃............./ | \\   \n" +
                    ".┃\n" +
                    "/-\\" +
                    "\n";
                    break;
                case 5:
                    desc += ". ┌─────┐\n" +
                    ".┃...............┋\n" +
                    ".┃...............┋\n" +
                    ".┃.............:dizzy_face: \n" +
                    ".┃............./ | \\   \n" +
                    ".┃............../\n" +
                    "/-\\" +
                    "\n";
                    break;
                case 6:
                    desc += ". ┌─────┐\n" +
                    ".┃...............┋\n" +
                    ".┃...............┋\n" +
                    ".┃.............:dizzy_face: \n" +
                    ".┃............./ | \\   \n" +
                    ".┃............../\\ \n" +
                    "/-\\" +
                    "\n";
                    break;
            }

            return desc;
        }

        private static bool CheckearGanadorTicTacToe(List<DiscordComponent> listaBotones, ButtonStyle style)
        {
            bool patronH1 = true;
            bool patronH2 = true;
            bool patronH3 = true;
            bool patronV1 = true;
            bool patronV3 = true;
            bool patronV2 = true;
            bool patronD1 = true;
            bool patronD2 = true;

            for (int i = 0; i < listaBotones.Count; i++)
            {
                DiscordButtonComponent boton = (DiscordButtonComponent)listaBotones[i];
                if (
                    !boton.Disabled || boton.Style != style ||
                    (boton.Disabled && ((style == ButtonStyle.Success && boton.Style == ButtonStyle.Danger) || (style == ButtonStyle.Danger && boton.Style == ButtonStyle.Success))))
                {
                    switch (i)
                    {
                        case 0:
                            patronH1 = false;
                            patronV1 = false;
                            patronD1 = false;
                            break;
                        case 1:
                            patronH1 = false;
                            patronV2 = false;
                            break;
                        case 2:
                            patronH1 = false;
                            patronV3 = false;
                            patronD2 = false;
                            break;
                        case 3:
                            patronH2 = false;
                            patronV1 = false;
                            break;
                        case 4:
                            patronH2 = false;
                            patronV2 = false;
                            patronD1 = false;
                            patronD2 = false;
                            break;
                        case 5:
                            patronH2 = false;
                            patronV3 = false;
                            break;
                        case 6:
                            patronH3 = false;
                            patronV1 = false;
                            patronD2 = false;
                            break;
                        case 7:
                            patronH3 = false;
                            patronV2 = false;
                            break;
                        case 8:
                            patronH3 = false;
                            patronV3 = false;
                            patronD1 = false;
                            break;
                    }
                }
            }

            bool ganador = patronH1 || patronH2 || patronH3 || patronV1 || patronV2 || patronV3 || patronD1 || patronD2;
            if (ganador)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool CheckearMovimientosPosiblesTicTacToe(List<DiscordComponent> listaBotones)
        {
            foreach (DiscordComponent c in listaBotones)
            {
                DiscordButtonComponent boton = (DiscordButtonComponent)c;
                if (!boton.Disabled)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<string> SetupMatchesTrivia(Gamemode gamemode, dynamic elegido)
        {
            List<string> matches = new();

            switch (gamemode)
            {
                case Gamemode.Characters:
                    Character elegidoP = elegido;

                    if (elegidoP.NameFull != null)
                    {
                        matches.Add(elegidoP.NameFull);
                    }

                    if (elegidoP.NameFirst != null)
                    {
                        matches.Add(elegidoP.NameFirst);
                    }

                    if (elegidoP.NameLast != null)
                    {
                        matches.Add(elegidoP.NameLast);
                    }

                    break;
                case Gamemode.Animes:
                    Anime elegidoA = elegido;

                    if (elegidoA.TitleRomaji != null)
                    {
                        matches.Add(elegidoA.TitleRomaji);
                    }

                    if (elegidoA.TitleEnglish != null)
                    {
                        matches.Add(elegidoA.TitleEnglish);
                    }

                    if (elegidoA.TitleRomajiFormatted != null)
                    {
                        matches.Add(elegidoA.TitleRomajiFormatted);
                    }

                    if (elegidoA.TitleEnglishFormatted != null)
                    {
                        matches.Add(elegidoA.TitleEnglishFormatted);
                    }

                    if (elegidoA.Sinonimos != null && elegidoA.Sinonimos.Count > 0)
                    {
                        matches.AddRange(elegidoA.Sinonimos);
                    }

                    if (elegidoA.MediaRelacionada != null && elegidoA.MediaRelacionada.Count > 0)
                    {
                        foreach (Anime anim in elegidoA.MediaRelacionada)
                        {
                            if (anim.TitleRomaji != null)
                            {
                                matches.Add(anim.TitleRomaji);
                            }

                            if (anim.TitleEnglish != null)
                            {
                                matches.Add(anim.TitleEnglish);
                            }

                            if (anim.TitleRomajiFormatted != null)
                            {
                                matches.Add(anim.TitleRomajiFormatted);
                            }

                            if (anim.TitleEnglishFormatted != null)
                            {
                                matches.Add(anim.TitleEnglishFormatted);
                            }

                            if (anim.Sinonimos != null && anim.Sinonimos.Count > 0)
                            {
                                matches.AddRange(anim.Sinonimos);
                            }
                        }
                    }

                    break;
                case Gamemode.Mangas:
                    Anime elegidoM = elegido;

                    if (elegidoM.TitleRomaji != null)
                    {
                        matches.Add(elegidoM.TitleRomaji);
                    }

                    if (elegidoM.TitleEnglish != null)
                    {
                        matches.Add(elegidoM.TitleEnglish);
                    }

                    if (elegidoM.TitleRomajiFormatted != null)
                    {
                        matches.Add(elegidoM.TitleRomajiFormatted);
                    }

                    if (elegidoM.TitleEnglishFormatted != null)
                    {
                        matches.Add(elegidoM.TitleEnglishFormatted);
                    }

                    if (elegidoM.Sinonimos != null && elegidoM.Sinonimos.Count > 0)
                    {
                        matches.AddRange(elegidoM.Sinonimos);
                    }

                    break;
                case Gamemode.Studios:
                    Anime elegidoS = elegido;

                    if (elegidoS.Estudios != null && elegidoS.Estudios.Count > 0)
                    {
                        matches.AddRange(elegidoS.Estudios.Select(x => x.Nombre));
                    }

                    break;
                case Gamemode.Protagonists:
                    Anime elegidoPr = elegido;

                    if (elegidoPr.Personajes != null && elegidoPr.Personajes.Count > 0)
                    {
                        foreach (Character pj in elegidoPr.Personajes)
                        {
                            if (pj.NameFull != null)
                            {
                                matches.Add(pj.NameFull);
                            }

                            if (pj.NameFirst != null)
                            {
                                matches.Add(pj.NameFirst);
                            }

                            if (pj.NameLast != null)
                            {
                                matches.Add(pj.NameLast);
                            }
                        }
                    }

                    break;
                case Gamemode.Genres:
                    Anime elegidoG = elegido;

                    if (elegidoG.TitleRomaji != null)
                    {
                        matches.Add(elegidoG.TitleRomaji);
                    }

                    if (elegidoG.TitleEnglish != null)
                    {
                        matches.Add(elegidoG.TitleEnglish);
                    }

                    if (elegidoG.TitleRomajiFormatted != null)
                    {
                        matches.Add(elegidoG.TitleRomajiFormatted);
                    }

                    if (elegidoG.TitleEnglishFormatted != null)
                    {
                        matches.Add(elegidoG.TitleEnglishFormatted);
                    }

                    if (elegidoG.Sinonimos != null && elegidoG.Sinonimos.Count > 0)
                    {
                        matches.AddRange(elegidoG.Sinonimos);
                    }

                    if (elegidoG.MediaRelacionada != null && elegidoG.MediaRelacionada.Count > 0)
                    {
                        foreach (Anime anim in elegidoG.MediaRelacionada)
                        {
                            if (anim.TitleRomaji != null)
                            {
                                matches.Add(anim.TitleRomaji);
                            }

                            if (anim.TitleEnglish != null)
                            {
                                matches.Add(anim.TitleEnglish);
                            }

                            if (anim.TitleRomajiFormatted != null)
                            {
                                matches.Add(anim.TitleRomajiFormatted);
                            }

                            if (anim.TitleEnglishFormatted != null)
                            {
                                matches.Add(anim.TitleEnglishFormatted);
                            }

                            if (anim.Sinonimos != null && anim.Sinonimos.Count > 0)
                            {
                                matches.AddRange(anim.Sinonimos);
                            }
                        }
                    }

                    break;
                default:
                    throw new ArgumentException("Programming error");
            }

            return matches;
        }

        private static string GetGamemodeLocalizatedSingular(Gamemode gamemode)
        {
            return gamemode switch
            {
                Gamemode.Characters => translations.character,
                Gamemode.Animes => translations.anime,
                Gamemode.Mangas => translations.manga,
                Gamemode.Protagonists => translations.protagonist,
                Gamemode.Genres => translations.genre,
                Gamemode.Studios => translations.studio,
                _ => throw new ArgumentException("Programming error"),
            };
        }
    }
}
