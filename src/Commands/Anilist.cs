namespace Yumiko.Commands
{
    using GraphQL;
    using GraphQL.Client.Http;
    using GraphQL.Client.Serializer.Newtonsoft;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using RestSharp;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web;

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
        [NameLocalization(Localization.Spanish, "asignarperfil")]
        [DescriptionLocalization(Localization.Spanish, "Asigna tu perfil de AniList")]
        public async Task SetAnilist(InteractionContext ctx)
        {
            string anilistApplicationId = ConfigurationUtils.GetConfiguration<string>(Configuration, Configurations.AnilistApiClientId);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .AsEphemeral(true)
                .AddEmbed(new DiscordEmbedBuilder
                {
                    Title = translations.setup_anilist_profile,
                    Description =
                        $"{translations.anilist_setprofile_instructions}:\n\n" +
                        $"{translations.anilist_setprofile_instructions_1}\n" +
                        $"{translations.anilist_setprofile_instructions_2}\n" +
                        $"{translations.anilist_setprofile_instructions_3}\n" +
                        $"{translations.anilist_setprofile_instructions_4}",
                    Color = Constants.YumikoColor
                })
                .AddComponents(
                    new DiscordLinkButtonComponent($"https://anilist.co/api/v2/oauth/authorize?client_id={anilistApplicationId}&response_type=token", translations.authorize),
                    new DiscordButtonComponent(ButtonStyle.Primary, $"modal-anilistprofileset-{ctx.User.Id}", translations.paste_code_here)
                )
            );

            DiscordMessage message = await ctx.GetOriginalResponseAsync();
            var interactivity = ctx.Client.GetInteractivity();
            var interactivityBtnResult = await interactivity.WaitForButtonAsync(message, TimeSpan.FromMinutes(5));

            if (!interactivityBtnResult.TimedOut)
            {
                var btnInteraction = interactivityBtnResult.Result.Interaction;
                string modalId = $"modal-{btnInteraction.Id}";

                var modal = new DiscordInteractionResponseBuilder()
                    .WithCustomId(modalId)
                    .WithTitle(translations.set_anilist_profile)
                    .AddComponents(new TextInputComponent(label: translations.code, placeholder: translations.paste_code_here, customId: "AniListToken"));

                await btnInteraction.CreateResponseAsync(InteractionResponseType.Modal, modal);

                var interactivityModalResult = await interactivity.WaitForModalAsync(modalId, TimeSpan.FromMinutes(5));

                if (!interactivityModalResult.TimedOut)
                {
                    var modalInteraction = interactivityModalResult.Result.Interaction;
                    string ALToken = interactivityModalResult.Result.Values.First().Value;

                    await modalInteraction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
                    var profile = await ViewerQuery.GetProfile(ctx, ALToken);
                    if (profile != null)
                    {
                        await UsuariosAnilist.SetAnilistAsync(profile.Id, ctx.Member.Id);
                        var embed = AnilistUtils.GetLoggedProfileEmbed(ctx, profile);
                        await modalInteraction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AsEphemeral(false).AddEmbed(embed: embed));
                        return;
                    }
                    else
                    {
                        await modalInteraction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AsEphemeral(true).AddEmbed(new DiscordEmbedBuilder
                        {
                            Title = translations.error,
                            Description = translations.unknown_error,
                            Color = DiscordColor.Red
                        }));
                    }
                }
                else
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = translations.response_timed_out,
                        Color = DiscordColor.Red
                    }));
                }
            }
            else
            {
                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = translations.response_timed_out,
                    Color = DiscordColor.Red
                }));
            }
        }

        [SlashCommand("deleteprofile", "Deletes your AniList profile")]
        [NameLocalization(Localization.Spanish, "eliminarperfil")]
        [DescriptionLocalization(Localization.Spanish, "Elimina tu perfil de AniList")]
        public async Task DeleteAnilist(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            var confirmar = await Common.GetYesNoInteractivityAsync(ctx, ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGeneral), ctx.Client.GetInteractivity(), translations.confirm_delete_profile, translations.action_cannont_be_undone);
            if (confirmar)
            {
                var borrado = await UsuariosAnilist.DeleteAnilistAsync(ctx.User.Id);
                if (borrado)
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = translations.success,
                        Description = translations.anilist_profile_deleted_successfully,
                        Color = DiscordColor.Green,
                    }));
                }
                else
                {
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = translations.error,
                        Description = translations.anilist_profile_not_found,
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
        [NameLocalization(Localization.Spanish, "perfil")]
        [DescriptionLocalization(Localization.Spanish, "Busca un perfil de AniList")]
        public async Task Profile(InteractionContext ctx, [Option("Member", "Member whose Anilist profile you want to see")] DiscordUser user)
        {
            await ctx.DeferAsync();
            user ??= ctx.User;

            var userAnilistDb = await UsuariosAnilist.GetPerfilAsync(user.Id);
            if (userAnilistDb != null)
            {
                var anilistUser = await ProfileQuery.GetProfile(ctx, userAnilistDb.AnilistId);
                if (anilistUser != null)
                {
                    DiscordEmbedBuilder builder = AnilistUtils.GetProfileEmbed(ctx.Client, anilistUser);
                    DiscordLinkButtonComponent profile = new($"{anilistUser.SiteUrl}", translations.profile, false, new DiscordComponentEmoji("👤"));
                    DiscordLinkButtonComponent animeList = new($"{anilistUser.SiteUrl}/animelist", translations.anime_list, false, new DiscordComponentEmoji("📺"));
                    DiscordLinkButtonComponent mangaList = new($"{anilistUser.SiteUrl}/mangalist", translations.manga_list, false, new DiscordComponentEmoji("📖"));

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder).AddComponents(profile, animeList, mangaList));
                    return;
                }
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
                Title = translations.anilist_profile_not_found,
                Description = $"{string.Format(translations.no_anilist_profile_vinculated, user.Mention)}.\n\n" +
                                $"{translations.to_vinculate_anilist_profile}: `/anilist setanilist`",
            }));
        }

        [ContextMenu(ApplicationCommandType.UserContextMenu, "AniList Profile")]
        [NameLocalization(Localization.Spanish, "Perfil de AniList")]
        public async Task UserProfile(ContextMenuContext ctx)
        {
            await ctx.DeferAsync(true);

            var userAnilistDb = await UsuariosAnilist.GetPerfilAsync(ctx.TargetUser.Id);
            if (userAnilistDb != null)
            {
                var anilistUser = await ProfileQuery.GetProfile(ctx, userAnilistDb.AnilistId);
                if (anilistUser != null)
                {
                    DiscordEmbedBuilder builder = AnilistUtils.GetProfileEmbed(ctx.Client, anilistUser);
                    DiscordLinkButtonComponent profile = new($"{anilistUser.SiteUrl}", translations.profile, false, new DiscordComponentEmoji("👤"));
                    DiscordLinkButtonComponent animeList = new($"{anilistUser.SiteUrl}/animelist", translations.anime_list, false, new DiscordComponentEmoji("📺"));
                    DiscordLinkButtonComponent mangaList = new($"{anilistUser.SiteUrl}/mangalist", translations.manga_list, false, new DiscordComponentEmoji("📖"));

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder).AddComponents(profile, animeList, mangaList));
                    return;
                }
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
                Title = translations.anilist_profile_not_found,
                Description = $"{string.Format(translations.no_anilist_profile_vinculated, ctx.User.Mention)}.\n\n" +
                                $"{translations.to_vinculate_anilist_profile}: `/anilist setanilist`",
            }));
        }

        [SlashCommand("anime", "Searchs for an anime")]
        [DescriptionLocalization(Localization.Spanish, "Busca un anime")]
        public async Task Anime(InteractionContext ctx, [Option("Anime", "Anime to search")] string anime, [Option("User", "User's Anilist stats")] DiscordUser? usuario = null)
        {
            await ctx.DeferAsync();
            usuario ??= ctx.User;

            var media = await MediaQuery.GetMedia(ctx, ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGeneral), anime, MediaType.ANIME);
            if (media != null)
            {
                var builder = new DiscordWebhookBuilder();

                if (media.IsAdult && !ctx.Channel.IsNSFW)
                {
                    await ctx.EditResponseAsync(builder.AddEmbed(Common.NsfwWarning));
                    return;
                }

                builder.AddEmbed(AnilistUtils.GetMediaEmbed(ctx, media, MediaType.ANIME));

                var userAnilistProfile = await UsuariosAnilist.GetPerfilAsync(usuario.Id);
                if (userAnilistProfile != null)
                {
                    var statsUser = await MediaUserQuery.GetMediaFromUser(ctx, userAnilistProfile.AnilistId, media.Id);
                    if (statsUser != null)
                    {
                        builder.AddEmbed(AnilistUtils.GetMediaUserStatsEmbed(statsUser));
                    }
                }

                await ctx.EditResponseAsync(builder);
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(Common.ResourceNotFound("Anime")));
            }
        }

        [SlashCommand("manga", "Searchs for a manga")]
        [DescriptionLocalization(Localization.Spanish, "Busca un manga")]
        public async Task Manga(InteractionContext ctx, [Option("Manga", "Manga to search")] string manga, [Option("User", "User's Anilist stats")] DiscordUser? usuario = null)
        {
            await ctx.DeferAsync();
            usuario ??= ctx.User;

            var media = await MediaQuery.GetMedia(ctx, ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGeneral), manga, MediaType.MANGA);
            if (media != null)
            {
                var builder = new DiscordWebhookBuilder();

                if (media.IsAdult && !ctx.Channel.IsNSFW)
                {
                    await ctx.EditResponseAsync(builder.AddEmbed(Common.NsfwWarning));
                    return;
                }

                builder.AddEmbed(AnilistUtils.GetMediaEmbed(ctx, media, MediaType.ANIME));

                var userAnilistProfile = await UsuariosAnilist.GetPerfilAsync(usuario.Id);
                if (userAnilistProfile != null)
                {
                    var statsUser = await MediaUserQuery.GetMediaFromUser(ctx, userAnilistProfile.AnilistId, media.Id);
                    if (statsUser != null)
                    {
                        builder.AddEmbed(AnilistUtils.GetMediaUserStatsEmbed(statsUser));
                    }
                }

                await ctx.EditResponseAsync(builder);
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(Common.ResourceNotFound("Manga")));
            }
        }

        [SlashCommand("character", "Searchs for a Character")]
        [NameLocalization(Localization.Spanish, "personaje")]
        [DescriptionLocalization(Localization.Spanish, "Busca un personaje")]
        public async Task Character(InteractionContext ctx, [Option("Character", "Character to search")] string search)
        {
            await ctx.DeferAsync();

            var character = await CharacterQuery.GetCharacter(ctx, ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGeneral), search);
            if (character != null)
            {
                var embed = AnilistUtils.GetCharacterEmbed(ctx, character);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(Common.ResourceNotFound("Character")));
            }
        }

        [SlashCommand("staff", "Searchs for a staff")]
        [DescriptionLocalization(Localization.Spanish, "Busca a un staff")]
        public async Task Staff(InteractionContext ctx, [Option("Staff", "Staff to search")] string search)
        {
            await ctx.DeferAsync();

            var staff = await StaffQuery.GetStaff(ctx, ConfigurationUtils.GetConfiguration<double>(Configuration, Configurations.TimeoutGeneral), search);
            if (staff != null)
            {
                var embed = AnilistUtils.GetStaffEmbed(staff);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(Common.ResourceNotFound("Staff")));
            }
        }

        [SlashCommand("sauce", "Searchs for the anime of an image")]
        [DescriptionLocalization(Localization.Spanish, "Busca el anime de una imágen")]
        public async Task Sauce(InteractionContext ctx, [Option("Image", "Image link")] string url)
        {
            await ctx.DeferAsync();
            var msg = "OK";
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                var extension = url[^4..];
                if (extension == ".jpg" || extension == ".png" || extension == "jpeg")
                {
                    var client = new RestClient("https://api.trace.moe/search?url=" + url);
                    var request = new RestRequest();
                    request.AddHeader("content-type", "application/json");
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().WithContent($"{translations.processing_image}.."));
                    var response = await client.ExecuteAsync(request);
                    if (response.IsSuccessful)
                    {
                        if (response.Content != null)
                        {
                            var resp = JsonConvert.DeserializeObject<dynamic>(response.Content);
                            var resultados = string.Empty;
                            var titulo = $"{translations.the_possible_anime_is}:";
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
                                        Media? media = await MediaQuery.GetMedia(ctx, id, MediaType.ANIME);
                                        string mediaTitle = media!.Title.Romaji;
                                        bool nsfw = media!.IsAdult;
                                        int from = resultado.from;
                                        string videoLink = resultado.video;
                                        if (!ctx.Channel.IsNSFW && nsfw)
                                        {
                                            msg = $"{translations.image_from_nsfw_anime}, {translations.use_command_in_nsfw_channel}";
                                        }
                                        resultados =
                                            $"{Formatter.Bold($"{translations.name}:")} [{mediaTitle}]({enlace += id})\n" +
                                            $"{Formatter.Bold($"{translations.similarity}:")} {similaridad}%\n" +
                                            $"{Formatter.Bold($"{translations.episode}:")} {resultado.episode} ({translations.minute}: {TimeSpan.FromSeconds(from):mm\\:ss}\n" +
                                            $"{Formatter.Bold($"{translations.video}:")} [{translations.link}]({videoLink})";
                                        break;
                                    }
                                }

                                if (!encontro)
                                {
                                    titulo = translations.no_results_found_image;
                                    resultados = translations.sauce_remember;
                                }

                                var embed = new DiscordEmbedBuilder
                                {
                                    Title = titulo,
                                    Description = resultados,
                                    ImageUrl = url,
                                    Color = Constants.YumikoColor,
                                };
                                embed.WithFooter($"{translations.retrieved_from} trace.moe", "https://trace.moe/favicon.png");
                                if (msg == "OK")
                                {
                                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                                    return;
                                }
                            }
                        }
                        else
                        {
                            msg = translations.unknown_error;
                        }
                    }
                    else
                    {
                        msg = response.StatusCode switch
                        {
                            HttpStatusCode.BadRequest => "Invalid image url",
                            HttpStatusCode.PaymentRequired => "Search quota depleted / Concurrency limit exceeded",
                            HttpStatusCode.Forbidden => "Invalid API key",
                            HttpStatusCode.MethodNotAllowed => "Method Not Allowed",
                            HttpStatusCode.InternalServerError => "Internal Server Error",
                            HttpStatusCode.ServiceUnavailable => "Search queue is full / Database is not responding",
                            HttpStatusCode.GatewayTimeout => "Server is overloaded",
                            _ => "Unknown error",
                        };
                        await Common.GrabarLogErrorAsync(ctx.Guild, ctx.Channel, "Error retriving image from trace.moe with `sauce` command.\nError: " + msg);
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(translations.unknown_error_tracemoe));
                        return;
                    }
                }
                else
                {
                    msg = translations.image_format_error;
                }
            }
            else
            {
                msg = translations.image_must_enter_link;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder()
            {
                Title = translations.error,
                Description = msg,
                Color = DiscordColor.Red,
            }));
        }

        [SlashCommand("pj", "Random character")]
        [DescriptionLocalization(Localization.Spanish, "Personaje aleatorio")]
        public async Task Pj(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            var page = Common.GetRandomNumber(1, 10000);
            var character = await RandomCharacterQuery.GetCharacter(ctx, page);
            if (character != null)
            {
                var embed = AnilistUtils.GetRandomCharacterEmbed(ctx, character, page);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            }
        }

        [SlashCommand("recommendations", "Auto recommendations based on your list")]
        [NameLocalization(Localization.Spanish, "recomendaciones")]
        [DescriptionLocalization(Localization.Spanish, "Recomendaciones automáticas basada en tu lista")]
        public async Task AutoRecomendation(
            InteractionContext ctx,
            [Option("Type", "The type of media")] MediaType type,
            [Option("User", "The user's recommendation to retrieve")] DiscordUser? user = null)
        {
            await ctx.DeferAsync();
            user ??= ctx.User;
            var userAnilist = await UsuariosAnilist.GetPerfilAsync(user.Id);
            if (userAnilist != null)
            {
                var recommendations = await RecommendatiosnQuery.GetRecommendations(ctx.Guild, ctx.Channel, userAnilist.AnilistId, type);
                if (recommendations.Item1 != null && recommendations.Item2 != null)
                {
                    var embed = AnilistUtils.GetMediaRecommendationsEmbed(user, recommendations.Item1, recommendations.Item2, type);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = translations.error,
                        Description = translations.unknown_error,
                        Color = DiscordColor.Red
                    }));
                }
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = translations.error,
                    Description = translations.anilist_profile_not_found,
                    Color = DiscordColor.Red
                }));
            }
        }

        [ContextMenu(ApplicationCommandType.UserContextMenu, "Anime Recommendations")]
        [NameLocalization(Localization.Spanish, "Recomendaciones de anime")]
        public async Task AnimeUserRecomendation(ContextMenuContext ctx)
        {
            await ctx.DeferAsync(ctx.User.Id == ctx.TargetUser.Id);
            var userAnilist = await UsuariosAnilist.GetPerfilAsync(ctx.TargetUser.Id);
            if (userAnilist != null)
            {
                var recommendations = await RecommendatiosnQuery.GetRecommendations(ctx.Guild, ctx.Channel, userAnilist.AnilistId, MediaType.ANIME);
                if (recommendations.Item1 != null && recommendations.Item2 != null)
                {
                    var embed = AnilistUtils.GetMediaRecommendationsEmbed(ctx.TargetUser, recommendations.Item1, recommendations.Item2, MediaType.ANIME);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = translations.error,
                        Description = translations.unknown_error,
                        Color = DiscordColor.Red
                    }));
                }
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = translations.error,
                    Description = translations.anilist_profile_not_found,
                    Color = DiscordColor.Red
                }));
            }
        }

        [ContextMenu(ApplicationCommandType.UserContextMenu, "Manga Recommendations")]
        [NameLocalization(Localization.Spanish, "Recomendaciones de manga")]
        public async Task MangaUserRecomendation(ContextMenuContext ctx)
        {
            await ctx.DeferAsync(ctx.User.Id == ctx.TargetUser.Id);
            var userAnilist = await UsuariosAnilist.GetPerfilAsync(ctx.TargetUser.Id);
            if (userAnilist != null)
            {
                var recommendations = await RecommendatiosnQuery.GetRecommendations(ctx.Guild, ctx.Channel, userAnilist.AnilistId, MediaType.MANGA);
                if (recommendations.Item1 != null && recommendations.Item2 != null)
                {
                    var embed = AnilistUtils.GetMediaRecommendationsEmbed(ctx.TargetUser, recommendations.Item1, recommendations.Item2, MediaType.MANGA);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = translations.error,
                        Description = translations.unknown_error,
                        Color = DiscordColor.Red
                    }));
                }
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = translations.error,
                    Description = translations.anilist_profile_not_found,
                    Color = DiscordColor.Red
                }));
            }
        }

        [SlashCommand("anitheme", "Searchs for anime openings and endings")]
        public async Task AnimeOpening(InteractionContext ctx, [Option("Anime", "Anime that you want to search openings and endings")] string anime)
        {
            await ctx.DeferAsync();

            string animeSearch = HttpUtility.UrlEncode(anime);

            var video = await SearchQuery.Search(ctx, 30, animeSearch);

            if (video != null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
                    $"{translations.link_for} {Formatter.Bold(video.Filename)}:\n\n" +
                    $"{video.Link}\n"
                    ));
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Red)
                    .WithTitle(translations.resource_not_found)
                    ));
            }
        }
    }
}
