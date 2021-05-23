using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using System;
using GraphQL.Client.Http;
using GraphQL;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL.Client.Abstractions.Utilities;
using RestSharp;
using System.Net;
using Newtonsoft.Json;
using System.Configuration;
using DSharpPlus.Interactivity.Extensions;

namespace Discord_Bot.Modulos
{
    public class Anilist : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        private readonly GraphQLHttpClient graphQLClient = new GraphQLHttpClient("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        [Command("anilist"), Aliases("user"), Description("Busca un perfil de AniList.")]
        public async Task Profile(CommandContext ctx, [Description("El nick del perfil de AniList")]string usuario = null)
        {
            if (String.IsNullOrEmpty(usuario))
            {
                usuario = await funciones.GetStringInteractivity(ctx, "Escriba un nombre de usuario de AniList", "Ejemplo: Josh", "Tiempo agotado esperando el usuario de AniList");
            }
            if (!String.IsNullOrEmpty(usuario))
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
                        string favoriteAnime = string.Empty;
                        foreach (var anime in data.Data.User.favourites.anime.nodes)
                        {
                            favoriteAnime += $"[{anime.title.romaji}]({anime.siteUrl})\n";
                        }
                        string favoriteManga = string.Empty;
                        foreach (var manga in data.Data.User.favourites.manga.nodes)
                        {
                            favoriteManga += $"[{manga.title.romaji}]({manga.siteUrl})\n";
                        }
                        string favoriteCharacters = string.Empty;
                        foreach (var character in data.Data.User.favourites.characters.nodes)
                        {
                            favoriteCharacters += $"[{character.name.full}]({character.siteUrl})\n";
                        }
                        string favoriteStaff = string.Empty;
                        foreach (var staff in data.Data.User.favourites.staff.nodes)
                        {
                            favoriteStaff += $"[{staff.name.full}]({staff.siteUrl})\n";
                        }
                        string favoriteStudios = string.Empty;
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
                            Color = funciones.GetColor(),
                            ImageUrl = data.Data.User.bannerImage
                        };
                        builder.AddField("Estadisticas - Anime", animeStats, true);
                        builder.AddField("Estadisticas - Manga", mangaStats, true);
                        builder.AddField("Opciones", options, true);
                        if (favoriteAnime != "")
                            builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":tv:")} Animes favoritos", favoriteAnime, true);
                        if (favoriteManga != "")
                            builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":book:")} Mangas favoritos", favoriteManga, true);
                        if (favoriteCharacters != "")
                            builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":bust_in_silhouette:")} Personajes favoritos", favoriteCharacters, true);
                        if (favoriteStaff != "")
                            builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":man_artist:")} Staff favoritos", favoriteStaff, true);
                        if (favoriteStudios != "")
                            builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":minidisc:")} Estudios favoritos", favoriteStudios, true);
                        await ctx.Channel.SendMessageAsync(embed: builder).ConfigureAwait(false);
                    }
                    else
                    {
                        foreach (var x in data.Errors)
                        {
                            var msg = await ctx.Channel.SendMessageAsync($"Error: {x.Message}").ConfigureAwait(false);
                            await Task.Delay(3000);
                            await funciones.BorrarMensaje(ctx, msg.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DiscordMessage msg = ex.Message switch
                    {
                        "The HTTP request failed with status code NotFound" => await ctx.Channel.SendMessageAsync($"No se ha encontrado al usuario de anilist `{usuario}`").ConfigureAwait(false),
                        _ => await ctx.Channel.SendMessageAsync($"Error inesperado").ConfigureAwait(false),
                    };
                    await Task.Delay(3000);
                    await funciones.BorrarMensaje(ctx, msg.Id);
                }
            }
        }

        [Command("anime"), Description("Busco un anime en AniList")]
        public async Task Anime(CommandContext ctx, [RemainingText][Description("Nombre del anime a buscar")] string anime = null)
        {
            if (String.IsNullOrEmpty(anime))
            {
                anime = await funciones.GetStringInteractivity(ctx, "Escriba el nombre del anime", "Ejemplo: Grisaia no Kajitsu", "Tiempo agotado esperando el nombre del anime");
            }
            if (!String.IsNullOrEmpty(anime))
            {
                var request = new GraphQLRequest
                {
                    Query =
                "query($nombre : String){" +
                "   Page(perPage:10){" +
                "       media(type: ANIME, search: $nombre){" +
                "           title{" +
                "               romaji" +
                "           }," +
                "           coverImage{" +
                "               large" +
                "           }," +
                "           siteUrl," +
                "           description," +
                "           format," +
                "           episodes" +
                "           status," +
                "           meanScore," +
                "           startDate{" +
                "               year," +
                "               month," +
                "               day" +
                "           }," +
                "           endDate{" +
                "               year," +
                "               month," +
                "               day" +
                "           }," +
                "           genres," +
                "           tags{" +
                "               name," +
                "               isMediaSpoiler" +
                "           }," +
                "           synonyms," +
                "           studios{" +
                "               nodes{" +
                "                   name," +
                "                   siteUrl" +
                "               }" +
                "           }," +
                "           externalLinks{" +
                "               site," +
                "               url" +
                "           }," +
                "           isAdult" +
                "       }" +
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
                        int cont = 0;
                        string opc = string.Empty;
                        foreach (var animeP in data.Data.Page.media)
                        {
                            cont++;
                            opc += $"{cont} - {animeP.title.romaji}\n";
                        }
                        var elegido = await funciones.GetElegido(ctx, opc, cont);
                        if (elegido > 0)
                        {
                            var datos = data.Data.Page.media[elegido - 1];
                            if ((datos.isAdult == "false") || (datos.isAdult == true && ctx.Channel.IsNSFW))
                            {
                                string descripcion = datos.description;
                                descripcion = funciones.NormalizarDescription(funciones.LimpiarTexto(descripcion));
                                if (descripcion == "")
                                    descripcion = "(Sin descripción)";
                                string estado = datos.status;
                                string episodios = datos.episodes;
                                string formato = datos.format;
                                string score = $"{datos.meanScore}/100";
                                string fechas;
                                string generos = string.Empty;
                                foreach (var genero in datos.genres)
                                {
                                    generos += genero;
                                    generos += ", ";
                                }
                                if (generos.Length >= 2)
                                    generos = generos.Remove(generos.Length - 2);
                                string tags = string.Empty;
                                foreach (var tag in datos.tags)
                                {
                                    if (tag.isMediaSpoiler == "false")
                                    {
                                        tags += tag.name;
                                    }
                                    else
                                    {
                                        tags += $"||{tag.name}||";
                                    }
                                    tags += ", ";
                                }
                                if (tags.Length >= 2)
                                    tags = tags.Remove(tags.Length - 2);
                                string titulos = string.Empty;
                                foreach (var title in datos.synonyms)
                                {
                                    titulos += $"`{title}`, ";
                                }
                                if (titulos.Length >= 2)
                                    titulos = titulos.Remove(titulos.Length - 2);
                                string estudios = string.Empty;
                                var nodos = datos.studios.nodes;
                                if (nodos.HasValues)
                                {
                                    foreach (var studio in datos.studios.nodes)
                                    {
                                        estudios += $"[{studio.name}]({studio.siteUrl}), ";
                                    }
                                }
                                if (estudios.Length >= 2)
                                    estudios = estudios.Remove(estudios.Length - 2);
                                string linksExternos = string.Empty;
                                foreach (var external in datos.externalLinks)
                                {
                                    linksExternos += $"[{external.site}]({external.url}), ";
                                }
                                if (linksExternos.Length >= 2)
                                    linksExternos = linksExternos.Remove(linksExternos.Length - 2);
                                if (datos.startDate.day != null)
                                {
                                    if (datos.endDate.day != null)
                                        fechas = $"{datos.startDate.day}/{datos.startDate.month}/{datos.startDate.year} al {datos.endDate.day}/{datos.endDate.month}/{datos.endDate.year}";
                                    else
                                        fechas = $"En emisión desde {datos.startDate.day}/{datos.startDate.month}/{datos.startDate.year}";
                                }
                                else
                                {
                                    fechas = $"Este anime no tiene fecha de emisión";
                                }
                                string titulo = datos.title.romaji;
                                string urlAnilist = datos.siteUrl;
                                var builder = new DiscordEmbedBuilder
                                {
                                    Title = titulo,
                                    Url = urlAnilist,
                                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                                    {
                                        Url = datos.coverImage.large
                                    },
                                    Footer = funciones.GetFooter(ctx),
                                    Color = funciones.GetColor(),
                                    Description = descripcion
                                };
                                if (episodios.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":1234:")} Episodios", funciones.NormalizarField(episodios), true);
                                if (formato.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":dividers:")} Formato", funciones.NormalizarField(formato), true);
                                if (estado.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":hourglass_flowing_sand:")} Estado", funciones.NormalizarField(estado.ToLower().ToUpperFirst()), true);
                                if (score.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":star:")} Puntuación", funciones.NormalizarField(score), false);
                                if (fechas.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":calendar_spiral:")} Fecha emisión", funciones.NormalizarField(fechas), false);
                                if (generos.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":scroll:")} Generos", funciones.NormalizarField(generos), false);
                                if (tags.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":notepad_spiral:")} Etiquetas", funciones.NormalizarField(tags), false);
                                if (titulos.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":pencil:")} Titulos alternativos", funciones.NormalizarField(titulos), false);
                                if (estudios.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":minidisc:")} Estudios", funciones.NormalizarField(estudios), false);
                                if (linksExternos.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":link:")} Links externos", funciones.NormalizarField(linksExternos), false);
                                await ctx.Channel.SendMessageAsync(embed: builder).ConfigureAwait(false);
                            }
                            else
                            {
                                DiscordMessage msg = await ctx.Channel.SendMessageAsync("", embed: new DiscordEmbedBuilder
                                {
                                    Title = "Requiere NSFW",
                                    Description = "Este comando debe ser invocado en un canal NSFW.",
                                    Color = new DiscordColor(0xFF0000),
                                    Footer = funciones.GetFooter(ctx)
                                });
                                await Task.Delay(3000);
                                await funciones.BorrarMensaje(ctx, msg.Id);
                            }
                        }
                    }
                    else
                    {
                        foreach (var x in data.Errors)
                        {
                            var msg = await ctx.Channel.SendMessageAsync($"Error: {x.Message}").ConfigureAwait(false);
                            await Task.Delay(3000);
                            await funciones.BorrarMensaje(ctx, msg.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DiscordMessage msg = ex.Message switch
                    {
                        "The HTTP request failed with status code NotFound" => await ctx.Channel.SendMessageAsync($"No se ha encontrado el anime `{anime}`").ConfigureAwait(false),
                        _ => await ctx.Channel.SendMessageAsync($"Error inesperado").ConfigureAwait(false),
                    };
                    await Task.Delay(3000);
                    await funciones.BorrarMensaje(ctx, msg.Id);
                }
            }
        }

        [Command("manga"), Description("Busco un manga en AniList")]
        public async Task Manga(CommandContext ctx, [RemainingText][Description("Nombre del manga a buscar")] string manga = null)
        {
            if (String.IsNullOrEmpty(manga))
            {
                manga = await funciones.GetStringInteractivity(ctx, "Escriba el nombre del manga", "Ejemplo: Berserk", "Tiempo agotado esperando el nombre del manga");
            }
            if (!String.IsNullOrEmpty(manga))
            {
                var request = new GraphQLRequest
                {
                    Query =
                "query($nombre : String){" +
                "   Page(perPage:10){" +
                "       media(type: MANGA, search: $nombre){" +
                "           title{" +
                "               romaji" +
                "           }," +
                "           coverImage{" +
                "               large" +
                "           }," +
                "           siteUrl," +
                "           description," +
                "           format," +
                "           chapters" +
                "           status," +
                "           meanScore," +
                "           startDate{" +
                "               year," +
                "               month," +
                "               day" +
                "           }," +
                "           endDate{" +
                "               year," +
                "               month," +
                "               day" +
                "           }," +
                "           genres," +
                "           tags{" +
                "               name," +
                "               isMediaSpoiler" +
                "           }," +
                "           synonyms," +
                "           isAdult" +
                "       }" +
                "   }" +
                "}",
                    Variables = new
                    {
                        nombre = manga
                    }
                };
                try
                {
                    var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                    if (data.Data != null)
                    {
                        int cont = 0;
                        string opc = string.Empty;
                        foreach (var animeP in data.Data.Page.media)
                        {
                            cont++;
                            opc += $"{cont} - {animeP.title.romaji}\n";
                        }
                        var elegido = await funciones.GetElegido(ctx, opc, cont);
                        if (elegido > 0)
                        {
                            var datos = data.Data.Page.media[elegido - 1];
                            if ((datos.isAdult == "false") || (datos.isAdult == true && ctx.Channel.IsNSFW))
                            {
                                string descripcion = datos.description;
                                descripcion = funciones.NormalizarDescription(funciones.LimpiarTexto(descripcion));
                                if (descripcion == "")
                                    descripcion = "(Sin descripción)";
                                string estado = datos.status;
                                string formato = datos.format;
                                string score = $"{datos.meanScore}/100";
                                string fechas;
                                string generos = string.Empty;
                                foreach (var genero in datos.genres)
                                {
                                    generos += genero;
                                    generos += ", ";
                                }
                                if (generos.Length >= 2)
                                    generos = generos.Remove(generos.Length - 2);
                                string tags = string.Empty;
                                foreach (var tag in datos.tags)
                                {
                                    if (tag.isMediaSpoiler == "false")
                                    {
                                        tags += tag.name;
                                    }
                                    else
                                    {
                                        tags += $"||{tag.name}||";
                                    }
                                    tags += ", ";
                                }
                                if (tags.Length >= 2)
                                    tags = tags.Remove(tags.Length - 2);
                                string titulos = string.Empty;
                                foreach (var title in datos.synonyms)
                                {
                                    titulos += $"`{title}`, ";
                                }
                                if (titulos.Length >= 2)
                                    titulos = titulos.Remove(titulos.Length - 2);
                                if (datos.startDate.day != null)
                                {
                                    if (datos.endDate.day != null)
                                        fechas = $"{datos.startDate.day}/{datos.startDate.month}/{datos.startDate.year} al {datos.endDate.day}/{datos.endDate.month}/{datos.endDate.year}";
                                    else
                                        fechas = $"En emisión desde {datos.startDate.day}/{datos.startDate.month}/{datos.startDate.year}";
                                }
                                else
                                {
                                    fechas = $"Este manga no tiene fecha de emisión";
                                }
                                string titulo = datos.title.romaji;
                                string urlAnilist = datos.siteUrl;
                                var builder = new DiscordEmbedBuilder
                                {
                                    Title = titulo,
                                    Url = urlAnilist,
                                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                                    {
                                        Url = datos.coverImage.large
                                    },
                                    Footer = funciones.GetFooter(ctx),
                                    Color = funciones.GetColor(),
                                    Description = descripcion
                                };
                                if (formato.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":dividers:")} Formato", funciones.NormalizarField(formato), true);
                                if (estado.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":hourglass_flowing_sand:")} Estado", funciones.NormalizarField(estado.ToLower().ToUpperFirst()), true);
                                if (score.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":star:")} Puntuación", funciones.NormalizarField(score), true);
                                if (fechas.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":calendar_spiral:")} Fecha de publicación", funciones.NormalizarField(fechas), false);
                                if (generos.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":scroll:")} Generos", funciones.NormalizarField(generos), false);
                                if (tags.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":notepad_spiral:")} Etiquetas", funciones.NormalizarField(tags), false);
                                if (titulos.Length > 0)
                                    builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":pencil:")} Titulos alternativos", funciones.NormalizarField(titulos), false);
                                await ctx.Channel.SendMessageAsync(embed: builder).ConfigureAwait(false);
                            }
                            else
                            {
                                DiscordMessage msg = await ctx.Channel.SendMessageAsync("", embed: new DiscordEmbedBuilder
                                {
                                    Title = "Requiere NSFW",
                                    Description = "Este comando debe ser invocado en un canal NSFW.",
                                    Color = new DiscordColor(0xFF0000),
                                    Footer = funciones.GetFooter(ctx)
                                });
                                await Task.Delay(3000);
                                await funciones.BorrarMensaje(ctx, msg.Id);
                            }
                        }
                    }
                    else
                    {
                        foreach (var x in data.Errors)
                        {
                            var msg = await ctx.Channel.SendMessageAsync($"Error: {x.Message}").ConfigureAwait(false);
                            await Task.Delay(3000);
                            await funciones.BorrarMensaje(ctx, msg.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DiscordMessage msg = ex.Message switch
                    {
                        "The HTTP request failed with status code NotFound" => await ctx.Channel.SendMessageAsync($"No se ha encontrado el anime `{manga}`").ConfigureAwait(false),
                        _ => await ctx.Channel.SendMessageAsync($"Error inesperado").ConfigureAwait(false),
                    };
                    await Task.Delay(3000);
                    await funciones.BorrarMensaje(ctx, msg.Id);
                }
            }
        }

        [Command("character"), Aliases("personaje"), Description("Busco un personaje en AniList")]
        public async Task Character(CommandContext ctx, [RemainingText][Description("Nombre del personaje a buscar")] string personaje = null)
        {
            if (String.IsNullOrEmpty(personaje))
            {
                personaje = await funciones.GetStringInteractivity(ctx, "Escriba el nombre del personaje", "Ejemplo: Yumiko Sakaki", "Tiempo agotado esperando el nombre");
            }
            if (!String.IsNullOrEmpty(personaje))
            {
                var request = new GraphQLRequest
                {
                    Query =
                "query($nombre : String){" +
                "   Page(perPage:10){" +
                "       characters(search: $nombre){" +
                "           name{" +
                "               full" +
                "           }," +
                "           image{" +
                "               large" +
                "           }," +
                "           siteUrl," +
                "           description," +
                "           animes: media(type: ANIME){" +
                "               nodes{" +
                "                   title{" +
                "                       romaji" +
                "                   }," +
                "                   siteUrl" +
                "               }" +
                "           }" +
                "           mangas: media(type: MANGA){" +
                "               nodes{" +
                "                   title{" +
                "                       romaji" +
                "                   }," +
                "                   siteUrl" +
                "               }" +
                "           }" +
                "       }" +
                "   }" +
                "}",
                    Variables = new
                    {
                        nombre = personaje
                    }
                };
                try
                {
                    var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                    if (data.Data != null)
                    {
                        int cont = 0;
                        string opc = string.Empty;
                        foreach (var animeP in data.Data.Page.characters)
                        {
                            cont++;
                            opc += $"{cont} - {animeP.name.full}\n";
                        }
                        var elegido = await funciones.GetElegido(ctx, opc, cont);
                        if (elegido > 0)
                        {
                            var datos = data.Data.Page.characters[elegido - 1];
                            string descripcion = datos.description;
                            descripcion = funciones.NormalizarDescription(funciones.LimpiarTexto(descripcion));
                            if (descripcion == "")
                                descripcion = "(Sin descripción)";
                            string nombre = datos.name.full;
                            string imagen = datos.image.large;
                            string urlAnilist = datos.siteUrl;
                            string animes = string.Empty;
                            foreach (var anime in datos.animes.nodes)
                            {
                                animes += $"[{anime.title.romaji}]({anime.siteUrl})\n";
                            }
                            string mangas = string.Empty;
                            foreach (var manga in datos.mangas.nodes)
                            {
                                mangas += $"[{manga.title.romaji}]({manga.siteUrl})\n";
                            }
                            var builder = new DiscordEmbedBuilder
                            {
                                Title = nombre,
                                Url = urlAnilist,
                                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                                {
                                    Url = imagen
                                },
                                Footer = funciones.GetFooter(ctx),
                                Color = funciones.GetColor(),
                                Description = descripcion
                            };
                            if (animes.Length > 0)
                                builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":tv:")} Animes", funciones.NormalizarField(animes), false);
                            if (mangas.Length > 0)
                                builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":book:")} Mangas", funciones.NormalizarField(mangas), false);
                            await ctx.Channel.SendMessageAsync(embed: builder).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        foreach (var x in data.Errors)
                        {
                            var msg = await ctx.Channel.SendMessageAsync($"Error: {x.Message}").ConfigureAwait(false);
                            await Task.Delay(3000);
                            await funciones.BorrarMensaje(ctx, msg.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DiscordMessage msg = ex.Message switch
                    {
                        "The HTTP request failed with status code NotFound" => await ctx.Channel.SendMessageAsync($"No se ha encontrado el personaje `{personaje}`").ConfigureAwait(false),
                        _ => await ctx.Channel.SendMessageAsync($"Error inesperado, mensaje: [{ex.Message}").ConfigureAwait(false),
                    };
                    await Task.Delay(5000);
                    await funciones.BorrarMensaje(ctx, msg.Id);
                }
            }
        }
        
        // Staff, algun dia

        [Command("sauce"), Description("Busca el anime de una imagen.")]
        public async Task Sauce(CommandContext ctx, [Description("Link de la imagen")] string url = null)
        {
            if (String.IsNullOrEmpty(url))
            {
                url = await funciones.GetStringInteractivity(ctx, "Escriba un link de una imagen", "La imagen debe ser JPG, PNG o JPEG", "Tiempo agotado esperando el link de la imagen");
            }
            if (!String.IsNullOrEmpty(url))
            {
                string msg = "OK";
                if (url.Length > 0)
                {
                    if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    {
                        string extension = url[^4..];
                        if (extension == ".jpg" || extension == ".png" || extension == "jpeg")
                        {
                            var client = new RestClient("https://trace.moe/api/search?url=" + url);
                            var request = new RestRequest(Method.GET);
                            request.AddHeader("content-type", "application/json");
                            var procesando = await ctx.Channel.SendMessageAsync("Procesando imagen..").ConfigureAwait(false);
                            IRestResponse response = client.Execute(request);
                            await funciones.BorrarMensaje(ctx, procesando.Id);
                            switch (response.StatusCode)
                            {
                                case HttpStatusCode.OK:
                                    var resp = JsonConvert.DeserializeObject<dynamic>(response.Content);
                                    string resultados = string.Empty;
                                    string titulo = "El posible anime de la imagen es:";
                                    bool encontro = false;
                                    foreach (var resultado in resp.docs)
                                    {
                                        string enlace = "https://anilist.co/anime/";
                                        int similaridad = resultado.similarity * 100;
                                        if (similaridad >= 87)
                                        {
                                            encontro = true;
                                            int segundo = resultado.at;
                                            TimeSpan time = TimeSpan.FromSeconds(segundo);
                                            string at = time.ToString(@"mm\:ss");
                                            resultados =
                                                $"Nombre:    [{resultado.title_romaji}]({enlace += resultado.anilist_id})\n" +
                                                $"Similitud: {similaridad}%\n" +
                                                $"Episodio:  {resultado.episode} (Minuto: {at})\n";
                                            break;
                                        }
                                    }
                                    if (!encontro)
                                    {
                                        titulo = "No se han encontrado resultados para esta imagen";
                                        resultados = "Recuerda que solamente funciona con imágenes que sean partes de un episodio";
                                    }
                                    var embed = new DiscordEmbedBuilder
                                    {
                                        Footer = funciones.GetFooter(ctx),
                                        Color = funciones.GetColor(),
                                        Title = "Sauce (Trace.moe)",
                                        ImageUrl = url
                                    };
                                    embed.AddField(titulo, resultados);
                                    await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
                                    break;
                                case HttpStatusCode.BadRequest:
                                    msg = "Debes ingresar un link";
                                    break;
                                case HttpStatusCode.Forbidden:
                                    msg = "Acceso denegado";
                                    break;
                                case HttpStatusCode.TooManyRequests:
                                    msg = "Ratelimit excedido";
                                    break;
                                case HttpStatusCode.InternalServerError:
                                case HttpStatusCode.ServiceUnavailable:
                                    msg = "Error interno en el servidor de Trace.moe";
                                    break;
                                default:
                                    msg = "Error inesperado";
                                    break;
                            }
                        }
                        else
                        {
                            msg = "La imagen debe ser JPG, PNG o JPEG";
                        }
                    }
                    else
                    {
                        msg = "Debes ingresar el link de una imagen";
                    }
                }
                if (msg != "OK")
                {
                    DiscordMessage msgError = await ctx.Channel.SendMessageAsync(msg).ConfigureAwait(false);
                    await Task.Delay(3000);
                    await funciones.BorrarMensaje(ctx, msgError.Id);
                }
            }
        }

        [Command("pj"), Description("Personaje aleatorio.")]
        public async Task Pj(CommandContext ctx)
        {
            int pag = funciones.GetNumeroRandom(1, 5000);
            Character personaje = await funciones.GetRandomCharacter(ctx, pag);
            if(personaje != null)
            {
                DiscordEmoji corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                var msg = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = personaje.NameFull,
                    Url = personaje.SiteUrl,
                    ImageUrl = personaje.Image,
                    Description = $"[{personaje.AnimePrincipal.TitleRomaji}]({personaje.AnimePrincipal.SiteUrl})\n{personaje.Favoritos} {corazon} (nº {pag} en popularidad)",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor()
                }).ConfigureAwait(false);
                await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsup:")).ConfigureAwait(false);
                await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsdown:")).ConfigureAwait(false);
                await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":question:")).ConfigureAwait(false);
            }
        }

        [Command("recomendacion"), Description("Personaje aleatorio."), Aliases("recomendaciones", "recommendation", "recommendations"), Hidden]
        public async Task Recommendation(CommandContext ctx, string anime = null)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var msgAnime = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Escribe el nombre del anime",
                Description = "Ejemplo: Grisaia no Kajitsu",
                Footer = funciones.GetFooter(ctx),
                Color = funciones.GetColor(),
            });
            var msgAnimeInter = await interactivity.WaitForMessageAsync(xm => xm.Channel == ctx.Channel && xm.Author == ctx.User, TimeSpan.FromSeconds(Convert.ToDouble(ConfigurationManager.AppSettings["TimeoutGeneral"])));
            if (!msgAnimeInter.TimedOut)
            {
                anime = msgAnimeInter.Result.Content;
                if (msgAnime != null)
                    await funciones.BorrarMensaje(ctx, msgAnime.Id);
                if (msgAnimeInter.Result != null)
                    await funciones.BorrarMensaje(ctx, msgAnimeInter.Result.Id);
            }
            else
            {
                var msgError = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Error",
                    Description = "Tiempo agotado esperando el usuario de AniList",
                    Footer = funciones.GetFooter(ctx),
                    Color = DiscordColor.Red,
                });
                await Task.Delay(3000);
                if (msgError != null)
                    await funciones.BorrarMensaje(ctx, msgError.Id);
                if (msgAnime != null)
                    await funciones.BorrarMensaje(ctx, msgAnime.Id);
                return;
            }
            var request = new GraphQLRequest
            {
                Query =
                "query($nombre : String){" +
                "   Page(perPage:10){" +
                "       media(type: ANIME, search: $nombre){" +
                "           id," +
                "           title{" +
                "               romaji" +
                "           }," +
                "           siteUrl," +
                "           isAdult" +
                "       }" +
                "   }" +
                "}",
                Variables = new
                {
                    nombre = anime
                }
            };
        }
    }
}
