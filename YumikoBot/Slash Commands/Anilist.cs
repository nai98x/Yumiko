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

        [SlashCommand("anilist", "Search for an AniList profile")]
        public async Task Profile(InteractionContext ctx, [Option("Nickname", "AniList nickname of the user to search")] string usuario)
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
                        nsfw = "Yes";
                    else
                        nsfw = "No";
                    string animeStats = $"Total: `{data.Data.User.statistics.anime.count}`\nEpisodes: `{data.Data.User.statistics.anime.episodesWatched}`\nAvarage Score: `{data.Data.User.statistics.anime.meanScore}`";
                    string mangaStats = $"Total: `{data.Data.User.statistics.manga.count}`\nChapters: `{data.Data.User.statistics.manga.chaptersRead}`\nAvarage Score: `{data.Data.User.statistics.manga.meanScore}`";
                    string options = $"Titles language: `{data.Data.User.options.titleLanguage}`\nNSFW: `{nsfw}`\nColor: `{data.Data.User.options.profileColor}`";
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
                    builder.AddField("Stats - Anime", animeStats, true);
                    builder.AddField("Stats - Manga", mangaStats, true);
                    builder.AddField("User Preferences", options, true);
                    if (favoriteAnime != "")
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":tv:")} Favorite anime", favoriteAnime, true);
                    if (favoriteManga != "")
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":book:")} Favorite manga", favoriteManga, true);
                    if (favoriteCharacters != "")
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":bust_in_silhouette:")} Favorite characters", favoriteCharacters, true);
                    if (favoriteStaff != "")
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":man_artist:")} Favorite staff", favoriteStaff, true);
                    if (favoriteStudios != "")
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":minidisc:")} Favorite studios", favoriteStudios, true);
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
                    "The HTTP request failed with status code NotFound" => $"AniList user not found `{usuario}`",
                    _ => $"Unknown error, message: [{ex.Message}"
                };
                DiscordMessage msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(mensaje));
                await Task.Delay(5000);
                await funciones.BorrarMensaje(context, msg.Id);
            }
        }

        [SlashCommand("anime", "Find an anime on AniList")]
        public async Task Anime(InteractionContext ctx, [Option("Anime", "Name of anime")] string anime)
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
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":1234:")} Episodes", funciones.NormalizarField(media.Episodios), true);
                    if (media.Formato != null && media.Formato.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":dividers:")} Format", funciones.NormalizarField(media.Formato), true);
                    if (media.Estado != null && media.Estado.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":hourglass_flowing_sand:")} Status", funciones.NormalizarField(media.Estado.ToLower().ToUpperFirst()), true);
                    if (media.Score != null && media.Score.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":star:")} Score", funciones.NormalizarField(media.Score), false);
                    if (media.Fechas != null && media.Fechas.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":calendar_spiral:")} Start Date", funciones.NormalizarField(media.Fechas), false);
                    if (media.Generos != null && media.Generos.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":scroll:")} Genres", funciones.NormalizarField(media.Generos), false);
                    if (media.Tags != null && media.Tags.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":notepad_spiral:")} Tags", funciones.NormalizarField(media.Tags), false);
                    if (media.Titulos != null && media.Titulos.Count > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":pencil:")} Synonyms", funciones.NormalizarField(titulos), false);
                    if (media.Estudios != null && media.Estudios.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":minidisc:")} Studios", funciones.NormalizarField(media.Estudios), false);
                    if (media.LinksExternos != null && media.LinksExternos.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":link:")} External links", funciones.NormalizarField(media.LinksExternos), false);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                }
                else
                {
                    DiscordMessage msg = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "NSFW required",
                        Description = "This command must be invoked on a NSFW channel.",
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

        [SlashCommand("manga", "Search for a manga on AniList")]
        public async Task Manga(InteractionContext ctx, [Option("Manga", "Name of the manga")] string manga)
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
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":dividers:")} Format", funciones.NormalizarField(media.Formato), true);
                    if (media.Estado != null && media.Estado.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":hourglass_flowing_sand:")} Status", funciones.NormalizarField(media.Estado.ToLower().ToUpperFirst()), true);
                    if (media.Score != null && media.Score.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":star:")} Score", funciones.NormalizarField(media.Score), true);
                    if (media.Fechas != null && media.Fechas.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":calendar_spiral:")} Start date", funciones.NormalizarField(media.Fechas), false);
                    if (media.Generos != null && media.Generos.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":scroll:")} Genres", funciones.NormalizarField(media.Generos), false);
                    if (media.Tags != null && media.Tags.Length > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":notepad_spiral:")} Tags", funciones.NormalizarField(media.Tags), false);
                    if (media.Titulos != null && media.Titulos.Count > 0)
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":pencil:")} Synonyms", funciones.NormalizarField(titulos), false);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                }
                else
                {
                    DiscordMessage msg = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "NSFW required",
                        Description = "This command must be invoked on a NSFW channel.",
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

        [SlashCommand("character", "Search for a character in AniList")]
        public async Task Character(InteractionContext ctx, [Option("Character", "Name of the character")] string personaje)
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
                            descripcion = "(Without description)";
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
                            Description = $"Character `{personaje}` not found"
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
                    "The HTTP request failed with status code NotFound" => $"Character `{personaje}` not found",
                    _ => $"Unknown error, message: [{ex.Message}"
                };
                DiscordMessage msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(mensaje));
                await Task.Delay(5000);
                await funciones.BorrarMensaje(context, msg.Id);
            }
        }

        // Staff, algun dia

        [SlashCommand("sauce", "Find the anime of an image (with trace.moe)")]
        public async Task Sauce(InteractionContext ctx, [Option("Url", "Image link (must be JPG, PNG or JPEG)")] string url)
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
                        var procesando = await ctx.Channel.SendMessageAsync("Processing image..").ConfigureAwait(false);
                        IRestResponse response = client.Execute(request);
                        await funciones.BorrarMensaje(context, procesando.Id);
                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.OK:
                                var resp = JsonConvert.DeserializeObject<dynamic>(response.Content);
                                string resultados = string.Empty;
                                string titulo = "The possible anime in the image is:";
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
                                            $"Name:    [{resultado.title_romaji}]({enlace += resultado.anilist_id})\n" +
                                            $"Similarity: {similaridad}%\n" +
                                            $"Episode:  {resultado.episode} (Minute: {at})\n";
                                        break;
                                    }
                                }
                                if (!encontro)
                                {
                                    titulo = "No results found for this image";
                                    resultados = "Note: Only works with images that are parts of an episode";
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
                                msg = "You must enter a link";
                                break;
                            case HttpStatusCode.Forbidden:
                                msg = "Access denied";
                                break;
                            case HttpStatusCode.TooManyRequests:
                                msg = "Ratelimit exceeded";
                                break;
                            case HttpStatusCode.InternalServerError:
                            case HttpStatusCode.ServiceUnavailable:
                                msg = "Internal server error on Trace.moe";
                                break;
                            default:
                                msg = "Unknown error";
                                break;
                        }
                    }
                    else
                    {
                        msg = "The image must be JPG, PNG or JPEG";
                    }
                }
                else
                {
                    msg = "You must provide a image link";
                }
            }
            if (msg != "OK")
            {
                DiscordMessage msgError = await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(msg));
                await Task.Delay(5000);
                await funciones.BorrarMensaje(context, msgError.Id);
            }
        }

        [SlashCommand("pj", "Random character with reactions")]
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
                    Description = $"[{personaje.AnimePrincipal.TitleRomaji}]({personaje.AnimePrincipal.SiteUrl})\n{personaje.Favoritos} {corazon} (nº {pag} on popularity)",
                    Footer = funciones.GetFooter(ctx),
                    Color = funciones.GetColor()
                };
                var msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsup:")).ConfigureAwait(false);
                await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsdown:")).ConfigureAwait(false);
                await msg.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":question:")).ConfigureAwait(false);
            }
        }

        [SlashCommand("AWCRuntime", "Calculate total minutes between animes for AWC")]
        public async Task AWCRuntime(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await ctx.DeleteResponseAsync();
            var context = funciones.GetContext(ctx);
            var interactivity = ctx.Client.GetInteractivity();
            bool terminar = false;
            int minutosTotales = 0;
            string animes = string.Empty;
            string titulo = "Total minutes between animes (AWC)";
            string descBase = $"{ctx.Member.Mention}, press a button to continue.";

            var builder = new DiscordEmbedBuilder
            {
                Title = titulo,
                Description = descBase,
                Color = funciones.GetColor(),
                Footer = funciones.GetFooter(ctx)
            };

            DiscordButtonComponent buttonSi = new(ButtonStyle.Success, "true", "Enter anime name");
            DiscordButtonComponent buttonNo = new(ButtonStyle.Danger, "terminar", "Finish");

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
                        string animeName = await funciones.GetStringInteractivity(context, "Ingrese un nombre de un anime", "Example: Naruto", "Time timed out waiting for the answer", 60);
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

                                            animes += $"**Anime:** {title} | **Minutes:** {minutosAnime}\n";

                                            builder = new DiscordEmbedBuilder
                                            {
                                                Title = titulo,
                                                Description = $"**Animes entered:**\n\n{animes}\n**Total minutes:** {minutosTotales}",
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
                                                await msgPrincipal.DeleteAsync("Yumiko auto erase");
                                            }
                                            msgPrincipal = await msgPrincipalBuilder.SendAsync(ctx.Channel);
                                        }
                                        else
                                        {
                                            var msgError = await ctx.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                                            {
                                                Title = "Not valid anime",
                                                Description = $"{ctx.Member.Mention}, you cannot enter animes that do not have the defined episodes duration",
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
                                            Title = "Not valid anime",
                                            Description = $"{ctx.Member.Mention}, animes that have not been released cannot be entered",
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
                                    "The HTTP request failed with status code NotFound" => await ctx.Channel.SendMessageAsync($"Anime `{animeName}` not found").ConfigureAwait(false),
                                    _ => await ctx.Channel.SendMessageAsync($"Unknown error").ConfigureAwait(false),
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
                Description = $"**Animes entered:**\n\n{animes}\n**Total minutes:** {minutosTotales}",
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
