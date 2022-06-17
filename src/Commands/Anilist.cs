namespace Yumiko.Commands
{
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

    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Not with D#+ Command classes")]
    [SlashCommandGroup("anilist", "Anilist queries")]
    public class Anilist : ApplicationCommandModule
    {
        public IConfigurationRoot Configuration { private get; set; } = null!;
        private readonly GraphQLHttpClient graphQlClient = new("https://graphql.anilist.co", new NewtonsoftJsonSerializer());

        public override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(ctx.Interaction.Locale!);
            return Task.FromResult(true);
        }

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
                        Title = strings.error,
                        Description = $"{string.Format(strings.must_enter_anilist_profile_url, ctx.User.Mention)}!\n{strings.example}: https://anilist.co/user/Josh/",
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
                    var confirmar = await Common.GetYesNoInteractivityAsync(ctx, ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGeneral), ctx.Client.GetInteractivity(), strings.confirm_save_profile, $"**{strings.your_anilist_profile_is}:**\n\n   **Nickname:** {name}\n   **Url:** {siteurl}");
                    if (confirmar)
                    {
                        await UsuariosAnilist.SetAnilistAsync(idAnilist, ctx.Member.Id);
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: new DiscordEmbedBuilder
                        {
                            Color = DiscordColor.Green,
                            Title = strings.new_profile_saved,
                            Description = string.Format(strings.new_profile_saved_mention, ctx.User.Mention),
                        }));
                    }
                    else
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: new DiscordEmbedBuilder
                        {
                            Color = DiscordColor.Red,
                            Title = strings.action_cancelled,
                            Description = string.Format(strings.new_profile_cancelled_mention, ctx.User.Mention),
                        }));
                    }
                }
                else
                {
                    if (data.Errors != null)
                    {
                        foreach (var x in data.Errors)
                        {
                            var msg = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{strings.error}: {x.Message}"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message switch
                {
                    "The HTTP request failed with status code NotFound" => await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{strings.anilist_profile_not_found}: `{perfil}`")),
                    _ => await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{strings.unknown_error}: {ex.Message}")),
                };
            }
        }

        [SlashCommand("deleteprofile", "Deletes your AniList profile")]
        public async Task DeleteAnilist(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var context = ctx;
            var confirmar = await Common.GetYesNoInteractivityAsync(context, ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGeneral), ctx.Client.GetInteractivity(), strings.confirm_delete_profile, strings.action_cannont_be_undone);
            if (confirmar)
            {
                var borrado = await UsuariosAnilist.DeleteAnilistAsync(ctx.User.Id);
                if (borrado)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = strings.success,
                        Description = strings.anilist_profile_deleted_successfully,
                        Color = DiscordColor.Green,
                    }));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = strings.error,
                        Description = strings.anilist_profile_not_found,
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

            user ??= ctx.User;
            var miembro = await ctx.Guild.GetMemberAsync(user.Id);
            var userAnilist = await UsuariosAnilist.GetPerfilAsync(miembro.Id);
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
                            nsfw = strings.yes;
                        }
                        else
                        {
                            nsfw = strings.no;
                        }

                        string titulosStr = data.Data.User.options.titleLanguage;
                        string colorStr = data.Data.User.options.profileColor;
                        var animeStats = $"{strings.total}: `{data.Data.User.statistics.anime.count}`\n{strings.episodes}: `{data.Data.User.statistics.anime.episodesWatched}`\n{strings.mean_score}: `{data.Data.User.statistics.anime.meanScore}/100`";
                        var mangaStats = $"{strings.total}: `{data.Data.User.statistics.manga.count}`\n{strings.chapters}: `{data.Data.User.statistics.manga.chaptersRead}`\n{strings.mean_score}: `{data.Data.User.statistics.manga.meanScore}/100`";
                        var options = $"{strings.titles_language}: `{titulosStr.UppercaseFirst()}`\nNSFW: `{nsfw}`\n{strings.color}: `{colorStr.UppercaseFirst()}`";
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

                        builder.AddField(strings.anime_stats, animeStats, true);
                        builder.AddField(strings.manga_stats, mangaStats, true);
                        builder.AddField(strings.settings, options, true);
                        if (!string.IsNullOrEmpty(favoriteAnime)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":tv:")} {strings.favorite_animes}", favoriteAnime, true);
                        if (!string.IsNullOrEmpty(favoriteManga)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":book:")} {strings.favorite_mangas}", favoriteManga, true);
                        if (!string.IsNullOrEmpty(favoriteCharacters)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":bust_in_silhouette:")} {strings.favorite_characters}", favoriteCharacters, true);
                        if (!string.IsNullOrEmpty(favoriteStaff)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":man_artist:")} {strings.favorite_staff}", favoriteStaff, true);
                        if (!string.IsNullOrEmpty(favoriteStudios)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":minidisc:")} {strings.favorite_studios}", favoriteStudios, true);

                        DiscordLinkButtonComponent perfil = new($"{siteurl}", strings.profile, false, new DiscordComponentEmoji("👤"));
                        DiscordLinkButtonComponent animeList = new($"{siteurl}/animelist", strings.anime_list, false, new DiscordComponentEmoji("📺"));
                        DiscordLinkButtonComponent mangaList = new($"{siteurl}/mangalist", strings.manga_list, false, new DiscordComponentEmoji("📖"));
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder).AddComponents(perfil, animeList, mangaList));
                    }
                    else
                    {
                        if (data.Errors != null)
                        {
                            foreach (var x in data.Errors)
                            {
                                var msg = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{strings.error}: {x.Message}"));
                                await Task.Delay(10000);
                                await ctx.DeleteFollowupAsync(msg.Id);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var mensaje = ex.Message switch
                    {
                        "The HTTP request failed with status code NotFound" => $"{strings.anilist_profile_not_found}, {user.Mention}",
                        _ => $"{strings.unknown_error}, {strings.message}: [{ex.Message}"
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = strings.error,
                        Description = mensaje,
                        Color = DiscordColor.Red,
                    }));
                }
            }
            else
            {
                var builder = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red,
                    Title = strings.anilist_profile_not_found,
                    Description = $"{miembro.Mention}, {string.Format(strings.no_anilist_profile_vinculated, miembro.Mention)}.\n\n" +
                                $"{strings.to_vinculate_anilist_profile}: `/anilist setanilist`",
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
            }
        }

        [SlashCommand("anime", "Searchs for an anime")]
        public async Task Anime(InteractionContext ctx, [Option("Anime", "Anime to search")] string anime, [Option("User", "User's Anilist stats")] DiscordUser? usuario = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            usuario ??= ctx.User;
            var media = await AnilistServices.GetAniListMedia(ctx, ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGeneral), anime, MediaType.ANIME);
            if (media.Ok == true)
            {
                string titulos;
                if (media.Titulos != null)
                {
                    titulos = string.Join(", ", media.Titulos);
                }
                else
                {
                    titulos = strings.without_titles;
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

                    if (!string.IsNullOrEmpty(media.Episodios)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":1234:")} {strings.episodes}", Common.NormalizarField(media.Episodios), true);
                    if (!string.IsNullOrEmpty(media.Formato)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":dividers:")} {strings.format}", Common.NormalizarField(media.Formato), true);
                    if (!string.IsNullOrEmpty(media.Estado)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":hourglass_flowing_sand:")} {strings.status}", Common.NormalizarField(media.Estado.ToLower().ToUpperFirst()), true);
                    if (!string.IsNullOrEmpty(media.Score)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":star:")} {strings.score}", Common.NormalizarField(media.Score), false);
                    if (!string.IsNullOrEmpty(media.Fechas)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":calendar_spiral:")} {strings.start_date}", Common.NormalizarField(media.Fechas), false);
                    if (!string.IsNullOrEmpty(media.Generos)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":scroll:")} {strings.genres}", Common.NormalizarField(media.Generos), false);
                    if (!string.IsNullOrEmpty(media.Tags)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":notepad_spiral:")} {strings.genres}", Common.NormalizarField(media.Tags), false);
                    if (!string.IsNullOrEmpty(titulos)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":pencil:")} {strings.synonyms}", Common.NormalizarField(titulos), false);
                    if (!string.IsNullOrEmpty(media.Estudios)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":minidisc:")} {strings.studios}", Common.NormalizarField(media.Estudios), false);
                    if (!string.IsNullOrEmpty(media.LinksExternos)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":link:")} {strings.external_links}", Common.NormalizarField(media.LinksExternos), false);

                    DiscordWebhookBuilder whbuilder = new();
                    whbuilder.AddEmbed(builder);

                    var usuarioB = await UsuariosAnilist.GetPerfilAsync(usuario.Id);
                    if (usuarioB != null)
                    {
                        var embedN = await AnilistServices.GetInfoMediaUser(ctx, usuarioB.AnilistId, media.Id);
                        if (embedN != null)
                        {
                            whbuilder.AddEmbed(embedN);
                        }
                    }

                    await ctx.EditResponseAsync(whbuilder);
                }
                else
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(Constants.NsfwWarning));
                }
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(media.MsgError));
            }
        }

        [SlashCommand("manga", "Searchs for a manga")]
        public async Task Manga(InteractionContext ctx, [Option("Manga", "Manga to search")] string manga, [Option("User", "User's Anilist stats")] DiscordUser? usuario = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            usuario ??= ctx.User;
            var media = await AnilistServices.GetAniListMedia(ctx, ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGeneral), manga, MediaType.MANGA);
            if (media.Ok == true)
            {
                string titulos;
                if (media.Titulos != null)
                {
                    titulos = string.Join(", ", media.Titulos);
                }
                else
                {
                    titulos = strings.without_titles;
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

                    if (!string.IsNullOrEmpty(media.Chapters)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":1234:")} {strings.chapters}", Common.NormalizarField(media.Chapters), true);
                    if (!string.IsNullOrEmpty(media.Formato)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":dividers:")} {strings.format}", Common.NormalizarField(media.Formato), true);
                    if (!string.IsNullOrEmpty(media.Estado)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":hourglass_flowing_sand:")} {strings.status}", Common.NormalizarField(media.Estado.ToLower().ToUpperFirst()), true);
                    if (!string.IsNullOrEmpty(media.Score)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":star:")} {strings.score}", Common.NormalizarField(media.Score), true);
                    if (!string.IsNullOrEmpty(media.Fechas)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":calendar_spiral:")} {strings.publication_date}", Common.NormalizarField(media.Fechas), false);
                    if (!string.IsNullOrEmpty(media.Generos)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":scroll:")} {strings.genres}", Common.NormalizarField(media.Generos), false);
                    if (!string.IsNullOrEmpty(media.Tags)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":notepad_spiral:")} {strings.tags}", Common.NormalizarField(media.Tags), false);
                    if (!string.IsNullOrEmpty(titulos)) builder.AddField($"{DiscordEmoji.FromName(ctx.Client, ":pencil:")} {strings.synonyms}", Common.NormalizarField(titulos), false);

                    DiscordWebhookBuilder whbuilder = new();
                    whbuilder.AddEmbed(builder);

                    var usuarioB = await UsuariosAnilist.GetPerfilAsync(usuario.Id);
                    if (usuarioB != null)
                    {
                        var embedN = await AnilistServices.GetInfoMediaUser(ctx, usuarioB.AnilistId, media.Id);
                        if (embedN != null)
                        {
                            whbuilder.AddEmbed(embedN);
                        }
                    }

                    await ctx.EditResponseAsync(whbuilder);
                }
                else
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(Constants.NsfwWarning));
                }
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(media.MsgError));
            }
        }

        [SlashCommand("character", "Searchs for a Character")]
        public async Task Character(InteractionContext ctx, [Option("Character", "character to search")] string personaje)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
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
                                opcDesc = strings.without_animes_or_mangas;
                            }
                        }

                        opc.Add(new AnimeShort
                        {
                            Title = opcName,
                            Description = opcDesc,
                        });
                    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                    var elegido = await Common.GetElegidoAsync(ctx, ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGeneral), opc);
                    if (elegido > 0)
                    {
                        var datos = data.Data.Page.characters[elegido - 1];
                        string descripcion = datos.description;
                        descripcion = Common.NormalizarDescription(Common.LimpiarTexto(descripcion));
                        if (descripcion == string.Empty)
                        {
                            descripcion = strings.without_description;
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
                            Title = strings.error,
                            Description = $"{strings.character_not_found}: `{personaje}`",
                        }));
                    }
                    else
                    {
                        foreach (var x in data.Errors)
                        {
                            var msg = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"Error: {x.Message}"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var mensaje = ex.Message switch
                {
                    "The HTTP request failed with status code NotFound" => $"{strings.character_not_found}: `{personaje}`",
                    _ => $"{strings.unknown_error}, {strings.message}: {ex.Message}"
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(mensaje));
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
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{strings.processing_image}.."));
                    var response = await client.ExecuteAsync(request);
                    if (response.IsSuccessful)
                    {
                        if (response.Content != null)
                        {
                            var resp = JsonConvert.DeserializeObject<dynamic>(response.Content);
                            var resultados = string.Empty;
                            var titulo = $"{strings.the_possible_anime_is}:";
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
                                        var media = await AnilistServices.GetAniListMediaTitleAndNsfwFromId(ctx, id, MediaType.ANIME);
                                        string mediaTitle = media.Item1;
                                        bool nsfw = media.Item2;
                                        int from = resultado.from;
                                        string videoLink = resultado.video;
                                        if (!ctx.Channel.IsNSFW && nsfw)
                                        {
                                            msg = $"{strings.image_from_nsfw_anime}, {strings.use_command_in_nsfw_channel}";
                                        }
                                        resultados =
                                            $"{Formatter.Bold($"{strings.name}:")} [{mediaTitle}]({enlace += id})\n" +
                                            $"{Formatter.Bold($"{strings.similarity}:")} {similaridad}%\n" +
                                            $"{Formatter.Bold($"{strings.episode}:")} {resultado.episode} ({strings.minute}: {TimeSpan.FromSeconds(from):mm\\:ss}\n" +
                                            $"{Formatter.Bold($"{strings.video}:")} [{strings.link}]({videoLink})";
                                        break;
                                    }
                                }

                                if (!encontro)
                                {
                                    titulo = strings.no_results_found_image;
                                    resultados = strings.sauce_remember;
                                }

                                var embed = new DiscordEmbedBuilder
                                {
                                    Title = titulo,
                                    Description = resultados,
                                    ImageUrl = url,
                                    Color = Constants.YumikoColor,
                                };
                                embed.WithFooter($"{strings.retrieved_from} trace.moe", "https://trace.moe/favicon.png");
                                if (msg == "OK")
                                {
                                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                                    return;
                                }
                            }
                        }
                        else
                        {
                            msg = strings.unknown_error;
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
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(strings.unknown_error_tracemoe));
                        return;
                    }
                }
                else
                {
                    msg = strings.image_format_error;
                }
            }
            else
            {
                msg = strings.image_must_enter_link;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder()
            {
                Title = strings.error,
                Description = msg,
                Color = DiscordColor.Red,
            }));
        }

        [SlashCommand("pj", "Random character")]
        public async Task Pj(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var pag = Common.GetNumeroRandom(1, 10000);
            var personaje = await Common.GetRandomCharacterAsync(ctx, pag);
            if (personaje != null)
            {
                var corazon = DiscordEmoji.FromName(ctx.Client, ":heart:");
                var builder = new DiscordEmbedBuilder
                {
                    Title = personaje.NameFull,
                    Url = personaje.SiteUrl,
                    ImageUrl = personaje.Image,
                    Description = $"[{personaje.AnimePrincipal?.TitleRomaji}]({personaje.AnimePrincipal?.SiteUrl})\n{personaje.Favoritos} {corazon} (nº {pag} {strings.in_popularity_rank})",
                    Color = Constants.YumikoColor
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
            }
        }
    }
}
