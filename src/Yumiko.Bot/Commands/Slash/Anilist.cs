using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.Localization;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using Yumiko.Application.Anilist;
using Yumiko.Application.Helpers;
using Yumiko.Bot.Commands.Framework;
using Yumiko.Bot.Commands.Framework.Attributes;
using Yumiko.Bot.Commands.Framework.Choices;
using Yumiko.Bot.Configuration;
using Yumiko.Bot.Extensions;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services;
using Yumiko.Model.Entities;
using Yumiko.Model.Entities.Anilist;
using Yumiko.Model.Entities.AnimeThemes;
using Yumiko.Model.Enum;
using Yumiko.Model.Exceptions;
using Yumiko.Model.Interfaces;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Bot.Commands.Slash;

[TestCommand]
[Command("anilist")]
[Description("Anilist queries")]
public sealed class Anilist(
    ILocalizer localizer,
    DiscordBotService discordBotService,
    BotConfiguration config,
    GamesSettings gamesSettings,
    TimeoutSettings timeouts,
    IAnilistClient anilist,
    IAnilistUsersRepository anilistUsers,
    ITraceMoeClient traceMoe,
    AnithemeSelector anithemeSelector,
    AnilistResponses responses,
    DiscordInteractivity discordInteractivity,
    InteractivityExtension interactivity,
    DiscordLogService logService)
{
    /// <summary>Below this similarity percentage trace.moe is not reliable.</summary>
    private const int MinimumSimilarity = 87;

    private const int EntriesPerPage = 25;

    [Command("setprofile")]
    [Description("Sets your AniList profile")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task SetProfileAsync(SlashCommandContext ctx)
    {
        Loc loc = ctx.Loc(localizer);

        await ctx.RespondAsync(new DiscordInteractionResponseBuilder()
            .AsEphemeral()
            .AddEmbed(new DiscordEmbedBuilder
            {
                Title = loc[Keys.setup_anilist_profile],
                Description =
                    $"{loc[Keys.anilist_setprofile_instructions]}:\n\n" +
                    $"{loc[Keys.anilist_setprofile_instructions_1]}\n" +
                    $"{loc[Keys.anilist_setprofile_instructions_2]}\n" +
                    $"{loc[Keys.anilist_setprofile_instructions_3]}\n" +
                    $"{loc[Keys.anilist_setprofile_instructions_4]}",
                Color = YumikoColors.Primary,
            })
            .AddActionRowComponent(
                new DiscordLinkButtonComponent(
                    $"https://anilist.co/api/v2/oauth/authorize?client_id={config.AnilistApiClientId}&response_type=token",
                    loc[Keys.authorize]),
                new DiscordButtonComponent(DiscordButtonStyle.Primary, $"modal-anilistprofileset-{ctx.User.Id}", loc[Keys.paste_code_here])));

        DiscordMessage message = await ctx.GetResponseAsync();
        InteractivityResult<ComponentInteractionCreatedEventArgs> button =
            await interactivity.WaitForButtonAsync(message, TimeSpan.FromMinutes(5));

        if (button.TimedOut)
        {
            await NotifyTimeoutAsync(ctx, loc);
            return;
        }

        DiscordInteraction buttonInteraction = button.Result.Interaction;
        string modalId = $"modal-{buttonInteraction.Id}";

        await buttonInteraction.CreateResponseAsync(DiscordInteractionResponseType.Modal, new DiscordModalBuilder()
            .WithCustomId(modalId)
            .WithTitle(loc[Keys.set_anilist_profile])
            .AddTextInput(
                new DiscordTextInputComponent("AniListToken", placeholder: loc[Keys.paste_code_here]),
                loc[Keys.code]));

        InteractivityResult<ModalSubmittedEventArgs> modal = await interactivity.WaitForModalAsync(modalId, TimeSpan.FromMinutes(5));

        if (modal.TimedOut)
        {
            await NotifyTimeoutAsync(ctx, loc);
            return;
        }

        DiscordInteraction modalInteraction = modal.Result.Interaction;
        await modalInteraction.CreateResponseAsync(DiscordInteractionResponseType.DeferredChannelMessageWithSource);

        User? profile = await anilist.GetViewerAsync(modal.Result.TextOf("AniListToken"));

        if (profile is null)
        {
            await modalInteraction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .AsEphemeral()
                .AddEmbed(ErrorEmbed.Unknown(loc)));
            return;
        }

        await anilistUsers.SetAnilistAsync(profile.Id, ctx.User.Id);

        await modalInteraction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
            .AddEmbed(AnilistEmbeds.LoggedProfile(profile, ctx.User, loc)));
    }

    [Command("deleteprofile")]
    [Description("Deletes your AniList profile")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task DeleteProfileAsync(SlashCommandContext ctx)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await PrepareAsync(ctx, loc))
        {
            return;
        }

        if (!await discordInteractivity.ConfirmAsync(ctx, loc[Keys.confirm_delete_profile], loc[Keys.action_cannont_be_undone], loc))
        {
            await ctx.DeleteResponseAsync();
            return;
        }

        bool deleted = await anilistUsers.DeleteAnilistAsync(ctx.User.Id);

        await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(deleted
            ? new DiscordEmbedBuilder
            {
                Title = loc[Keys.success],
                Description = loc[Keys.anilist_profile_deleted_successfully],
                Color = DiscordColor.Green,
            }
            : ErrorEmbed.Create(loc[Keys.error], loc[Keys.anilist_profile_not_found])));
    }

    [Command("profile")]
    [Description("Searchs for an AniList profile")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task ProfileAsync(
        SlashCommandContext ctx,
        [Parameter("member")] [Description("Member whose Anilist profile you want to see")] DiscordUser? user = null)
    {
        Loc loc = ctx.Loc(localizer);
        DiscordUser target = user ?? ctx.User;

        if (!await PrepareAsync(ctx, loc))
        {
            return;
        }
        await ctx.EditResponseAsync(await responses.ProfileAsync(target, loc));
    }

    [Command("anime")]
    [Description("Searchs for an anime")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public Task AnimeAsync(
        SlashCommandContext ctx,
        [Parameter("anime")] [Description("Anime to search")] string anime,
        [Parameter("user")] [Description("User's Anilist stats")] DiscordUser? user = null) =>
        SearchMediaResponseAsync(ctx, anime, MediaType.ANIME, user);

    [Command("manga")]
    [Description("Searchs for a manga")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public Task MangaAsync(
        SlashCommandContext ctx,
        [Parameter("manga")] [Description("Manga to search")] string manga,
        [Parameter("user")] [Description("User's Anilist stats")] DiscordUser? user = null) =>
        SearchMediaResponseAsync(ctx, manga, MediaType.MANGA, user);

    [Command("character")]
    [Description("Searchs for a Character")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task CharacterAsync(
        SlashCommandContext ctx,
        [Parameter("character")] [Description("Character to search")] string search)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await PrepareAsync(ctx, loc))
        {
            return;
        }

        List<Character> results = await anilist.SearchCharacterAsync(search, gamesSettings.AnilistPerPage);
        Character? character = await ChooseAsync(ctx, results, CharacterDescription, loc);

        await ctx.EditResponseAsync(character is null
            ? ErrorEmbed.NotFound(loc, "Character")
            : AnilistEmbeds.Character(character));
    }

    [Command("staff")]
    [Description("Searchs for a staff")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task StaffAsync(
        SlashCommandContext ctx,
        [Parameter("staff")] [Description("Staff to search")] string search)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await PrepareAsync(ctx, loc))
        {
            return;
        }

        List<Staff> results = await anilist.SearchStaffAsync(search, gamesSettings.AnilistPerPage);
        Staff? staff = await ChooseAsync(ctx, results, s => new TitleDescription { Title = s.Name.Full }, loc);

        await ctx.EditResponseAsync(staff is null
            ? ErrorEmbed.NotFound(loc, "Staff")
            : AnilistEmbeds.Staff(staff, loc));
    }

    [Command("pj")]
    [Description("Random character")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task RandomCharacterAsync(SlashCommandContext ctx)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await PrepareAsync(ctx, loc))
        {
            return;
        }

        int page = RandomHelper.GetRandomNumber(1, gamesSettings.RandomPageMax);
        Character? character = await anilist.GetRandomCharacterAsync(page);

        await ctx.EditResponseAsync(character is null
            ? ErrorEmbed.NotFound(loc, "Character")
            : AnilistEmbeds.RandomCharacter(character, page, loc));
    }

    [Command("sauce")]
    [Description("Searchs for the anime of an image")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task SauceAsync(
        SlashCommandContext ctx,
        [Parameter("image")] [Description("Image link")] string url)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await PrepareAsync(ctx, loc))
        {
            return;
        }

        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
        {
            await ctx.EditResponseAsync(ErrorEmbed.Create(loc[Keys.error], loc[Keys.image_must_enter_link]));
            return;
        }

        if (url.Length < 4 || url[^4..] is not (".jpg" or ".png" or "jpeg"))
        {
            await ctx.EditResponseAsync(ErrorEmbed.Create(loc[Keys.error], loc[Keys.image_format_error]));
            return;
        }

        await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().WithContent($"{loc[Keys.processing_image]}.."));

        List<TraceMoeMatch> results;
        try
        {
            results = await traceMoe.SearchAsync(url);
        }
        catch (TraceMoeImageFetchException)
        {
            // The link is the problem, not the bot: there is nothing to log.
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(loc[Keys.image_could_not_be_downloaded]));
            return;
        }
        catch (Exception ex)
        {
            string detail = ex is TraceMoeQuotaException quota
                ? $"HTTP {quota.StatusCode}: search quota depleted / concurrency limit exceeded"
                : ex.Message;
            await logService.LogErrorAsync(ctx.Guild, ctx.Channel, $"Error retriving image from trace.moe with `sauce` command.\nError: {detail}");
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(loc[Keys.unknown_error_tracemoe]));
            return;
        }

        string title = loc[Keys.no_results_found_image];
        string description = loc[Keys.sauce_remember];

        foreach (TraceMoeMatch result in results)
        {
            int similarity = (int)(result.Similarity * 100);
            if (similarity < MinimumSimilarity)
            {
                continue;
            }

            Media? media = await anilist.GetMediaAsync(result.AnilistId, MediaType.ANIME, gamesSettings.AnilistPerPage);
            if (media is null)
            {
                break;
            }

            if (media.IsAdult && !ctx.Channel.IsNSFW)
            {
                await ctx.EditResponseAsync(ErrorEmbed.Create(
                    loc[Keys.error],
                    $"{loc[Keys.image_from_nsfw_anime]}, {loc[Keys.use_command_in_nsfw_channel]}"));
                return;
            }

            title = $"{loc[Keys.the_possible_anime_is]}:";
            description =
                $"{Formatter.Bold($"{loc[Keys.name]}:")} [{media.Title.Romaji}](https://anilist.co/anime/{result.AnilistId})\n" +
                $"{Formatter.Bold($"{loc[Keys.similarity]}:")} {similarity}%\n" +
                $"{Formatter.Bold($"{loc[Keys.episode]}:")} {result.Episode} ({loc[Keys.minute]}: {TimeSpan.FromSeconds(result.From):mm\\:ss})\n" +
                $"{Formatter.Bold($"{loc[Keys.video]}:")} [{loc[Keys.link]}]({result.Video})";
            break;
        }

        await ctx.EditResponseAsync(new DiscordEmbedBuilder
        {
            Title = title,
            Description = description,
            ImageUrl = url,
            Color = YumikoColors.Primary,
        }.WithFooter($"{loc[Keys.retrieved_from]} trace.moe", "https://trace.moe/favicon.png"));
    }

    [Command("anitheme")]
    [Description("Searchs for anime openings and endings")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task AnithemeAsync(
        SlashCommandContext ctx,
        [Parameter("anime")] [Description("Anime that you want to search openings and endings")] string anime)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await PrepareAsync(ctx, loc))
        {
            return;
        }

        AnithemeData? data = await anithemeSelector.SearchAsync(ctx, anime, loc);

        if (data is null)
        {
            await ctx.EditResponseAsync(new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Red)
                .WithTitle(loc[Keys.resource_not_found]));
            return;
        }

        string typeName = data.theme.Type.ToLowerInvariant() switch
        {
            "op" => "Opening",
            "ed" => "Ending",
            _ => data.theme.Type,
        };

        if (data.theme.Sequence is not null)
        {
            typeName += $" {data.theme.Sequence}";
        }

        string description = $"## **{data.anime.Name}** ({data.anime.Season} {data.anime.Year})\n- {typeName}\n";

        if (data.song.Version is not null)
        {
            description += $"- Version: {data.song.Version}\n";
        }

        if (!string.IsNullOrEmpty(data.song.Episodes))
        {
            description += $"- {loc[Keys.episodes]}: {data.song.Episodes}\n";
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{description}\n[Link]({data.video.Link})"));
    }

    [Command("recommendations")]
    [Description("Auto recommendations based on your list")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task RecommendationsAsync(
        SlashCommandContext ctx,
        [Parameter("type")] [Description("The type of media")] MediaTypeChoice type,
        [Parameter("user")] [Description("The user's recommendation to retrieve")] DiscordUser? user = null)
    {
        Loc loc = ctx.Loc(localizer);
        DiscordUser target = user ?? ctx.User;

        if (!await PrepareAsync(ctx, loc))
        {
            return;
        }
        await ctx.EditResponseAsync(await responses.RecommendationsAsync(target, type.ToModel(), loc));
    }

    [Command("animelist")]
    [Description("Searchs a user's anime list")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task AnimeListAsync(
        SlashCommandContext ctx,
        [Parameter("user")] [Description("User to search")] DiscordUser user,
        [Parameter("status")] [Description("Status of the animes")] MediaUserStatusChoice statusChoice,
        [Parameter("sorting")] [Description("Sort the list by something")] MediaUserSortChoice orderChoice,
        [Parameter("language")] [Description("Language of the titles to be shown")] MediaTitleTypeChoice mediaTitleChoice)
    {
        Loc loc = ctx.Loc(localizer);

        MediaUserStatus status = statusChoice.ToModel();
        MediaTitleType language = mediaTitleChoice.ToModel();

        if (!await PrepareAsync(ctx, loc))
        {
            return;
        }

        AnilistUserLink? link = await anilistUsers.GetLinkAsync(user.Id);
        User? profile = link is null ? null : await anilist.GetProfileAsync(link.AnilistId);

        if (link is null || profile is null)
        {
            await ctx.EditResponseAsync(ErrorEmbed.Create(loc[Keys.error], loc[Keys.anilist_profile_not_found]));
            return;
        }

    // TODO: support manga; today the command only lists anime.
        MediaUserList? mediaList = await anilist.GetMediaListsAsync(link.AnilistId, status, orderChoice.ToModel(), language, MediaType.ANIME);

        if (mediaList?.Entries is null || mediaList.Entries.Count == 0)
        {
            await ctx.EditResponseAsync(ErrorEmbed.Create(loc[Keys.error], loc[Keys.resource_not_found]));
            return;
        }

        string title = loc.Format(Keys.user_anime_list, user.Username, $"{status}");
        List<Page> pages = [];
        System.Text.StringBuilder pageContent = new();
        int onPage = 0;
        int position = 1;

        foreach (MediaUserEntry entry in mediaList.Entries)
        {
            if (onPage == EntriesPerPage)
            {
                pages.Add(NewPage(title, pageContent.ToString(), profile.Avatar.Medium.ToString()));
                pageContent.Clear();
                onPage = 0;
            }

            string name = TitleForLanguage(entry.Media.Title, language);
            string mediaLink = Formatter.MaskedUrl(name, entry.Media.SiteUrl);
            string score = entry.Score != 0
                ? $" ({ScoreFormatter.FormatScoreUser($"{profile.MediaListOptions.ScoreFormat}", $"{entry.Score}")})"
                : string.Empty;

            pageContent.Append($"- {Formatter.Bold($"#{position}")} {mediaLink}{score}\n");
            onPage++;
            position++;
        }

        if (onPage > 0)
        {
            pages.Add(NewPage(title, pageContent.ToString(), profile.Avatar.Medium.ToString()));
        }

        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(timeouts.General));
        await interactivity.SendPaginatedResponseAsync(ctx.Interaction, false, ctx.User, pages, token: cts.Token);
    }

    /// <summary>Stops if the bot has not finished initializing; otherwise it defers the response.</summary>
    private async Task<bool> PrepareAsync(SlashCommandContext ctx, Loc loc)
    {
        if (!await ctx.EnsureBotReadyAsync(discordBotService, loc))
        {
            return false;
        }

        await ctx.DeferResponseAsync();
        return true;
    }

    private static Page NewPage(string title, string content, string avatarUrl) => new()
    {
        Embed = new DiscordEmbedBuilder
        {
            Title = title,
            Description = content.NormalizeDescription(),
            Color = YumikoColors.Primary,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail { Url = avatarUrl },
        },
    };

    private static string TitleForLanguage(MediaTitle title, MediaTitleType language) => language switch
    {
        MediaTitleType.ENGLISH when !string.IsNullOrEmpty(title.English) => title.English,
        MediaTitleType.NATIVE when !string.IsNullOrEmpty(title.Native) => title.Native,
        _ => title.Romaji,
    };

    private static TitleDescription CharacterDescription(Character character)
    {
        string description = character.Animes.Nodes?.Count > 0
            ? character.Animes.Nodes[0].Title.Romaji
            : character.Mangas.Nodes?.Count > 0
                ? character.Mangas.Nodes[0].Title.Romaji
                : "(Without animes and mangas)";

        return new TitleDescription { Title = character.Name.Full, Description = description };
    }

    private async Task SearchMediaResponseAsync(SlashCommandContext ctx, string search, MediaType type, DiscordUser? user)
    {
        Loc loc = ctx.Loc(localizer);
        DiscordUser target = user ?? ctx.User;

        if (!await PrepareAsync(ctx, loc))
        {
            return;
        }

        List<Media> results = await anilist.SearchMediaAsync(search, type, gamesSettings.AnilistPerPage);
        Media? media = await ChooseAsync(ctx, results, m => MediaDescription(m, loc), loc);

        if (media is null)
        {
            await ctx.EditResponseAsync(ErrorEmbed.NotFound(loc, $"{type}".UppercaseFirst()));
            return;
        }

        if (media.IsAdult && !ctx.Channel.IsNSFW)
        {
            await ctx.EditResponseAsync(ErrorEmbed.NsfwRequired(loc));
            return;
        }

        DiscordWebhookBuilder builder = new DiscordWebhookBuilder().AddEmbed(AnilistEmbeds.Media(media, type, loc));

        AnilistUserLink? link = await anilistUsers.GetLinkAsync(target.Id);
        if (link is not null && await anilist.GetMediaFromUserAsync(link.AnilistId, media.Id) is { } stats)
        {
            builder.AddEmbed(AnilistEmbeds.MediaUserStats(stats, loc));
        }

        await ctx.EditResponseAsync(builder);
    }

    private static TitleDescription MediaDescription(Media media, Loc loc)
    {
        string year = media.SeasonYear is not null
            ? $"{media.SeasonYear}"
            : media.StartDate.Year is not null
                ? $"{media.StartDate.Year}"
                : loc[Keys.not_yet_released];

        return new TitleDescription { Title = media.Title.Romaji, Description = $"{media.Format} - {year}" };
    }

    private async Task<T?> ChooseAsync<T>(SlashCommandContext ctx, List<T> results, Func<T, TitleDescription> toOption, Loc loc)
        where T : class
    {
        if (results.Count == 0)
        {
            return null;
        }

        int? chosen = await discordInteractivity.ChooseAsync(ctx, [.. results.Select(toOption)], loc);
        return chosen is null ? null : results[chosen.Value];
    }

    private static async Task NotifyTimeoutAsync(SlashCommandContext ctx, Loc loc) =>
        await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
        {
            Title = loc[Keys.response_timed_out],
            Color = DiscordColor.Red,
        }));
}
