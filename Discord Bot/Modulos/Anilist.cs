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
            var msgCntRondas = await ctx.RespondAsync(embed : new DiscordEmbedBuilder { 
                Title = "Elige la cantidad de rondas",
                Description = "Por ejemplo: 10"
            });
            var msgRondasInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(10));
            if (!msgRondasInter.TimedOut)
            {
                bool result = int.TryParse(msgRondasInter.Result.Content, out int rondas);
                if (result)
                {
                    string dificultadStr;
                    var msgDificultad = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Elije la dificultad",
                        Description = "1- Fácil (300 personajes)\n2- Media (1000 personajes)\n3- Dificil (3000 personajes)"
                    });
                    var msgDificultadInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(10));
                    if (!msgDificultadInter.TimedOut)
                    {
                        result = int.TryParse(msgDificultadInter.Result.Content, out int dificultad);
                        if (result)
                        {
                            if (dificultad == 1 || dificultad == 2 || dificultad == 3)
                            {
                                int iteraciones;
                                switch (dificultad)
                                {
                                    case 1:
                                        iteraciones = 6;
                                        dificultadStr = "Fácil";
                                        break;
                                    case 2:
                                        iteraciones = 20;
                                        dificultadStr = "Media";
                                        break;
                                    case 3:
                                        iteraciones = 60;
                                        dificultadStr = "Dificil";
                                        break;
                                    default:
                                        iteraciones = 20;
                                        dificultadStr = "Medio";
                                        break;
                                }
                                await ctx.Message.DeleteAsync("Auto borrado de Yumiko");
                                await msgCntRondas.DeleteAsync("Auto borrado de Yumiko");
                                await msgRondasInter.Result.DeleteAsync("Auto borrado de Yumiko");
                                await msgDificultad.DeleteAsync("Auto borrado de Yumiko");
                                await msgDificultadInter.Result.DeleteAsync("Auto borrado de Yumiko");
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
                                for (int i = 1; i <= iteraciones; i++)
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
                                        DiscordMessage msg;
                                        switch (ex.Message)
                                        {
                                            default:
                                                msg = await ctx.RespondAsync($"Error inesperado").ConfigureAwait(false);
                                                break;
                                        }
                                        await Task.Delay(3000);
                                        await ctx.Message.DeleteAsync("Auto borrado de yumiko");
                                        await msg.DeleteAsync("Auto borrado de yumiko");
                                        return;
                                    };
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
                                        , TimeSpan.FromSeconds(20));
                                    if (!msg.TimedOut)
                                    {
                                        if(msg.Result.Author == ctx.User && msg.Result.Content.ToLower() == "cancelar")
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
                                var error = await ctx.RespondAsync("La dificultad debe ser 1, 2 o 3").ConfigureAwait(false);
                                // Msg de pusiste el numero mal (ingresar dificultad out of range) borrar msg anteriores
                            }
                        }
                        else
                        {
                            var error = await ctx.RespondAsync("La dificultad debe ser un número (1, 2 o 3)").ConfigureAwait(false);
                            // Msg de pusiste el numero mal (ingresar dificultad) borrar msg anteriores
                        }
                    }
                    else
                    {
                        var error = await ctx.RespondAsync("Tiempo agotado esperando la dificultad").ConfigureAwait(false);
                        // Msg de timeout (ingresar dificultad) borrar msg anteriores
                    }
                }
                else
                {
                    var error = await ctx.RespondAsync("La cantidad de rondas debe ser un numero").ConfigureAwait(false);
                    // Msg de pusiste el numero mal (ingresar rondas) borrar msg anteriores
                }
            }
            else
            {
                var error = await ctx.RespondAsync("Tiempo agotado esperando la cantidad de rondas").ConfigureAwait(false);
                // Msg de timeout (ingresar rondas) borrar msg anteriores
            }
        }

        [Command("quizA"), Aliases("adivinaelanime")]
        public async Task QuizAnimeGlobal(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var msgCntRondas = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Elige la cantidad de rondas",
                Description = "Por ejemplo: 10"
            });
            var msgRondasInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(10));
            if (!msgRondasInter.TimedOut)
            {
                bool result = int.TryParse(msgRondasInter.Result.Content, out int rondas);
                if (result)
                {
                    var msgDificultad = await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Elije la dificultad",
                        Description = "1- Fácil (300 animes)\n2- Media (1000 animes)\n3- Dificil (3000 animes)"
                    });
                    var msgDificultadInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(10));
                    if (!msgDificultadInter.TimedOut)
                    {
                        result = int.TryParse(msgDificultadInter.Result.Content, out int dificultad);
                        if (result)
                        {
                            string dificultadStr;
                            if (dificultad == 1 || dificultad == 2 || dificultad == 3)
                            {
                                int iteraciones;
                                switch (dificultad)
                                {
                                    case 1:
                                        iteraciones = 6;
                                        dificultadStr = "Fácil";
                                        break;
                                    case 2:
                                        iteraciones = 20;
                                        dificultadStr = "Media";
                                        break;
                                    case 3:
                                        iteraciones = 60;
                                        dificultadStr = "Dificil";
                                        break;
                                    default:
                                        iteraciones = 20;
                                        dificultadStr = "Medio";
                                        break;
                                }
                                await ctx.Message.DeleteAsync("Auto borrado de Yumiko");
                                await msgCntRondas.DeleteAsync("Auto borrado de Yumiko");
                                await msgRondasInter.Result.DeleteAsync("Auto borrado de Yumiko");
                                await msgDificultad.DeleteAsync("Auto borrado de Yumiko");
                                await msgDificultadInter.Result.DeleteAsync("Auto borrado de Yumiko");
                                DiscordEmbed embebido = new DiscordEmbedBuilder
                                {
                                    Title = "Adivina el anime",
                                    Description = $"Sesión iniciada por {ctx.User.Mention}",
                                    Color = new DiscordColor(78, 63, 96)
                                }.AddField("Rondas", $"{rondas}").AddField("Dificultad", $"{dificultadStr}");
                                await ctx.RespondAsync(embed: embebido).ConfigureAwait(false);
                                List<Anime> animeList = new List<Anime>();
                                Random rnd = new Random();
                                List<UsuarioJuego> participantes = new List<UsuarioJuego>();
                                DiscordMessage mensaje = await ctx.RespondAsync($"Obteniendo animes...").ConfigureAwait(false);
                                for (int i = 1; i <= iteraciones; i++)
                                {
                                    var request = new GraphQLRequest
                                    {
                                        Query =
                                        "query($pagina : Int){" +
                                        "   Page(page: $pagina){" +
                                        "       media(sort: POPULARITY_DESC, type: ANIME){" +
                                        "           siteUrl," +
                                        "           title{" +
                                        "               romaji," +
                                        "               english," +
                                        "           }," +
                                        "           coverImage{" +
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
                                        foreach (var x in data.Data.Page.media)
                                        {
                                            animeList.Add(new Anime()
                                            {
                                                Image = x.coverImage.large,
                                                TitleEnglish = x.title.romaji,
                                                TitleRomaji = x.title.english,
                                                SiteUrl = x.siteUrl
                                            });
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        DiscordMessage msg;
                                        switch (ex.Message)
                                        {
                                            default:
                                                msg = await ctx.RespondAsync($"Error inesperado").ConfigureAwait(false);
                                                break;
                                        }
                                        await Task.Delay(3000);
                                        await ctx.Message.DeleteAsync("Auto borrado de yumiko");
                                        await msg.DeleteAsync("Auto borrado de yumiko");
                                        return;
                                    };
                                }
                                await mensaje.DeleteAsync("Auto borrado de Yumiko");
                                int lastRonda;
                                for (int ronda = 1; ronda <= rondas; ronda++)
                                {
                                    lastRonda = ronda;
                                    int random = funciones.GetNumeroRandom(0, animeList.Count - 1);
                                    Anime elegido = animeList[random];
                                    await ctx.RespondAsync(embed: new DiscordEmbedBuilder
                                    {
                                        Color = DiscordColor.Gold,
                                        Title = "Adivina el anime",
                                        Description = $"Ronda {ronda} de {rondas}",
                                        ImageUrl = elegido.Image
                                    }).ConfigureAwait(false);
                                    var msg = await interactivity.WaitForMessageAsync
                                        (xm => (xm.Channel == ctx.Channel) &&
                                        ((elegido.TitleEnglish != null && elegido.TitleEnglish.ToLower().Trim() == xm.Content.ToLower().Trim()) || elegido.TitleRomaji.ToLower().Trim() == xm.Content.ToLower().Trim()) || (xm.Content.ToLower() == "cancelar" && xm.Author == ctx.User)
                                        , TimeSpan.FromSeconds(20));
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
                                            Description = $"El nombre es: [{elegido.TitleRomaji}]({elegido.SiteUrl})",
                                            Color = DiscordColor.Green
                                        }).ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                                            Title = "¡Nadie ha acertado!",
                                            Description = $"El nombre era: [{elegido.TitleRomaji}]({elegido.SiteUrl})",
                                            Color = DiscordColor.Red
                                        }).ConfigureAwait(false);
                                    }
                                }
                                await GetResultados(ctx, participantes, rondas);
                            }
                            else
                            {
                                var error = await ctx.RespondAsync("La dificultad debe ser 1, 2 o 3").ConfigureAwait(false);
                                // Msg de pusiste el numero mal (ingresar dificultad out of range) borrar msg anteriores
                            }
                        }
                        else
                        {
                            var error = await ctx.RespondAsync("La dificultad debe ser un número (1, 2 o 3)").ConfigureAwait(false);
                            // Msg de pusiste el numero mal (ingresar dificultad) borrar msg anteriores
                        }
                    }
                    else
                    {
                        var error = await ctx.RespondAsync("Tiempo agotado esperando la dificultad").ConfigureAwait(false);
                        // Msg de timeout (ingresar dificultad) borrar msg anteriores
                    }
                }
                else
                {
                    var error = await ctx.RespondAsync("La cantidad de rondas debe ser un numero").ConfigureAwait(false);
                    // Msg de pusiste el numero mal (ingresar rondas) borrar msg anteriores
                }
            }
            else
            {
                var error = await ctx.RespondAsync("Tiempo agotado esperando la cantidad de rondas").ConfigureAwait(false);
                // Msg de timeout (ingresar rondas) borrar msg anteriores
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
                        Footer = funciones.GetFooter(ctx, "anilist"),
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
                DiscordMessage msg;
                switch (ex.Message)
                {
                    case "The HTTP request failed with status code NotFound":
                        msg = await ctx.RespondAsync($"No se ha encontrado al usuario de anilist `{usuario}`").ConfigureAwait(false);
                        break;
                    default:
                        msg = await ctx.RespondAsync($"Error inesperado").ConfigureAwait(false);
                        break;
                }
                await Task.Delay(3000);
                await ctx.Message.DeleteAsync("Auto borrado de yumiko");
                await msg.DeleteAsync("Auto borrado de yumiko");
            };
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
    }
}
