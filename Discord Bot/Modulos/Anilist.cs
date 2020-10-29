using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus.Entities;
using System;
using DSharpPlus.Interactivity;
using GraphQL.Client.Http;
using GraphQL;
using GraphQL.Client.Serializer.Newtonsoft;
using System.Linq;
using System.Configuration;
using GraphQL.Client.Abstractions.Utilities;

namespace Discord_Bot.Modulos
{
    public class Anilist : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        private readonly GraphQLHttpClient graphQLClient = new GraphQLHttpClient("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        [Command("quizC"), Aliases("adivinaelpersonaje")]
        public async Task QuizCharactersGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await InicializarJuego(ctx, interactivity);
            if (settings.Ok)
            {
                int rondas = settings.Rondas;
                string dificultadStr = settings.Dificultad;
                int iterIni = settings.IterIni;
                int iterFin = settings.IterFin;
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el personaje",
                    Description = $"Sesión iniciada por {ctx.User.Mention}",
                    Color = new DiscordColor(78, 63, 96)
                }.AddField("Rondas", $"{rondas}").AddField("Dificultad", $"{dificultadStr}");
                await ctx.RespondAsync(embed: embebido).ConfigureAwait(false);
                List<Character> characterList = new List<Character>();
                Random rnd = new Random();
                List<UsuarioJuego> participantes = new List<UsuarioJuego>();
                DiscordMessage mensaje = await ctx.RespondAsync($"Obteniendo pesonajes...").ConfigureAwait(false);
                for (int i = iterIni; i <= iterFin; i++)
                {
                    var request = new GraphQLRequest
                    {
                        Query =
                        "query($pagina : Int){" +
                        "   Page(page: $pagina){" +
                        "       characters(sort: FAVOURITES_DESC){" +
                        "           siteUrl," +
                        "           name{" +
                        "               first," +
                        "               last," +
                        "               full" +
                        "           }," +
                        "           image{" +
                        "               large" +
                        "           }" +
                        "       }" +
                        "   }" +
                        "}",
                        Variables = new
                        {
                            pagina = i
                        }
                    };
                    try
                    {
                        var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                        foreach (var x in data.Data.Page.characters)
                        {
                            characterList.Add(new Character()
                            {
                                Image = x.image.large,
                                NameFull = x.name.full,
                                NameFirst = x.name.first,
                                NameLast = x.name.last,
                                SiteUrl = x.siteUrl
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        DiscordMessage msg = ex.Message switch
                        {
                            _ => await ctx.RespondAsync($"Error inesperado").ConfigureAwait(false),
                        };
                        await Task.Delay(3000);
                        await ctx.Message.DeleteAsync("Auto borrado de yumiko");
                        await msg.DeleteAsync("Auto borrado de yumiko");
                        return;
                    }
                }
                await mensaje.DeleteAsync("Auto borrado de Yumiko");
                int lastRonda;
                for (int ronda = 1; ronda <= rondas; ronda++)
                {
                    lastRonda = ronda;
                    int random = funciones.GetNumeroRandom(0, characterList.Count - 1);
                    Character elegido = characterList[random];
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Gold,
                        Title = "Adivina el personaje",
                        Description = $"Ronda {ronda} de {rondas}",
                        ImageUrl = elegido.Image
                    }).ConfigureAwait(false);
                    var msg = await interactivity.WaitForMessageAsync
                        (xm => (xm.Channel == ctx.Channel) &&
                        (xm.Content.ToLower().Trim() == elegido.NameFull.ToLower().Trim() || xm.Content.ToLower().Trim() == elegido.NameFirst.ToLower().Trim() || (elegido.NameLast != null && xm.Content.ToLower().Trim() == elegido.NameLast.ToLower().Trim())) || (xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User)
                        , TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["GuessTimeGames"])));
                    if (!msg.TimedOut)
                    {
                        if (msg.Result.Author == ctx.User && msg.Result.Content.ToLower() == "cancelar")
                        {
                            await ctx.RespondAsync($"El juego ha sido cancelado por **{ctx.User.Username}#{ctx.User.Discriminator}**").ConfigureAwait(false);
                            await GetResultados(ctx, participantes, lastRonda);
                            return;
                        }
                        DiscordMember acertador = await ctx.Guild.GetMemberAsync(msg.Result.Author.Id);
                        UsuarioJuego usr = participantes.Find(x => x.Usuario == msg.Result.Author);
                        if (usr != null)
                        {
                            usr.Puntaje++;
                        }
                        else
                        {
                            participantes.Add(new UsuarioJuego()
                            {
                                Usuario = msg.Result.Author,
                                Puntaje = 1
                            });
                        }
                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = $"¡**{acertador.DisplayName}** ha acertado!",
                            Description = $"El nombre es: [{elegido.NameFull}]({elegido.SiteUrl})",
                            Color = DiscordColor.Green
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "¡Nadie ha acertado!",
                            Description = $"El nombre era: [{elegido.NameFull}]({elegido.SiteUrl})",
                            Color = DiscordColor.Red
                        }).ConfigureAwait(false);
                    }
                }
                await GetResultados(ctx, participantes, rondas);
            }
            else
            {
                var error = await ctx.RespondAsync(settings.MsgError).ConfigureAwait(false);
            }
        }

        [Command("quizA"), Aliases("adivinaelanime")]
        public async Task QuizAnimeGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            SettingsJuego settings = await InicializarJuego(ctx, interactivity);
            if (settings.Ok)
            {
                int rondas = settings.Rondas;
                string dificultadStr = settings.Dificultad;
                int iterIni = settings.IterIni;
                int iterFin = settings.IterFin;
                DiscordEmbed embebido = new DiscordEmbedBuilder
                {
                    Title = "Adivina el anime",
                    Description = $"Sesión iniciada por {ctx.User.Mention}",
                    Color = new DiscordColor(78, 63, 96)
                }.AddField("Rondas", $"{rondas}").AddField("Dificultad", $"{dificultadStr}");
                await ctx.RespondAsync(embed: embebido).ConfigureAwait(false);
                Random rnd = new Random();
                List<UsuarioJuego> participantes = new List<UsuarioJuego>();
                DiscordMessage mensaje = await ctx.RespondAsync($"Obteniendo personajes...").ConfigureAwait(false);
                var characterList = new List<Character>();
                for (int i = iterIni; i <= iterFin; i++)
                {
                    var request = new GraphQLRequest
                    {
                        Query =
                        "query($pagina : Int){" +
                        "   Page(page: $pagina){" +
                        "       characters(sort: FAVOURITES_DESC){" +
                        "           siteUrl," +
                        "           name{" +
                        "               full" +
                        "           }," +
                        "           image{" +
                        "               large" +
                        "           }," +
                        "           media(type:ANIME){" +
                        "               nodes{" +
                        "                   title{" +
                        "                       romaji," +
                        "                       english" +
                        "                   }," +
                        "                   siteUrl" +
                        "               }" +
                        "           }" +
                        "       }" +
                        "   }" +
                        "}",
                        Variables = new
                        {
                            pagina = i
                        }
                    };
                    try
                    {
                        var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                        foreach (var x in data.Data.Page.characters)
                        {
                            Character c = new Character()
                            {
                                Image = x.image.large,
                                NameFull = x.name.full,
                                SiteUrl = x.siteUrl,
                                Animes = new List<Anime>()
                            };
                            foreach (var y in x.media.nodes)
                            {
                                c.Animes.Add(new Anime()
                                {
                                    TitleEnglish = y.title.english,
                                    TitleRomaji = y.title.romaji,
                                    SiteUrl = y.siteUrl
                                });
                            }
                            if (c.Animes.Count() > 0)
                            {
                                characterList.Add(c);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DiscordMessage msg = ex.Message switch
                        {
                            _ => await ctx.RespondAsync($"Error inesperado").ConfigureAwait(false),
                        };
                        await Task.Delay(3000);
                        await ctx.Message.DeleteAsync("Auto borrado de yumiko");
                        await msg.DeleteAsync("Auto borrado de yumiko");
                        return;
                    }
                }
                await mensaje.DeleteAsync("Auto borrado de Yumiko");
                int lastRonda;
                for (int ronda = 1; ronda <= rondas; ronda++)
                {
                    lastRonda = ronda;
                    int random = funciones.GetNumeroRandom(0, characterList.Count - 1);
                    Character elegido = characterList[random];
                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Gold,
                        Title = $"Adivina el anime del personaje",
                        Description = $"Ronda {ronda} de {rondas}",
                        ImageUrl = elegido.Image
                    }).ConfigureAwait(false);
                    var msg = await interactivity.WaitForMessageAsync
                        (xm => (xm.Channel == ctx.Channel) &&
                        ((xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User) ||
                        (elegido.Animes.Find(x => x.TitleEnglish != null && x.TitleEnglish.ToLower().Trim() == xm.Content.ToLower().Trim()) != null) || 
                        (elegido.Animes.Find(x => x.TitleRomaji != null && x.TitleRomaji.ToLower().Trim() == xm.Content.ToLower().Trim()) != null)), 
                        TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["GuessTimeGames"])));
                    string descAnimes = $"Los animes de [{elegido.NameFull}]({elegido.SiteUrl}) son:\n\n";
                    foreach (Anime anim in elegido.Animes)
                    {
                        descAnimes += $"- [{anim.TitleRomaji}]({anim.SiteUrl})\n";
                    }
                    if (!msg.TimedOut)
                    {
                        if (msg.Result.Author == ctx.User && msg.Result.Content.ToLower() == "cancelar")
                        {
                            await ctx.RespondAsync($"El juego ha sido cancelado por **{ctx.User.Username}#{ctx.User.Discriminator}**").ConfigureAwait(false);
                            await GetResultados(ctx, participantes, lastRonda);
                            return;
                        }
                        DiscordMember acertador = await ctx.Guild.GetMemberAsync(msg.Result.Author.Id);
                        UsuarioJuego usr = participantes.Find(x => x.Usuario == msg.Result.Author);
                        if (usr != null)
                        {
                            usr.Puntaje++;
                        }
                        else
                        {
                            participantes.Add(new UsuarioJuego()
                            {
                                Usuario = msg.Result.Author,
                                Puntaje = 1
                            });
                        }
                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = $"¡**{acertador.DisplayName}** ha acertado!",
                            Description = descAnimes,
                            Color = DiscordColor.Green
                        }).ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "¡Nadie ha acertado!",
                            Description = descAnimes,
                            Color = DiscordColor.Red
                        }).ConfigureAwait(false);
                    }
                }
                await GetResultados(ctx, participantes, rondas);
            }
            else
            {
                var error = await ctx.RespondAsync(settings.MsgError).ConfigureAwait(false);
            }
        }

        [Command("anilist"), Aliases("user")]
        public async Task Profile(CommandContext ctx, string usuario)
        {
            var request = new GraphQLRequest
            {
                Query =
                "query($nombre : String){" +
                "   User(search: $nombre){" +
                "       id," +
                "       name," +
                "       siteUrl," +
                "       avatar{" +
                "           medium" +
                "       }" +
                "       bannerImage," +
                "       options{" +
                "           titleLanguage," +
                "           displayAdultContent," +
                "           profileColor" +
                "       }" +
                "       statistics{" +
                "           anime{" +
                "               count," +
                "               episodesWatched," +
                "               meanScore" +
                "           }," +
                "           manga{" +
                "               count," +
                "               chaptersRead," +
                "               meanScore" +
                "           }" +
                "       }," +
                "       favourites{" +
                "           anime(perPage:3){" +
                "               nodes{" +
                "                   title{" +
                "                       romaji" +
                "                   }," +
                "                   siteUrl" +
                "               }" +
                "           }," +
                "           manga(perPage:3){" +
                "               nodes{" +
                "                   title{" +
                "                       romaji" +
                "                   }," +
                "                   siteUrl" +
                "               }" +
                "           }," +
                "           characters(perPage:3){" +
                "               nodes{" +
                "                   name{" +
                "                       full" +
                "                   }," +
                "                   siteUrl" +
                "               }" +
                "           }," +
                "           staff(perPage:3){" +
                "               nodes{" +
                "                   name{" +
                "                       full" +
                "                   }," +
                "                   siteUrl" +
                "               }" +
                "           }," +
                "           studios(perPage:3){" +
                "               nodes{" +
                "                   name," +
                "                   siteUrl" +
                "               }" +
                "           }" +
                "       }" +
                "   }" +
                "}",
                Variables = new
                {
                    nombre = usuario
                }
            };
            try
            {
                var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                if (data.Data != null)
                {
                    string nsfw1 = data.Data.User.options.displayAdultContent;
                    string nsfw;
                    if (nsfw1 == "True")
                        nsfw = "Si";
                    else
                        nsfw = "No";
                    string animeStats = $"Total: `{data.Data.User.statistics.anime.count}`\nEpisodios: `{data.Data.User.statistics.anime.episodesWatched}`\nPuntaje promedio: `{data.Data.User.statistics.anime.meanScore}`";
                    string mangaStats = $"Total: `{data.Data.User.statistics.manga.count}`\nLeído: `{data.Data.User.statistics.manga.chaptersRead}`\nPuntaje promedio: `{data.Data.User.statistics.manga.meanScore}`";
                    string options = $"Titulos: `{data.Data.User.options.titleLanguage}`\nNSFW: `{nsfw}`\nColor: `{data.Data.User.options.profileColor}`";
                    string favoriteAnime = "";
                    foreach (var anime in data.Data.User.favourites.anime.nodes)
                    {
                        favoriteAnime += $"[{anime.title.romaji}]({anime.siteUrl})\n";
                    }
                    string favoriteManga = "";
                    foreach (var manga in data.Data.User.favourites.manga.nodes)
                    {
                        favoriteManga += $"[{manga.title.romaji}]({manga.siteUrl})\n";
                    }
                    string favoriteCharacters = "";
                    foreach (var character in data.Data.User.favourites.characters.nodes)
                    {
                        favoriteCharacters += $"[{character.name.full}]({character.siteUrl})\n";
                    }
                    string favoriteStaff = "";
                    foreach (var staff in data.Data.User.favourites.staff.nodes)
                    {
                        favoriteStaff += $"[{staff.name.full}]({staff.siteUrl})\n";
                    }
                    string favoriteStudios = "";
                    foreach (var studio in data.Data.User.favourites.studios.nodes)
                    {
                        favoriteStudios += $"[{studio.name}]({studio.siteUrl})\n";
                    }
                    string nombre = data.Data.User.name;
                    string avatar = data.Data.User.avatar.medium;
                    string siteurl = data.Data.User.siteUrl;
                    var builder = new DiscordEmbedBuilder
                    {
                        Author = funciones.GetAuthor(nombre, avatar, siteurl),
                        Footer = funciones.GetFooter(ctx),
                        Color = new DiscordColor(78, 63, 96),
                        ImageUrl = data.Data.User.bannerImage
                    };
                    builder.AddField("Estadisticas - Anime", animeStats, true);
                    builder.AddField("Estadisticas - Manga", mangaStats, true);
                    builder.AddField("Opciones", options, true);
                    if (favoriteAnime != "")
                        builder.AddField("Animes favoritos", favoriteAnime, true);
                    if (favoriteManga != "")
                        builder.AddField("Mangas favoritos", favoriteManga, true);
                    if (favoriteCharacters != "")
                        builder.AddField("Personajes favoritos", favoriteCharacters, true);
                    if (favoriteStaff != "")
                        builder.AddField("Staff favoritos", favoriteStaff, true);
                    if (favoriteStudios != "")
                        builder.AddField("Estudios favoritos", favoriteStudios, true);
                    await ctx.RespondAsync(embed: builder).ConfigureAwait(false);
                    await ctx.Message.DeleteAsync("Auto borrado de yumiko").ConfigureAwait(false);
                }
                else
                {
                    foreach (var x in data.Errors)
                    {
                        var msg = await ctx.RespondAsync($"Error: {x.Message}").ConfigureAwait(false);
                        await Task.Delay(3000);
                        await ctx.Message.DeleteAsync("Auto borrado de yumiko");
                        await msg.DeleteAsync("Auto borrado de yumiko");
                    }
                }
            }
            catch(Exception ex)
            {
                DiscordMessage msg = ex.Message switch
                {
                    "The HTTP request failed with status code NotFound" => await ctx.RespondAsync($"No se ha encontrado al usuario de anilist `{usuario}`").ConfigureAwait(false),
                    _ => await ctx.RespondAsync($"Error inesperado").ConfigureAwait(false),
                };
                await Task.Delay(3000);
                await ctx.Message.DeleteAsync("Auto borrado de yumiko");
                await msg.DeleteAsync("Auto borrado de yumiko");
            }
        }

        [Command("anime")]
        public async Task Anime(CommandContext ctx, [RemainingText]string anime)
        {
            var request = new GraphQLRequest
            {
                Query =
                "query($nombre : String){" +
                "   Media(type: ANIME, search: $nombre){" +
                "       title{" +
                "           romaji" +
                "       }," +
                "       coverImage{" +
                "           medium" +
                "       }," +
                "       siteUrl," +
                "       description," +
                "       format," +
                "       episodes" +
                "       status," +
                "       meanScore," +
                "       startDate{" +
                "           year," +
                "           month," +
                "           day" +
                "       }," +
                "       endDate{" +
                "           year," +
                "           month," +
                "           day" +
                "       }," +
                "       genres," +
                "       tags{" +
                "           name," +
                "           isGeneralSpoiler" +
                "       }," +
                "       synonyms," +
                "       studios{" +
                "           nodes{" +
                "               name," +
                "               siteUrl" +
                "           }" +
                "       }," +
                "       externalLinks{" +
                "           site," +
                "           url" +
                "       }," +
                "       isAdult" +
                "   }" +
                "}",
                Variables = new
                {
                    nombre = anime
                }
            };
            try
            {
                var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                if (data.Data != null)
                {
                    var datos = data.Data.Media;
                    if(datos.isAdult == "false")
                    {
                        string estado = datos.status;
                        string formato = datos.format;
                        string score = $"{datos.meanScore}/100";
                        string fechas;
                        string generos = "";
                        foreach (var genero in datos.genres)
                        {
                            generos += genero;
                            generos += ", ";
                        }
                        string tags = "";
                        foreach (var tag in datos.tags)
                        {
                            if (tag.isGeneralSpoiler == "false")
                            {
                                tags += tag.name;
                            }
                            else
                            {
                                tags += $"||{tag.name}||";
                            }
                            tags += ", ";
                        }
                        string titulos = "";
                        foreach (var title in datos.synonyms)
                        {
                            titulos += $"`{title}`, ";
                        }
                        string estudios = "";
                        foreach (var studio in datos.studios.nodes)
                        {
                            estudios += $"[{studio.name}]({studio.siteUrl}), ";
                        }
                        string linksExternos = "";
                        foreach (var external in datos.externalLinks)
                        {
                            linksExternos += $"[{external.site}]({external.url}), ";
                        }
                        if (datos.startDate.day != null)
                        {
                            if (datos.endDate.day != null)
                                fechas = $"{datos.startDate.day}/{datos.startDate.month}/{datos.startDate.year} al {datos.endDate.day}/{datos.endDate.month}/{datos.endDate.year}";
                            else
                                fechas = $"En emisión desde {datos.startDate.day}/{datos.startDate.month}/{datos.startDate.year}";
                        }
                        else
                        {
                            fechas = $"Este anime todavía no tiene fecha de emisión";
                        }  
                        var builder = new DiscordEmbedBuilder
                        {
                            Author = new DiscordEmbedBuilder.EmbedAuthor()
                            {
                                Name = datos.title.romaji,
                                Url = datos.siteUrl
                            },
                            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                            {
                                Url = datos.coverImage.medium
                            },
                            Footer = funciones.GetFooter(ctx),
                            Color = new DiscordColor(78, 63, 96),
                            Description = datos.description
                        };
                        builder.AddField("Formato", formato, true);
                        builder.AddField("Estado", estado.ToLower().ToUpperFirst(), true);
                        builder.AddField("Puntuación", score, true);
                        builder.AddField("Fecha emisión", fechas, false);
                        builder.AddField("Generos", generos, false);
                        builder.AddField("Etiquetas", tags, false);
                        builder.AddField("Titulos alternativos", titulos, false);
                        builder.AddField("Estudios", estudios, false);
                        builder.AddField("Links externos", linksExternos, false);
                        await ctx.RespondAsync(embed: builder).ConfigureAwait(false);
                        await ctx.Message.DeleteAsync("Auto borrado de yumiko").ConfigureAwait(false);
                    }
                    else
                    {
                        DiscordMessage msg = await ctx.RespondAsync("", embed: new DiscordEmbedBuilder
                        {
                            Title = "Requiere NSFW",
                            Description = "Este comando debe ser invocado en un canal NSFW.",
                            Color = new DiscordColor(0xFF0000),
                            Footer = funciones.GetFooter(ctx)
                        });
                        await Task.Delay(3000);
                        await ctx.Message.DeleteAsync("Auto borrado de yumiko");
                        await msg.DeleteAsync("Auto borrado de yumiko");
                    }
                }
                else
                {
                    foreach (var x in data.Errors)
                    {
                        var msg = await ctx.RespondAsync($"Error: {x.Message}").ConfigureAwait(false);
                        await Task.Delay(3000);
                        await ctx.Message.DeleteAsync("Auto borrado de yumiko");
                        await msg.DeleteAsync("Auto borrado de yumiko");
                    }
                }
            }
            catch (Exception ex)
            {
                DiscordMessage msg = ex.Message switch
                {
                    "The HTTP request failed with status code NotFound" => await ctx.RespondAsync($"No se ha encontrado el anime `{anime}`").ConfigureAwait(false),
                    _ => await ctx.RespondAsync($"Error inesperado").ConfigureAwait(false),
                };
                await Task.Delay(3000);
                await ctx.Message.DeleteAsync("Auto borrado de yumiko");
                await msg.DeleteAsync("Auto borrado de yumiko");
            }
        }

        public async Task GetResultados(CommandContext ctx, List<UsuarioJuego> participantes, int rondas)
        {
            string resultados = "";
            participantes.Sort((x, y) => y.Puntaje.CompareTo(x.Puntaje));
            int tot = 0;
            foreach (UsuarioJuego uj in participantes)
            {
                resultados += $"- {uj.Usuario.Username}#{uj.Usuario.Discriminator}: {uj.Puntaje} aciertos\n";
                tot += uj.Puntaje;
            }
            resultados += $"\nTotal ({tot}/{rondas})";
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
            {
                Title = "Resultados",
                Description = resultados,
                Color = new DiscordColor(78, 63, 96)
            }).ConfigureAwait(false);
        } 

        public async Task<SettingsJuego> InicializarJuego(CommandContext ctx, InteractivityExtension interactivity)
        {
            var msgCntRondas = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Elige la cantidad de rondas (máximo 100)",
                Description = "Por ejemplo: 10"
            });
            var msgRondasInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
            if (!msgRondasInter.TimedOut)
            {
                bool result = int.TryParse(msgRondasInter.Result.Content, out int rondas);
                if (result)
                {
                    if(rondas > 0 && rondas <= 100)
                    {
                        var msgDificultad = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                        {
                            Title = "Elije la dificultad",
                            Description = "1- Fácil\n2- Media\n3- Dificil\n4- Extremo"
                        });
                        var msgDificultadInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGames"])));
                        if (!msgDificultadInter.TimedOut)
                        {
                            result = int.TryParse(msgDificultadInter.Result.Content, out int dificultad);
                            if (result)
                            {
                                string dificultadStr;
                                if (dificultad == 1 || dificultad == 2 || dificultad == 3 || dificultad == 4)
                                {
                                    int iterIni;
                                    int iterFin;
                                    switch (dificultad)
                                    {
                                        case 1:
                                            iterIni = 1;
                                            iterFin = 6;
                                            dificultadStr = "Fácil";
                                            break;
                                        case 2:
                                            iterIni = 6;
                                            iterFin = 20;
                                            dificultadStr = "Media";
                                            break;
                                        case 3:
                                            iterIni = 20;
                                            iterFin = 60;
                                            dificultadStr = "Dificil";
                                            break;
                                        case 4:
                                            iterIni = 60;
                                            iterFin = 100;
                                            dificultadStr = "Extremo";
                                            break;
                                        default:
                                            iterIni = 6;
                                            iterFin = 20;
                                            dificultadStr = "Medio";
                                            break;
                                    }
                                    await ctx.Message.DeleteAsync("Auto borrado de Yumiko");
                                    await msgCntRondas.DeleteAsync("Auto borrado de Yumiko");
                                    await msgRondasInter.Result.DeleteAsync("Auto borrado de Yumiko");
                                    await msgDificultad.DeleteAsync("Auto borrado de Yumiko");
                                    await msgDificultadInter.Result.DeleteAsync("Auto borrado de Yumiko");
                                    return new SettingsJuego()
                                    {
                                        Ok = true,
                                        Rondas = rondas,
                                        IterIni = iterIni,
                                        IterFin = iterFin,
                                        Dificultad = dificultadStr
                                    };
                                }
                                else
                                {
                                    return new SettingsJuego()
                                    {
                                        Ok = false,
                                        MsgError = "La dificultad debe ser 1, 2, 3 o 4"
                                    };
                                }
                            }
                            else
                            {
                                return new SettingsJuego()
                                {
                                    Ok = false,
                                    MsgError = "La dificultad debe ser un número (1, 2, 3 o 4)"
                                };
                            }
                        }
                        else
                        {
                            return new SettingsJuego()
                            {
                                Ok = false,
                                MsgError = "Tiempo agotado esperando la dificultad"
                            };
                        }
                    }
                    else
                    {
                        return new SettingsJuego()
                        {
                            Ok = false,
                            MsgError = "La cantidad de rondas debe ser mayor a 0 y menor a 100"
                        };
                    }
                }
                else
                {
                    return new SettingsJuego()
                    {
                        Ok = false,
                        MsgError = "La cantidad de rondas debe ser un numero"
                    };
                }
            }
            else
            {
                return new SettingsJuego()
                {
                    Ok = false,
                    MsgError = "Tiempo agotado esperando la cantidad de rondas"
                };
            }
        }

    }
}
