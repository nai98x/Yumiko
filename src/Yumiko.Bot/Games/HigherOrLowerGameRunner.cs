using System.Diagnostics;
using System.Globalization;
using DSharpPlus;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using Yumiko.Application.Games;
using Yumiko.Application.Helpers;
using Yumiko.Bot.Configuration;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services.State;
using Yumiko.Model.Entities;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Bot.Games;

public sealed class HigherOrLowerGameRunner(
    AnilistMediaCacheState mediaCache,
    IHigherOrLowerLeaderboardRepository leaderboard,
    IHigherOrLowerDuoLeaderboardRepository duoLeaderboard,
    IHttpClientFactory httpClientFactory,
    TimeoutSettings timeouts)
{
    private const string CancelCustomId = "hol-cancel";
    private const string ConfirmCustomId = "hol-duo-confirm";
    private const string RejectCustomId = "hol-duo-reject";
    private const int ImageWidth = 500;
    private const int ImageHeight = 375;

    private static readonly TimeSpan AnswerTimeout = TimeSpan.FromSeconds(15);

    /// <summary>
    /// The token of an interaction expires after 15 minutes; it is cut a minute earlier to be able to send
    /// the closing message.
    /// </summary>
    private static readonly TimeSpan MaxDuration = TimeSpan.FromMinutes(14);

    public async Task PlayAsync(SlashCommandContext ctx, GamemodeHoL gamemode, InteractivityExtension interactivity, Loc loc)
    {
        Stopwatch clock = Stopwatch.StartNew();
        List<Anime> list = [.. mediaCache.Media];

        DiscordEmbed? previousEmbed = null;
        int score = 0;

        while (list.Count >= 2)
        {
            if (HigherOrLower.PickPair(list) is not { } pair)
            {
                break;
            }

            (Anime first, Anime second) = pair;

            DiscordMessage message = await ctx.FollowupAsync(await BuildRoundAsync(first, second, gamemode, previousEmbed, null, loc));

            InteractivityResult<ComponentInteractionCreatedEventArgs> answer =
                await interactivity.WaitForButtonAsync(message, ctx.User, AnswerTimeout);

            if (answer.TimedOut)
            {
                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = loc[Keys.defeat],
                    Description = $"{loc.Format(Keys.no_answser_in_time, AnswerTimeout.TotalSeconds)}\n\n{loc[Keys.score]}: **{score}**",
                    Color = DiscordColor.Red,
                }));
                break;
            }

            if (answer.Result.Id == CancelCustomId)
            {
                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder { Title = loc[Keys.game_cancelled], Color = DiscordColor.Red }));
                break;
            }

            bool choseFirst = answer.Result.Id == $"{first.Id}";
            Anime selected = choseFirst ? first : second;
            Anime other = choseFirst ? second : first;
            TimeSpan elapsed = answer.Result.Interaction.CreationTimestamp - message.CreationTimestamp;

            if (!HigherOrLower.IsCorrect(selected, other, gamemode))
            {
                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = loc.Format(Keys.miss_user_games, DisplayNameOf(ctx)),
                    Description = $"**{loc[Keys.defeat]}**\n\n{Comparison(selected, other, gamemode, false, loc)}\n\n{loc[Keys.score]}: **{score}**",
                    Color = DiscordColor.Red,
                    Footer = Footer(ctx.User, elapsed, loc),
                }));
                break;
            }

            score++;
            previousEmbed = new DiscordEmbedBuilder
            {
                Title = loc.Format(Keys.guess_user, DisplayNameOf(ctx)),
                Description = $"{Comparison(selected, other, gamemode, true, loc)}\n\n{loc[Keys.score]}: **{score}**",
                Color = DiscordColor.Green,
                Footer = Footer(ctx.User, elapsed, loc),
            };

            list.Remove(first);
            list.Remove(second);

            if (list.Count < 2 || clock.Elapsed >= MaxDuration)
            {
                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder { Title = loc[Keys.victory], Color = DiscordColor.Green }));
                break;
            }
        }

        clock.Stop();

        if (score > 0 && await leaderboard.AddResultAsync(ctx.Guild!.Id, ctx.User.Id, score))
        {
            await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = loc[Keys.new_record],
                Description = $"{loc.Format(Keys.new_record_desc, ctx.User.Mention)}\n\n{loc.Format(Keys.your_new_record_is, score)}",
                Color = YumikoColors.Primary,
            }));
        }
    }

    public async Task PlayDuoAsync(SlashCommandContext ctx, DiscordUser partner, GamemodeHoL gamemode, InteractivityExtension interactivity, Loc loc)
    {
        if (!await ConfirmPartnerAsync(ctx, partner, gamemode, interactivity, loc))
        {
            return;
        }

        Dictionary<ulong, DiscordUser> players = new() { [ctx.User.Id] = ctx.User, [partner.Id] = partner };
        DiscordUser starter = Random.Shared.Next(2) == 0 ? ctx.User : partner;
        HigherOrLowerDuoState state = new(starter.Id, starter.Id == ctx.User.Id ? partner.Id : ctx.User.Id);

        await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
        {
            Title = loc.Format(Keys.higher_or_lower_duo_starts, DisplayNameOf(starter)),
            Color = YumikoColors.Primary,
        }));

        Stopwatch clock = Stopwatch.StartNew();
        List<Anime> list = [.. mediaCache.Media];
        DiscordEmbed? previousEmbed = null;

        while (HigherOrLower.PickPair(list) is { } pair)
        {
            (Anime first, Anime second) = pair;
            DiscordUser current = players[state.CurrentPlayer];

            DiscordMessage message = await ctx.FollowupAsync(await BuildRoundAsync(first, second, gamemode, previousEmbed, current, loc));

            InteractivityResult<ComponentInteractionCreatedEventArgs> answer = await interactivity.WaitForButtonAsync(
                message,
                e => e.Id == CancelCustomId ? players.ContainsKey(e.User.Id) : e.User.Id == current.Id,
                AnswerTimeout);

            if (answer.TimedOut)
            {
                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = loc[Keys.defeat],
                    Description = $"{current.Mention}: {loc.Format(Keys.no_answser_in_time, AnswerTimeout.TotalSeconds)}\n\n{loc[Keys.score]}: **{state.Score}**",
                    Color = DiscordColor.Red,
                }));
                break;
            }

            if (answer.Result.Id == CancelCustomId)
            {
                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder { Title = loc[Keys.game_cancelled], Color = DiscordColor.Red }));
                break;
            }

            bool choseFirst = answer.Result.Id == $"{first.Id}";
            Anime selected = choseFirst ? first : second;
            Anime other = choseFirst ? second : first;
            TimeSpan elapsed = answer.Result.Interaction.CreationTimestamp - message.CreationTimestamp;
            bool isCorrect = HigherOrLower.IsCorrect(selected, other, gamemode);

            state.Answer(isCorrect, elapsed);

            if (!isCorrect)
            {
                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = loc.Format(Keys.miss_user_games, DisplayNameOf(current)),
                    Description = $"**{loc[Keys.defeat]}**\n\n{Comparison(selected, other, gamemode, false, loc)}\n\n{loc[Keys.score]}: **{state.Score}**",
                    Color = DiscordColor.Red,
                    Footer = Footer(current, elapsed, loc),
                }));
                break;
            }

            previousEmbed = new DiscordEmbedBuilder
            {
                Title = loc.Format(Keys.guess_user, DisplayNameOf(current)),
                Description = $"{Comparison(selected, other, gamemode, true, loc)}\n\n{loc[Keys.score]}: **{state.Score}**",
                Color = DiscordColor.Green,
                Footer = Footer(current, elapsed, loc),
            };

            list.Remove(first);
            list.Remove(second);

            if (list.Count < 2 || clock.Elapsed >= MaxDuration)
            {
                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder { Title = loc[Keys.victory], Color = DiscordColor.Green }));
                break;
            }
        }

        clock.Stop();

        if (state.Summaries().Any(s => s.Answers > 0))
        {
            await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(DuoSummary(state, players, clock.Elapsed, loc)));
        }

        if (state.Score > 0 && await duoLeaderboard.AddResultAsync(ctx.Guild!.Id, ctx.User.Id, partner.Id, state.Score))
        {
            await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
            {
                Title = loc[Keys.new_record],
                Description = loc.Format(Keys.higher_or_lower_duo_new_record_desc, ctx.User.Mention, partner.Mention, state.Score),
                Color = YumikoColors.Primary,
            }));
        }
    }

    private async Task<bool> ConfirmPartnerAsync(
        SlashCommandContext ctx,
        DiscordUser partner,
        GamemodeHoL gamemode,
        InteractivityExtension interactivity,
        Loc loc)
    {
        string description =
            $"{loc[gamemode == GamemodeHoL.Score ? Keys.higher_or_lower_desc : Keys.higher_or_lower_desc_popularity]}\n\n" +
            loc.Format(Keys.higher_or_lower_duo_invite, ctx.User.Mention, partner.Mention);

        DiscordEmbedBuilder invite = new()
        {
            Title = "Higher or Lower Duo",
            Description = $"{description}\n\n{loc.Format(Keys.higher_or_lower_duo_awaiting_confirmation, partner.Mention)}",
            Color = YumikoColors.Primary,
        };

        await ctx.RespondAsync(new DiscordInteractionResponseBuilder()
            .WithContent(loc.Format(Keys.higher_or_lower_duo_confirm_prompt, partner.Mention))
            .AddMention(new UserMention(partner))
            .AddEmbed(invite)
            .AddActionRowComponent(
                new DiscordButtonComponent(DiscordButtonStyle.Success, ConfirmCustomId, loc[Keys.confirm]),
                new DiscordButtonComponent(DiscordButtonStyle.Danger, RejectCustomId, loc[Keys.reject])));

        if (await ctx.GetResponseAsync() is not { } message)
        {
            return false;
        }

        InteractivityResult<ComponentInteractionCreatedEventArgs> result =
            await interactivity.WaitForButtonAsync(message, partner, TimeSpan.FromSeconds(timeouts.General));

        bool confirmed = !result.TimedOut && result.Result.Id == ConfirmCustomId;
        invite.Description = description;

        if (!confirmed)
        {
            invite.Color = DiscordColor.Red;
            invite.AddField(
                loc[Keys.game_cancelled],
                loc.Format(result.TimedOut ? Keys.higher_or_lower_duo_not_confirmed : Keys.higher_or_lower_duo_rejected, partner.Mention));
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(partner.Mention).AddEmbed(invite));

        return confirmed;
    }

    private static DiscordEmbedBuilder DuoSummary(
        HigherOrLowerDuoState state,
        IReadOnlyDictionary<ulong, DiscordUser> players,
        TimeSpan duration,
        Loc loc)
    {
        IReadOnlyList<HigherOrLowerDuoPlayerSummary> summaries = state.Summaries();

        // The lightning marks the one with the best average, only when both answered and they differ.
        TimeSpan? bestAverage = summaries.All(s => s.AverageTime is not null) && summaries[0].AverageTime != summaries[1].AverageTime
            ? summaries.Min(s => s.AverageTime)
            : null;

        DiscordEmbedBuilder embed = new()
        {
            Title = loc[Keys.game_summary],
            Description =
                $"{loc[Keys.score]}: **{state.Score}**\n" +
                $"{loc[Keys.game_duration]}: **{duration:m\\:ss}**",
            Color = YumikoColors.Primary,
        };

        foreach (HigherOrLowerDuoPlayerSummary summary in summaries)
        {
            string name = DisplayNameOf(players[summary.UserId]);

            embed.AddField(
                summary.AverageTime is not null && summary.AverageTime == bestAverage ? $"⚡ {name}" : name,
                $"{loc[Keys.guesses]}: **{summary.Hits}**\n" +
                $"{loc[Keys.average_time]}: **{Seconds(summary.AverageTime)}**\n" +
                $"{loc[Keys.fastest_answer]}: **{Seconds(summary.FastestTime)}**",
                true);
        }

        return embed;
    }

    private async Task<DiscordFollowupMessageBuilder> BuildRoundAsync(
        Anime first,
        Anime second,
        GamemodeHoL gamemode,
        DiscordEmbed? previousEmbed,
        DiscordUser? turnOf,
        Loc loc)
    {
        DiscordFollowupMessageBuilder builder = new DiscordFollowupMessageBuilder()
            .AddActionRowComponent(
                new DiscordButtonComponent(DiscordButtonStyle.Primary, $"{first.Id}", first.TitleRomaji.NormalizeButton()),
                new DiscordButtonComponent(DiscordButtonStyle.Primary, $"{second.Id}", second.TitleRomaji.NormalizeButton()),
                new DiscordButtonComponent(DiscordButtonStyle.Danger, CancelCustomId, loc[Keys.finish_game]));

        if (await BuildImageAsync(first, second) is { } image)
        {
            builder.AddFile("image.png", image.ToMemoryStream());
        }

        if (previousEmbed is not null)
        {
            builder.AddEmbed(previousEmbed);
        }

        return builder.AddEmbed(new DiscordEmbedBuilder
        {
            Title = loc[gamemode == GamemodeHoL.Score ? Keys.which_one_has_better_score : Keys.which_one_is_more_popular],
            Description = turnOf is null ? null : loc.Format(Keys.player_turn, turnOf.Mention),
            Color = YumikoColors.Primary,
            ImageUrl = "attachment://image.png",
        });
    }

    private async Task<byte[]?> BuildImageAsync(Anime first, Anime second)
    {
        if (first.Image is null || second.Image is null)
        {
            return null;
        }

        HttpClient client = httpClientFactory.CreateClient();
        byte[] bytes1 = await client.GetByteArrayAsync(first.Image);
        byte[] bytes2 = await client.GetByteArrayAsync(second.Image);

        byte[] merged = ImageHelper.MergeImage(bytes1, bytes2, ImageWidth, ImageHeight);
        byte[] frame = await File.ReadAllBytesAsync(Path.Join(AppContext.BaseDirectory, "Images", "frame-hol.png"));

        return ImageHelper.OverlapImage(merged, frame, ImageWidth, ImageHeight);
    }

    private static string Comparison(Anime selected, Anime other, GamemodeHoL gamemode, bool isCorrect, Loc loc)
    {
        if (gamemode == GamemodeHoL.Popularity)
        {
            return loc.Format(
                isCorrect ? Keys.higher_or_lower_round_win_popularity : Keys.higher_or_lower_round_defeat_popularity,
                selected.TitleRomaji, selected.Favourites, other.TitleRomaji, other.Favourites);
        }

        return loc.Format(
            isCorrect ? Keys.higher_or_lower_round_win : Keys.higher_or_lower_round_defeat,
            selected.TitleRomaji, HigherOrLower.ScoreOutOfTen(selected),
            other.TitleRomaji, HigherOrLower.ScoreOutOfTen(other));
    }

    private static DiscordEmbedBuilder.EmbedFooter Footer(DiscordUser user, TimeSpan elapsed, Loc loc) => new()
    {
        IconUrl = user.AvatarUrl,
        Text = $"{loc[Keys.time]}: {Seconds(elapsed)}",
    };

    private static string Seconds(TimeSpan? time) =>
        time is { } t ? $"{t.TotalSeconds.ToString("0.##", CultureInfo.InvariantCulture)}s" : "-";

    private static string DisplayNameOf(SlashCommandContext ctx) =>
        ctx.Member?.DisplayName ?? ctx.User.Username;

    private static string DisplayNameOf(DiscordUser user) =>
        (user as DiscordMember)?.DisplayName ?? user.GlobalName ?? user.Username;
}
