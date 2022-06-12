namespace Yumiko.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity.Extensions;
    using DSharpPlus.SlashCommands;
    using GraphQL;
    using GraphQL.Client.Abstractions.Utilities;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using RestSharp;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading.Tasks;
    using Yumiko.Datatypes;
    using Yumiko.Enums;
    using Yumiko.Services.Firebase;
    using Yumiko.Utils;

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Not with D#+ Command classes")]
    [SlashCommandGroup("anilist", "Anilist queries")]
    public class Anilist : ApplicationCommandModule
    {
        public IConfigurationRoot Configuration { private get; set; } = null!;
        private readonly GraphQLHttpClient graphQlClient = new("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        [SlashCommand("setprofile", "Sets your AniList profile")]
        public async Task SetAnilist(InteractionContext ctx, [Option("Profile", "Nickname or URL of your AniList profile (must be public)")] string perfil)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var porUrl = Uri.TryCreate(perfil, UriKind.Absolute, out var uriResult)
                         && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (porUrl)
            {
                var match = perfil.Contains("https://anilist.co/user/");
                if (match)
                {
                    var inputUrl = perfil.Trim();
                    var userName = inputUrl;
                    if (inputUrl.EndsWith("/"))
                    {
                        userName = inputUrl.Remove(inputUrl.Length - 1);
                    }

                    var index = userName.LastIndexOf('/');
                    perfil = userName[(index + 1)..];
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = "Error",
                        Description = $"{ctx.User.Mention}, you must enter the URL of your Anilist profile!\nExample: https://anilist.co/user/Josh/",
                        Color = DiscordColor.Red,
                    }));
                    return;
                }
            }

            var request = new GraphQLRequest
            {
                Query =
                "query($nombre : String){" +
                "   User(search: $nombre){" +
                "       siteUrl," +
                "       id," +
                "       name" +
                "   }" +
                "}",
                Variables = new
                {
                    nombre = perfil,
                },
            };
            try
            {
                GraphQLHttpClient graphQlClient = new("https://graphql.anilist.co", new NewtonsoftJsonSerializer());
                var data = await graphQlClient.SendQueryAsync<dynamic>(request);
                if (data.Data != null)
                {
                    string siteurl = data.Data.User.siteUrl;
                    string name = data.Data.User.name;
                    string idString = data.Data.User.id;
                    var idAnilist = int.Parse(idString);
                    var confirmar = await Common.GetYesNoInteractivityAsync(ctx, Configuration, ctx.Client.GetInteractivity(), "Confirm that you want to save this profile", $"**Your Anilist profile is:**\n\n   **Nickname:** {name}\n   **Url:** {siteurl}");
                    if (confirmar)
                    {
                        await UsuariosAnilist.SetAnilistAsync(Configuration, idAnilist, ctx.Member.Id);
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: new DiscordEmbedBuilder
                        {
                            Color = DiscordColor.Green,
                            Title = "New profile saved successfully",
                            Description = $"{ctx.User.Mention}, you have successfully saved your Anilist profile",
                        }));
                    }
                    else
                    {
                        await ctx.DeleteResponseAsync();
                    }
                }
                else
                {
                    if (data.Errors != null)
                    {
                        foreach (var x in data.Errors)
                        {
                            var msg = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Error: {x.Message}"));
                            await Task.Delay(3000);
                            await Common.BorrarMensajeAsync(ctx, msg.Id);
                        }
                    }

                    await ctx.DeleteResponseAsync();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message switch
                {
                    "The HTTP request failed with status code NotFound" => await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Profile not found: `{perfil}`")),
                    _ => await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Unknown error: {ex.Message}")),
                };
                await Task.Delay(5000);
                await Common.BorrarMensajeAsync(ctx, msg.Id);
                await ctx.DeleteResponseAsync();
            }
        }

        [SlashCommand("deleteprofile", "Deletes your AniList profile")]
        public async Task DeleteAnilist(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = ctx;
            var confirmar = await Common.GetYesNoInteractivityAsync(context, Configuration, ctx.Client.GetInteractivity(), "Confirm that you want to delete your profile", "This action can not be undone");
            if (confirmar)
            {
                var borrado = await UsuariosAnilist.DeleteAnilistAsync(Configuration, ctx.User.Id);
                if (borrado)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = "Success",
                        Description = "Anilist profile deleted successfully",
                        Color = DiscordColor.Green,
                    }));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = "Error",
                        Description = "Anilist profile not found",
                        Color = DiscordColor.Red,
                    }));
                }
            }
            else
            {
                await ctx.DeleteResponseAsync();
            }
        }

        [SlashCommand("profile", "Searchs for an AniList profile")]
        public async Task Profile(InteractionContext ctx, [Option("Member", "Member whose Anilist profile you want to see")] DiscordUser user)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = ctx;

            user ??= ctx.User;
            var miembro = await ctx.Guild.GetMemberAsync(user.Id);
            var userAnilist = await UsuariosAnilist.GetPerfilAsync(Configuration, miembro.Id);
            if (userAnilist != null)
            {
                var anilistId = userAnilist.AnilistId;
                var request = new GraphQLRequest
                {
                    Query =
                "query ($codigo: Int) {" +
                "   User(id: $codigo) {" +
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
                "                       romaji," +
                "                       english" +
                "                   }," +
                "                   siteUrl" +
                "               }" +
                "           }," +
                "           manga(perPage:3){" +
                "               nodes{" +
                "                   title{" +
                "                       romaji," +
                "                       english" +
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
                        codigo = anilistId,
                    },
                };
                try
                {
                    var data = await graphQlClient.SendQueryAsync<dynamic>(request);
                    if (data.Data != null)
                    {
                        string nsfw1 = data.Data.User.options.displayAdultContent;
                        string nsfw;
                        if (nsfw1 == "True")
                        {
                            nsfw = "Yes";
                        }
                        else
                        {
                            nsfw = "No";
                        }

                        string titulosStr = data.Data.User.options.titleLanguage;
                        string colorStr = data.Data.User.options.profileColor;
                        var animeStats = $"Total: `{data.Data.User.statistics.anime.count}`\nEpisodes: `{data.Data.User.statistics.anime.episodesWatched}`\nMean score: `{data.Data.User.statistics.anime.meanScore}/100`";
                        var mangaStats = $"Total: `{data.Data.User.statistics.manga.count}`\nChapters: `{data.Data.User.statistics.manga.chaptersRead}`\nMean score: `{data.Data.User.statistics.manga.meanScore}/100`";
                        var options = $"Titles language: `{titulosStr.UppercaseFirst()}`\nNSFW: `{nsfw}`\nColor: `{colorStr.UppercaseFirst()}`";
                        var favoriteAnime = string.Empty;
                        foreach (var anime in data.Data.User.favourites.anime.nodes)
                        {
                            string tituloRomaji = anime.title.romaji;
                            string tituloEnglish = anime.title.english;
                            if (titulosStr == "ENGLISH" && !string.IsNullOrEmpty(tituloEnglish))
                            {
                                favoriteAnime += $"- [{tituloEnglish}]({anime.siteUrl})\n";
                            }
                            else
                            {
                                favoriteAnime += $"- [{tituloRomaji}]({anime.siteUrl})\n";
                            }
                        }

                        var favoriteManga = string.Empty;
                        foreach (var manga in data.Data.User.favourites.manga.nodes)
                        {
                            string tituloRomaji = manga.title.romaji;
                            string tituloEnglish = manga.title.english;
                            if (titulosStr == "ENGLISH" && !string.IsNullOrEmpty(tituloEnglish))
                            {
                                favoriteManga += $"- [{tituloEnglish}]({manga.siteUrl})\n";
                            }
                            else
                            {
                                favoriteManga += $"- [{tituloRomaji}]({manga.siteUrl})\n";
                            }
                        }

                        var favoriteCharacters = string.Empty;
                        foreach (var character in data.Data.User.favourites.characters.nodes)
                        {
                            favoriteCharacters += $"- [{character.name.full}]({character.siteUrl})\n";
                        }

                        var favoriteStaff = string.Empty;
                        foreach (var staff in data.Data.User.favourites.staff.nodes)
                        {
                            favoriteStaff += $"- [{staff.name.full}]({staff.siteUrl})\n";
                        }

                        var favoriteStudios = string.Empty;
                        foreach (var studio in data.Data.User.favourites.studios.nodes)
                        {
                            favoriteStudios += $"- [{studio.name}]({studio.siteUrl})\n";
                        }

                        string nombre = data.Data.User.name;
                        string avatar = data.Data.User.avatar.medium;
                        string siteurl = data.Data.User.siteUrl;
                        var builder = new DiscordEmbedBuilder
                        {
                            Title = nombre,
                            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                            {
                                Url = avatar,
                            },
                            Color = Constants.YumikoColor,
                            ImageUrl = data.Data.User.bannerImage,
                        };
                        builder.AddField("Anime Stats", animeStats, true);
                        builder.AddField("Manga Stats", mangaStats, true);
                        builder.AddField("Settings", options, true);
                        if (favoriteAnime != string.Empty)
                        {
                            builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":tv:")} Favorite animes", favoriteAnime, true);
                        }

                        if (favoriteManga != string.Empty)
                        {
                            builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":book:")} Favorite mangas", favoriteManga, true);
                        }

                        if (favoriteCharacters != string.Empty)
                        {
                            builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":bust_in_silhouette:")} ", favoriteCharacters, true);
                        }

                        if (favoriteStaff != string.Empty)
                        {
                            builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":man_artist:")} Favorite staff", favoriteStaff, true);
                        }

                        if (favoriteStudios != string.Empty)
                        {
                            builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":minidisc:")} Favorite studios", favoriteStudios, true);
                        }

                        DiscordLinkButtonComponent perfil = new($"{siteurl}", "Profile", false, new DiscordComponentEmoji("👤"));
                        DiscordLinkButtonComponent animeList = new($"{siteurl}/animelist", "Anime List", false, new DiscordComponentEmoji("📺"));
                        DiscordLinkButtonComponent mangaList = new($"{siteurl}/mangalist", "Manga List", false, new DiscordComponentEmoji("📖"));
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder).AddComponents(perfil, animeList, mangaList));
                    }
                    else
                    {
                        if (data.Errors != null)
                        {
                            foreach (var x in data.Errors)
                            {
                                var msg = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Error: {x.Message}"));
                                await Task.Delay(5000);
                                await Common.BorrarMensajeAsync(context, msg.Id);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var mensaje = ex.Message switch
                    {
                        "The HTTP request failed with status code NotFound" => $"Anilist profile not found, {user.Mention}",
                        _ => $"Unknown error, message: [{ex.Message}"
                    };
                    var msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = "Error",
                        Description = mensaje,
                        Color = DiscordColor.Red,
                    }));
                    await Task.Delay(5000);
                    await Common.BorrarMensajeAsync(context, msg.Id);
                }
            }
            else
            {
                var builder = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = "Anilist profile not found",
                    Description = $"{miembro.Mention}, you dont have any Anilist profile vinculated.\n\n" +
                                $"To vinculate your profile, the user must use the following command: `/anilist setanilist`",
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
            }
        }

        [SlashCommand("anime", "Searchs for an anime")]
        public async Task Anime(InteractionContext ctx, [Option("Anime", "Anime to search")] string anime, [Option("User", "User's Anilist stats")] DiscordUser? usuario = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = ctx;
            usuario ??= ctx.User;
            var media = await AnilistUtils.GetAniListMedia(ctx, Configuration, anime, MediaType.ANIME);
            if (media.Ok == true)
            {
                string titulos;
                if (media.Titulos != null)
                {
                    titulos = string.Join(", ", media.Titulos);
                }
                else
                {
                    titulos = "Without titles";
                }

                if ((!media.IsAdult) || (media.IsAdult && ctx.Channel.IsNSFW))
                {
                    var builder = new DiscordEmbedBuilder
                    {
                        Title = media.TituloRomaji,
                        Url = media.UrlAnilist,
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                        {
                            Url = media.CoverImage,
                        },
                        Color = Constants.YumikoColor,
                        Description = media.Descripcion
                    };
                    if (media.Episodios != null && media.Episodios.Length > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":1234:")} Episodes", Common.NormalizarField(media.Episodios), true);
                    }

                    if (media.Formato != null && media.Formato.Length > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":dividers:")} Format", Common.NormalizarField(media.Formato), true);
                    }

                    if (media.Estado != null && media.Estado.Length > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":hourglass_flowing_sand:")} Status", Common.NormalizarField(media.Estado.ToLower().ToUpperFirst()), true);
                    }

                    if (media.Score != null && media.Score.Length > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":star:")} Score", Common.NormalizarField(media.Score), false);
                    }

                    if (media.Fechas != null && media.Fechas.Length > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":calendar_spiral:")} Start date", Common.NormalizarField(media.Fechas), false);
                    }

                    if (media.Generos != null && media.Generos.Length > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":scroll:")} Genres", Common.NormalizarField(media.Generos), false);
                    }

                    if (media.Tags != null && media.Tags.Length > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":notepad_spiral:")} Tags", Common.NormalizarField(media.Tags), false);
                    }

                    if (media.Titulos != null && media.Titulos.Count > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":pencil:")} Synonyms", Common.NormalizarField(titulos), false);
                    }

                    if (media.Estudios != null && media.Estudios.Length > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":minidisc:")} Studios", Common.NormalizarField(media.Estudios), false);
                    }

                    if (media.LinksExternos != null && media.LinksExternos.Length > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":link:")} External & Streaming links", Common.NormalizarField(media.LinksExternos), false);
                    }

                    DiscordWebhookBuilder whbuilder = new();
                    whbuilder.AddEmbed(builder);

                    var usuarioB = await UsuariosAnilist.GetPerfilAsync(Configuration, usuario.Id);
                    if (usuarioB != null)
                    {
                        var embedN = await AnilistUtils.GetInfoMediaUser(ctx, usuarioB.AnilistId, media.Id);
                        if (embedN != null)
                        {
                            whbuilder.AddEmbed(embedN);
                        }
                    }

                    await ctx.EditResponseAsync(whbuilder);
                }
                else
                {
                    var msg = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(Constants.NsfwWarning));
                    await Task.Delay(3000);
                    await Common.BorrarMensajeAsync(context, msg.Id);
                }
            }
            else
            {
                var msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(media.MsgError));
                await Task.Delay(5000);
                await Common.BorrarMensajeAsync(context, msg.Id);
            }
        }

        [SlashCommand("manga", "Searchs for a manga")]
        public async Task Manga(InteractionContext ctx, [Option("Manga", "Manga to search")] string manga, [Option("User", "User's Anilist stats")] DiscordUser? usuario = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = ctx;
            usuario ??= ctx.User;
            var media = await AnilistUtils.GetAniListMedia(ctx, Configuration, manga, MediaType.MANGA);
            if (media.Ok == true)
            {
                string titulos;
                if (media.Titulos != null)
                {
                    titulos = string.Join(", ", media.Titulos);
                }
                else
                {
                    titulos = "Without titles";
                }

                if ((!media.IsAdult) || (media.IsAdult && ctx.Channel.IsNSFW))
                {
                    var builder = new DiscordEmbedBuilder
                    {
                        Title = media.TituloRomaji,
                        Url = media.UrlAnilist,
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail()
                        {
                            Url = media.CoverImage,
                        },
                        Color = Constants.YumikoColor,
                        Description = media.Descripcion,
                    };
                    if (media.Chapters != null && media.Chapters.Length > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":1234:")} Chapters", Common.NormalizarField(media.Chapters), true);
                    }

                    if (media.Formato != null && media.Formato.Length > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":dividers:")} Format", Common.NormalizarField(media.Formato), true);
                    }

                    if (media.Estado != null && media.Estado.Length > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":hourglass_flowing_sand:")} Status", Common.NormalizarField(media.Estado.ToLower().ToUpperFirst()), true);
                    }

                    if (media.Score != null && media.Score.Length > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":star:")} Score", Common.NormalizarField(media.Score), true);
                    }

                    if (media.Fechas != null && media.Fechas.Length > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":calendar_spiral:")} Publication date", Common.NormalizarField(media.Fechas), false);
                    }

                    if (media.Generos != null && media.Generos.Length > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":scroll:")} Genres", Common.NormalizarField(media.Generos), false);
                    }

                    if (media.Tags != null && media.Tags.Length > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":notepad_spiral:")} Tags", Common.NormalizarField(media.Tags), false);
                    }

                    if (media.Titulos != null && media.Titulos.Count > 0)
                    {
                        builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":pencil:")} Synonyms", Common.NormalizarField(titulos), false);
                    }

                    DiscordWebhookBuilder whbuilder = new();
                    whbuilder.AddEmbed(builder);

                    var usuarioB = await UsuariosAnilist.GetPerfilAsync(Configuration, usuario.Id);
                    if (usuarioB != null)
                    {
                        var embedN = await AnilistUtils.GetInfoMediaUser(ctx, usuarioB.AnilistId, media.Id);
                        if (embedN != null)
                        {
                            whbuilder.AddEmbed(embedN);
                        }
                    }

                    await ctx.EditResponseAsync(whbuilder);
                }
                else
                {
                    var msg = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(Constants.NsfwWarning));
                    await Task.Delay(3000);
                    await Common.BorrarMensajeAsync(context, msg.Id);
                }
            }
            else
            {
                var msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(media.MsgError));
                await Task.Delay(5000);
                await Common.BorrarMensajeAsync(context, msg.Id);
            }
        }

        [SlashCommand("character", "Searchs for a Character")]
        public async Task Character(InteractionContext ctx, [Option("Character", "character to search")] string personaje)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = ctx;
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
                    nombre = personaje,
                },
            };
            try
            {
                GraphQLResponse<dynamic> data = await graphQlClient.SendQueryAsync<dynamic>(request);
                if (data != null && data.Data != null)
                {
                    var cont = 0;
                    List<AnimeShort> opc = new();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    foreach (var animeP in data.Data.Page.characters)
                    {
                        cont++;

                        string opcName = animeP.name.full;
                        string opcDesc;
                        Newtonsoft.Json.Linq.JArray animes = animeP.animes.nodes;
                        if (animes.Count > 0)
                        {
                            string opcAnime = animeP.animes.nodes[0].title.romaji;
                            opcDesc = opcAnime;
                        }
                        else
                        {
                            Newtonsoft.Json.Linq.JArray mangas = animeP.mangas.nodes;
                            if (mangas.Count > 0)
                            {
                                string opcManga = animeP.mangas.nodes[0].title.romaji;
                                opcDesc = opcManga;
                            }
                            else
                            {
                                opcDesc = "(Without animes or mangas)";
                            }
                        }

                        opc.Add(new AnimeShort
                        {
                            Title = opcName,
                            Description = opcDesc,
                        });
                    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                    var elegido = await Common.GetElegidoAsync(ctx, Configuration, opc);
                    if (elegido > 0)
                    {
                        var datos = data.Data.Page.characters[elegido - 1];
                        string descripcion = datos.description;
                        descripcion = Common.NormalizarDescription(Common.LimpiarTexto(descripcion));
                        if (descripcion == string.Empty)
                        {
                            descripcion = "(Without description)";
                        }

                        string nombre = datos.name.full;
                        string imagen = datos.image.large;
                        string urlAnilist = datos.siteUrl;
                        var animes = string.Empty;
                        foreach (var anime in datos.animes.nodes)
                        {
                            animes += $"[{anime.title.romaji}]({anime.siteUrl})\n";
                        }

                        var mangas = string.Empty;
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
                                Url = imagen,
                            },
                            Color = Constants.YumikoColor,
                            Description = descripcion,
                        };
                        if (animes != null && animes.Length > 0)
                        {
                            builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":tv:")} Animes", Common.NormalizarField(animes), false);
                        }

                        if (mangas != null && mangas.Length > 0)
                        {
                            builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":book:")} Mangas", Common.NormalizarField(mangas), false);
                        }

                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                    }
                    else
                    {
                        await ctx.DeleteResponseAsync();
                    }
                }
                else
                {
                    if (data?.Errors == null)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                        {
                            Color = DiscordColor.Red,
                            Title = "Error",
                            Description = $"Character not found: `{personaje}`",
                        }));
                    }
                    else
                    {
                        foreach (var x in data.Errors)
                        {
                            var msg = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Error: {x.Message}"));
                            await Task.Delay(3000);
                            await Common.BorrarMensajeAsync(context, msg.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var mensaje = ex.Message switch
                {
                    "The HTTP request failed with status code NotFound" => $"No se ha encontrado el personaje `{personaje}`",
                    _ => $"Unknown error, message: [{ex.Message}"
                };
                var msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(mensaje));
                await Task.Delay(5000);
                await Common.BorrarMensajeAsync(context, msg.Id);
            }
        }

        // Staff, algun dia
        [SlashCommand("sauce", "Searchs for the anime of an image")]
        public async Task Sauce(InteractionContext ctx, [Option("Image", "Image link")] string url)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var msg = "OK";
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                var extension = url[^4..];
                if (extension == ".jpg" || extension == ".png" || extension == "jpeg")
                {
                    var client = new RestClient("https://api.trace.moe/search?url=" + url);
                    var request = new RestRequest();
                    request.AddHeader("content-type", "application/json");
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent("Processing image.."));
                    var response = await client.ExecuteAsync(request);
                    if (response.IsSuccessful)
                    {
                        if (response.Content != null)
                        {
                            var resp = JsonConvert.DeserializeObject<dynamic>(response.Content);
                            var resultados = string.Empty;
                            var titulo = "The possible anime of the image is:";
                            var encontro = false;
                            if (resp != null)
                            {
                                foreach (var resultado in resp.result)
                                {
                                    var enlace = "https://anilist.co/anime/";
                                    int similaridad = resultado.similarity * 100;
                                    if (similaridad >= 87)
                                    {
                                        encontro = true;
                                        int id = resultado.anilist;
                                        var media = await AnilistUtils.GetAniListMediaTitleAndNsfwFromId(ctx, id, MediaType.ANIME);
                                        string mediaTitle = media.Item1;
                                        bool nsfw = media.Item2;
                                        int from = resultado.from;
                                        string videoLink = resultado.video;
                                        if (!ctx.Channel.IsNSFW && nsfw)
                                        {
                                            msg = "This image is from an adult anime, please use this command in a age-restricted channel";
                                        }
                                        resultados =
                                            $"{Formatter.Bold("Name:")} [{mediaTitle}]({enlace += id})\n" +
                                            $"{Formatter.Bold("Similarity:")} {similaridad}%\n" +
                                            $"{Formatter.Bold("Episode:")} {resultado.episode} (Minute: {TimeSpan.FromSeconds(from):mm\\:ss}\n" +
                                            $"{Formatter.Bold("Video:")} [Link]({videoLink})";
                                        break;
                                    }
                                }

                                if (!encontro)
                                {
                                    titulo = "No results found for this image";
                                    resultados = "Remember that it only works with images that are part of an anime episode";
                                }

                                var embed = new DiscordEmbedBuilder
                                {
                                    Title = titulo,
                                    Description = resultados,
                                    ImageUrl = url,
                                    Color = Constants.YumikoColor,
                                };
                                embed.WithFooter("Retrieved from trace.moe", "https://trace.moe/favicon.png");
                                if(msg == "OK")
                                {
                                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                                    return;
                                }
                            }
                        }
                        else
                        {
                            msg = "Error obtaining the sauce";
                        }
                    }
                    else
                    {
                        msg = response.StatusCode switch
                        {
                            HttpStatusCode.BadRequest => "Invalid image url",
                            HttpStatusCode.PaymentRequired => "Search quota depleted / Concurrency limit exceeded",
                            HttpStatusCode.Forbidden => "	Invalid API key",
                            HttpStatusCode.MethodNotAllowed => "Method Not Allowed",
                            HttpStatusCode.InternalServerError => "Internal Server Error",
                            HttpStatusCode.ServiceUnavailable => "Search queue is full / Database is not responding",
                            HttpStatusCode.GatewayTimeout => "Server is overloaded",
                            _ => "Unknown error",
                        };
                        await Common.GrabarLogErrorAsync(ctx, "Error retriving image from trace.moe with `sauce` command.\nError: " + msg);
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Unknown error retrieving image from trace.moe"));
                        return;
                    }
                }
                else
                {
                    msg = "The image extension must be JPG, PNG o JPEG";
                }
            }
            else
            {
                msg = "You must enter the link of an image";
            }
            
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder()
            {
                Title = "Error",
                Description = msg,
                Color = DiscordColor.Red,
            }));
        }

        [SlashCommand("pj", "Random character")]
        public async Task Pj(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var pag = Common.GetNumeroRandom(1, 5000);
            var personaje = await Common.GetRandomCharacterAsync(ctx, pag);
            if (personaje != null)
            {
                var corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                var builder = new DiscordEmbedBuilder
                {
                    Title = personaje.NameFull,
                    Url = personaje.SiteUrl,
                    ImageUrl = personaje.Image,
                    Description = $"[{personaje.AnimePrincipal?.TitleRomaji}]({personaje.AnimePrincipal?.SiteUrl})\n{personaje.Favoritos} {corazon} (nº {pag} in popularity rank)",
                    Color = Constants.YumikoColor
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
            }
        }
    }
}
