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

namespace Discord_Bot.Modulos
{
    public class Anilist : BaseCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new FuncionesAuxiliares();
        private readonly GraphQLHttpClient graphQLClient = new GraphQLHttpClient("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        [Command("anilist"), Aliases("user"), Description("Busca un perfil de AniList.")]
        public async Task Profile(CommandContext ctx, [Description("El nick del perfil de AniList")]string usuario)
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
                        Color = funciones.GetColor(),
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

        [Command("anime"), Description("Busco un anime en AniList")]
        public async Task Anime(CommandContext ctx, [RemainingText][Description("Nombre del anime a buscar")] string anime)
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
                "           large" +
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
                "           isMediaSpoiler" +
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
                        string descripcion = datos.description;
                        descripcion = descripcion.Replace("<br>", "");
                        descripcion = descripcion.Replace("<i>", "");
                        descripcion = descripcion.Replace("</i>", "");
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
                        if (generos.Length >= 2)
                            generos = generos.Remove(generos.Length - 2);
                        string tags = "";
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
                        if(tags.Length >= 2)
                            tags = tags.Remove(tags.Length - 2);
                        string titulos = "";
                        foreach (var title in datos.synonyms)
                        {
                            titulos += $"`{title}`, ";
                        }
                        if (titulos.Length >= 2)
                            titulos = titulos.Remove(titulos.Length - 2);
                        string estudios = "";
                        foreach (var studio in datos.studios.nodes) // ERROR ACA, NODES A VECES NO TIENE ELEMENTOS BUSCAR IF PARA VERIFICAR
                        {
                            estudios += $"[{studio.name}]({studio.siteUrl}), ";
                        }
                        if (estudios.Length >= 2)
                            estudios = estudios.Remove(estudios.Length - 2);
                        string linksExternos = "";
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
                            fechas = $"Este anime todavía no tiene fecha de emisión";
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

        [Command("sauce"), Description("Busca el anime de una imagen.")]
        public async Task Sauce(CommandContext ctx, [Description("Link de la imagen")] string url)
        {
            string msg = "OK";
            if (url.Length > 0)
            {
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    string extension = url.Substring(url.Length - 4);
                    if (extension == ".jpg" || extension == ".png" || extension == "jpeg")
                    {
                        var client = new RestClient("https://trace.moe/api/search?url=" + url);
                        var request = new RestRequest(Method.GET);
                        request.AddHeader("content-type", "application/json");
                        var procesando = await ctx.RespondAsync("Procesando imagen..").ConfigureAwait(false);
                        await ctx.Message.DeleteAsync("Auto borrado de yumiko");
                        IRestResponse response = client.Execute(request);
                        await procesando.DeleteAsync("Auto borrado de yumiko");
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.OK:
                                var resp = JsonConvert.DeserializeObject<dynamic>(response.Content);
                                string resultados = "";
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
                DiscordMessage msgError = await ctx.RespondAsync(msg).ConfigureAwait(false);
                await Task.Delay(3000);
                await msgError.DeleteAsync("Auto borrado de yumiko").ConfigureAwait(false);
            }
        }
    }
}
