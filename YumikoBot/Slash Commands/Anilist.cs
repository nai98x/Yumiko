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
using DSharpPlus.Interactivity.Extensions;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace Discord_Bot.Modulos
{
    public class AnilistSlashCommands : ApplicationCommandModule
    {
        private readonly FuncionesAuxiliares funciones = new();
        private readonly FuncionesAnilist funcionesAnilist = new();
        private readonly GraphQLHttpClient graphQLClient = new("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        [SlashCommand("anilist", "Busca un perfil de AniList")]
        public async Task Profile(InteractionContext ctx, [Option("Usuario", "Nick de AniList del usuario a buscar")] string usuario)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = funciones.GetContext(ctx);
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
                        Author = new DiscordEmbedBuilder.EmbedAuthor {
                            IconUrl = avatar,
                            Name = nombre,
                            Url = siteurl
                        },
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
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                }
                else
                {
                    foreach (var x in data.Errors)
                    {
                        var msg = await ctx.Channel.SendMessageAsync($"Error: {x.Message}").ConfigureAwait(false);
                        await Task.Delay(3000);
                        await funciones.BorrarMensaje(context, msg.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                string mensaje = ex.Message switch
                {
                    "The HTTP request failed with status code NotFound" => $"No se ha encontrado el usuario de AniList `{usuario}`",
                    _ => $"Error inesperado, mensaje: [{ex.Message}"
                };
                DiscordMessage msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(mensaje));
                await Task.Delay(5000);
                await funciones.BorrarMensaje(context, msg.Id);
            }
        }

        [SlashCommand("anime", "Busca un anime en AniList")]
        public async Task Anime(InteractionContext ctx, [Option("Anime", "Nombre del anime a buscar")] string anime)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = funciones.GetContext(ctx);
            var media = await funcionesAnilist.GetAniListMedia(ctx, anime, "anime");
            if (media.Ok == true)
            {
                string titulos = string.Empty;
                foreach (var title in media.Titulos)
                {
                    titulos += $"`{title}`, ";
                }
                if (titulos.Length >= 2)
                    titulos = titulos.Remove(titulos.Length - 2);
                if ((!media.IsAdult) || (media.IsAdult && ctx.Channel.IsNSFW))
                {
                    var builder = new DiscordEmbedBuilder
                    {
                        Title = media.TituloRomaji,
                        Url = media.UrlAnilist,
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                        {
                            Url = media.CoverImage
                        },
                        Footer = funciones.GetFooter(ctx),
                        Color = funciones.GetColor(),
                        Description = media.Descripcion
                    };
                    if (media.Episodios != null && media.Episodios.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":1234:")} Episodios", funciones.NormalizarField(media.Episodios), true);
                    if (media.Formato != null && media.Formato.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":dividers:")} Formato", funciones.NormalizarField(media.Formato), true);
                    if (media.Estado != null && media.Estado.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":hourglass_flowing_sand:")} Estado", funciones.NormalizarField(media.Estado.ToLower().ToUpperFirst()), true);
                    if (media.Score != null && media.Score.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":star:")} Puntuación", funciones.NormalizarField(media.Score), false);
                    if (media.Fechas != null && media.Fechas.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":calendar_spiral:")} Fecha emisión", funciones.NormalizarField(media.Fechas), false);
                    if (media.Generos != null && media.Generos.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":scroll:")} Generos", funciones.NormalizarField(media.Generos), false);
                    if (media.Tags != null && media.Tags.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":notepad_spiral:")} Etiquetas", funciones.NormalizarField(media.Tags), false);
                    if (media.Titulos != null && media.Titulos.Count > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":pencil:")} Titulos alternativos", funciones.NormalizarField(titulos), false);
                    if (media.Estudios != null && media.Estudios.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":minidisc:")} Estudios", funciones.NormalizarField(media.Estudios), false);
                    if (media.LinksExternos != null && media.LinksExternos.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":link:")} Links externos", funciones.NormalizarField(media.LinksExternos), false);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                }
                else
                {
                    DiscordMessage msg = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Requiere NSFW",
                        Description = "Este comando debe ser invocado en un canal NSFW.",
                        Color = new DiscordColor(0xFF0000),
                        Footer = funciones.GetFooter(ctx)
                    });
                    await Task.Delay(3000);
                    await funciones.BorrarMensaje(context, msg.Id);
                }
            }
            else
            {
                DiscordMessage msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(media.MsgError));
                await Task.Delay(5000);
                await funciones.BorrarMensaje(context, msg.Id);
            }
        }

        [SlashCommand("manga", "Busca un manga en AniList")]
        public async Task Manga(InteractionContext ctx, [Option("Manga", "Nombre del manga a buscar")] string manga)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = funciones.GetContext(ctx);
            var media = await funcionesAnilist.GetAniListMedia(ctx, manga, "manga");
            if (media.Ok == true)
            {
                string titulos = string.Empty;
                foreach (var title in media.Titulos)
                {
                    titulos += $"`{title}`, ";
                }
                if (titulos.Length >= 2)
                    titulos = titulos.Remove(titulos.Length - 2);
                if ((!media.IsAdult) || (media.IsAdult && ctx.Channel.IsNSFW))
                {
                    var builder = new DiscordEmbedBuilder
                    {
                        Title = media.TituloRomaji,
                        Url = media.UrlAnilist,
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                        {
                            Url = media.CoverImage
                        },
                        Footer = funciones.GetFooter(ctx),
                        Color = funciones.GetColor(),
                        Description = media.Descripcion
                    };
                    if (media.Formato != null && media.Formato.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":dividers:")} Formato", funciones.NormalizarField(media.Formato), true);
                    if (media.Estado != null && media.Estado.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":hourglass_flowing_sand:")} Estado", funciones.NormalizarField(media.Estado.ToLower().ToUpperFirst()), true);
                    if (media.Score != null && media.Score.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":star:")} Puntuación", funciones.NormalizarField(media.Score), true);
                    if (media.Fechas != null && media.Fechas.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":calendar_spiral:")} Fecha de publicación", funciones.NormalizarField(media.Fechas), false);
                    if (media.Generos != null && media.Generos.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":scroll:")} Generos", funciones.NormalizarField(media.Generos), false);
                    if (media.Tags != null && media.Tags.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":notepad_spiral:")} Etiquetas", funciones.NormalizarField(media.Tags), false);
                    if (media.Titulos != null && media.Titulos.Count > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":pencil:")} Titulos alternativos", funciones.NormalizarField(titulos), false);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
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
                    await funciones.BorrarMensaje(context, msg.Id);
                }
            }
            else
            {
                DiscordMessage msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(media.MsgError));
                await Task.Delay(5000);
                await funciones.BorrarMensaje(context, msg.Id);
            }
        }

        [SlashCommand("character", "Busca un personaje en AniList")]
        public async Task Character(InteractionContext ctx, [Option("Personaje", "Nombre del personaje a buscar")] string personaje)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = funciones.GetContext(ctx);
            var request = new GraphQLRequest
            {
                Query =
                "query($nombre : String){" +
                "   Page(perPage:5){" +
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
                if (data != null && data.Data != null && data.Data.Page.characters != null)
                {
                    int cont = 0;
                    List<string> opc = new();
                    foreach (var animeP in data.Data.Page.characters)
                    {
                        cont++;
                        string opcStr = animeP.name.full;
                        opc.Add(opcStr);
                    }
                    var elegido = await funciones.GetElegido(ctx, opc);
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
                        if (animes != null && animes.Length > 0)
                            builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":tv:")} Animes", funciones.NormalizarField(animes), false);
                        if (mangas != null && mangas.Length > 0)
                            builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":book:")} Mangas", funciones.NormalizarField(mangas), false);
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                    }
                    else
                    {
                        await ctx.DeleteResponseAsync();
                    }
                }
                else
                {
                    if (data.Errors == null)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                        {
                            Color = DiscordColor.Red,
                            Title = "Error",
                            Description = $"No se ha encontrado el personaje `{personaje}`"
                        }));
                    }
                    else
                    {
                        foreach (var x in data.Errors)
                        {
                            var msg = await ctx.Channel.SendMessageAsync($"Error: {x.Message}").ConfigureAwait(false);
                            await Task.Delay(3000);
                            await funciones.BorrarMensaje(context, msg.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string mensaje = ex.Message switch
                {
                    "The HTTP request failed with status code NotFound" => $"No se ha encontrado el personaje `{personaje}`",
                    _ => $"Error inesperado, mensaje: [{ex.Message}"
                };
                DiscordMessage msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(mensaje));
                await Task.Delay(5000);
                await funciones.BorrarMensaje(context, msg.Id);
            }
        }

        // Staff, algun dia

        [SlashCommand("sauce", "Busca el anime de una imagen")]
        public async Task Sauce(InteractionContext ctx, [Option("Rondas", "Link de la imagen")] string url)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = funciones.GetContext(ctx);
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
                        await funciones.BorrarMensaje(context, procesando.Id);
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
                                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
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
                DiscordMessage msgError = await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(msg));
                await Task.Delay(5000);
                await funciones.BorrarMensaje(context, msgError.Id);
            }
        }

        [SlashCommand("pj", "Personaje aleatorio")]
        public async Task Pj(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            int pag = funciones.GetNumeroRandom(1, 5000);
            var context = funciones.GetContext(ctx);
            Character personaje = await funciones.GetRandomCharacter(context, pag);
            if(personaje != null)
            {
                DiscordEmoji corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                var builder = new DiscordEmbedBuilder
                {
                    Title = personaje.NameFull,
                    Url = personaje.SiteUrl,
                    ImageUrl = personaje.Image,
                    Description = $"[{personaje.AnimePrincipal.TitleRomaji}]({personaje.AnimePrincipal.SiteUrl})\n{personaje.Favoritos} {corazon} (nº {pag} en popularidad)",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor()
                };
                var msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsup:")).ConfigureAwait(false);
                await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsdown:")).ConfigureAwait(false);
                await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":question:")).ConfigureAwait(false);
            }
        }

        [SlashCommand("AWCRuntime", "Calcula los minutos totales entre animes para AWC")]
        public async Task AWCRuntime(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await ctx.DeleteResponseAsync();
            var context = funciones.GetContext(ctx);
            var interactivity = ctx.Client.GetInteractivity();
            bool terminar = false;
            int minutosTotales = 0;
            string animes = string.Empty;
            string titulo = "Minutos totales entre animes (AWC)";
            string descBase = $"{ctx.Member.Mention}, presiona un botón para continuar.";

            var builder = new DiscordEmbedBuilder
            {
                Title = titulo,
                Description = descBase,
                Color = funciones.GetColor(),
                Footer = funciones.GetFooter(ctx)
            };

            DiscordButtonComponent buttonSi = new(ButtonStyle.Success, "true", "Ingresar nombre de anime");
            DiscordButtonComponent buttonNo = new(ButtonStyle.Danger, "terminar", "Terminar");

            DiscordMessageBuilder msgPrincipalBuilder = new() {
                Embed = builder
            };

            msgPrincipalBuilder.AddComponents(buttonSi, buttonNo);

            DiscordMessage msgPrincipal = await msgPrincipalBuilder.SendAsync(ctx.Channel);

            do
            {
                var msgInter = await interactivity.WaitForButtonAsync(msgPrincipal, ctx.User, TimeSpan.FromSeconds(60));
                if (!msgInter.TimedOut)
                {
                    string resultBoton = msgInter.Result.Id;
                    if(resultBoton != "terminar")
                    {
                        string animeName = await funciones.GetStringInteractivity(context, "Ingrese un nombre de un anime", "Ejemplo: Naruto", "Tiempo agotado esperando la respuesta", 60);
                        if (!string.IsNullOrEmpty(animeName))
                        {
                            var request = new GraphQLRequest
                            {
                                Query =
                                    "query($nombre : String){" +
                                    "   Media(search: $nombre, type:ANIME){" +
                                    "       title {" +
                                    "           romaji" +
                                    "       }" +
                                    "       episodes," +
                                    "       duration," +
                                    "       status," +
                                    "       nextAiringEpisode {" +
                                    "           episode" +
                                    "       }" +
                                    "   }" +
                                    "}",
                                Variables = new
                                {
                                    nombre = animeName
                                }
                            };
                            try
                            {
                                var data = await graphQLClient.SendQueryAsync<dynamic>(request);
                                if (data.Data != null)
                                {
                                    string title = data.Data.Media.title.romaji;
                                    string episodes = data.Data.Media.episodes;
                                    string duration = data.Data.Media.duration;
                                    string status = data.Data.Media.status;

                                    if (status != "NOT_YET_RELEASED")
                                    {
                                        if (duration != null)
                                        {
                                            int minutosAnime;
                                            if (episodes != null)
                                            {
                                                minutosAnime = int.Parse(episodes) * int.Parse(duration);
                                            }
                                            else
                                            {
                                                string nextEpisode = data.Data.Media.nextAiringEpisode.episode;
                                                minutosAnime = (int.Parse(nextEpisode) - 1) * int.Parse(duration);
                                            }
                                            minutosTotales += minutosAnime;

                                            animes += $"**Anime:** {title} | **Minutos:** {minutosAnime}\n";

                                            builder = new DiscordEmbedBuilder
                                            {
                                                Title = titulo,
                                                Description = $"**Animes ingresados:**\n\n{animes}\n**Minutos totales:** {minutosTotales}",
                                                Color = funciones.GetColor(),
                                                Footer = funciones.GetFooter(ctx)
                                            };

                                            msgPrincipalBuilder = new DiscordMessageBuilder()
                                            {
                                                Embed = builder
                                            };
                                            msgPrincipalBuilder.AddComponents(buttonSi, buttonNo);

                                            if(msgPrincipal != null)
                                            {
                                                await msgPrincipal.DeleteAsync("Auto borrado de Yumiko");
                                            }
                                            msgPrincipal = await msgPrincipalBuilder.SendAsync(ctx.Channel);
                                        }
                                        else
                                        {
                                            var msgError = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                                            {
                                                Title = "Anime incorrecto",
                                                Description = $"{ctx.Member.Mention}, no se pueden ingresar animes que no tengan la duración capitulos definida",
                                                Color = DiscordColor.Red,
                                                Footer = funciones.GetFooter(ctx)
                                            });
                                            await Task.Delay(3000);
                                            await funciones.BorrarMensaje(context, msgError.Id);
                                        }
                                    }
                                    else
                                    {
                                        var msgError = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                                        {
                                            Title = "Anime incorrecto",
                                            Description = $"{ctx.Member.Mention}, no se pueden ingresar animes que no hayan sido emitidos",
                                            Color = DiscordColor.Red,
                                            Footer = funciones.GetFooter(ctx)
                                        });
                                        await Task.Delay(3000);
                                        await funciones.BorrarMensaje(context, msgError.Id);
                                    }
                                }
                                else
                                {
                                    foreach (var x in data.Errors)
                                    {
                                        var msg = await ctx.Channel.SendMessageAsync($"Error: {x.Message}").ConfigureAwait(false);
                                        await Task.Delay(3000);
                                        await funciones.BorrarMensaje(context, msg.Id);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                DiscordMessage msg = ex.Message switch
                                {
                                    "The HTTP request failed with status code NotFound" => await ctx.Channel.SendMessageAsync($"No se ha encontrado al anime `{animeName}`").ConfigureAwait(false),
                                    _ => await ctx.Channel.SendMessageAsync($"Error inesperado").ConfigureAwait(false),
                                };
                                await Task.Delay(5000);
                                await funciones.BorrarMensaje(context, msg.Id);
                            }
                        }
                    }
                    else
                    {
                        terminar = true;
                    }
                }
                else
                {
                    terminar = true;
                }
            } while (!terminar);
            builder = new DiscordEmbedBuilder
            {
                Title = titulo,
                Description = $"**Animes ingresados:**\n\n{animes}\n**Minutos totales:** {minutosTotales}",
                Color = funciones.GetColor(),
                Footer = funciones.GetFooter(ctx)
            };

            msgPrincipalBuilder = new DiscordMessageBuilder()
            {
                Embed = builder
            };

            await msgPrincipal.ModifyAsync(msgPrincipalBuilder);
        }
    }
}
