using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.Localization;
using DSharpPlus.Entities;
using Yumiko.Application.Games;
using Yumiko.Bot.Commands.Framework;
using Yumiko.Bot.Commands.Framework.Attributes;
using Yumiko.Bot.Commands.Framework.Choices;
using Yumiko.Bot.Extensions;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services;
using Yumiko.Model.Entities;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Bot.Commands.Slash;

[TestCommand]
[Command("stats")]
[Description("Statistics from different games")]
[RequireGuild]
public sealed class Stats(
    ILocalizer localizer,
    DiscordBotService discordBotService,
    IQuizLeaderboardRepository quizLeaderboard,
    IHigherOrLowerLeaderboardRepository holLeaderboard,
    IHigherOrLowerDuoLeaderboardRepository holDuoLeaderboard,
    GenreSelector genreSelector,
    DiscordInteractivity discordInteractivity,
    TopggService topgg)
{
    private const int LeaderboardSize = 10;
    private const int UserDuoPairs = 5;

    [Command("user")]
    [Description("Shows the statistics of all games of a user")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task UserAsync(
        SlashCommandContext ctx,
        [Parameter("user")] [Description("The user's stats to retrieve")] DiscordUser? user = null)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await PrepareAsync(ctx, loc))
        {
            return;
        }

        DiscordUser target = user ?? ctx.User;
        List<GameStatsUser> trivia = await quizLeaderboard.GetStatsUserAsync(ctx.Guild!.Id, target.Id);
        List<GameStats> genres = await quizLeaderboard.GetGenreStatsUserAsync(ctx.Guild.Id, target.Id);
        HigherOrLowerEntry? hol = await holLeaderboard.GetStatsUserAsync(ctx.Guild.Id, target.Id);
        List<HigherOrLowerDuoEntry> holDuo = await holDuoLeaderboard.GetStatsUserAsync(ctx.Guild.Id, target.Id, UserDuoPairs);
        Dictionary<ulong, string> names = await MemberNames.ResolveAsync(
            ctx.Client, ctx.Guild, [target.Id, .. holDuo.SelectMany(p => new[] { p.FirstUserId, p.SecondUserId })]);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(GameStatsEmbeds.UserTriviaStats(names[target.Id], trivia, loc))
            .AddEmbed(GameStatsEmbeds.UserGenreStats(genres, loc))
            .AddEmbed(GameStatsEmbeds.UserHigherOrLowerStats(hol, loc))
            .AddEmbed(GameStatsEmbeds.UserHigherOrLowerDuoStats(target.Id, holDuo, names, loc)));

        await SendVoteReminderAsync(ctx, loc);
    }

    [Command("trivia")]
    [Description("Shows the statistics of the trivia game")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task TriviaAsync(
        SlashCommandContext ctx,
        [Parameter("game")] [Description("The gamemode you want to see the stats")] GamemodeChoice gamemodeChoice)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await PrepareAsync(ctx, loc))
        {
            return;
        }

        Gamemode gamemode = gamemodeChoice.ToModel();

        if (gamemode == Gamemode.Genres)
        {
            if (await genreSelector.ChooseAsync(ctx, loc) is not { } genre)
            {
                await ctx.EditResponseAsync(ErrorEmbed.Create(loc[Keys.error], loc[Keys.no_genre_selected]));
                return;
            }

            List<GameStats> players = await quizLeaderboard.GetGenreLeaderboardAsync(ctx.Guild!.Id, genre, LeaderboardSize);
            Dictionary<ulong, string> genreNames = await MemberNames.ResolveAsync(ctx.Client, ctx.Guild, players.Select(p => (ulong)p.UserId));
            await ctx.EditResponseAsync(GameStatsEmbeds.LeaderboardGenre(genre, players, genreNames, loc));
            await SendVoteReminderAsync(ctx, loc);
            return;
        }

        Dictionary<Difficulty, List<GameStats>> byDifficulty = [];

        foreach (Difficulty difficulty in System.Enum.GetValues<Difficulty>())
        {
            byDifficulty[difficulty] = await quizLeaderboard.GetLeaderboardAsync(ctx.Guild!.Id, gamemode, difficulty, LeaderboardSize);
        }

        string game = loc.IsSpanish
            ? GameStatsEmbeds.GamemodeName(gamemode, loc)
            : $"{loc[Keys.guess_the]} {$"{gamemode}".ToLower(loc.Culture)}";

        Dictionary<ulong, string> names = await MemberNames.ResolveAsync(
            ctx.Client, ctx.Guild!, byDifficulty.Values.SelectMany(players => players.Select(p => (ulong)p.UserId)));

        await ctx.EditResponseAsync(GameStatsEmbeds.LeaderboardQuiz($"{loc[Keys.stats]} - {game}", byDifficulty, names, loc));
        await SendVoteReminderAsync(ctx, loc);
    }

    [Command("higherorlower")]
    [Description("Shows the statistics of the Higher or Lower game")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task HigherOrLowerAsync(SlashCommandContext ctx)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await PrepareAsync(ctx, loc))
        {
            return;
        }

        List<Rank<HigherOrLowerEntry>> ranks = LeaderboardRanking.RankHigherOrLower(await holLeaderboard.GetLeaderboardAsync(ctx.Guild!.Id));
        Dictionary<ulong, string> names = await MemberNames.ResolveAsync(ctx.Client, ctx.Guild, ranks.Select(r => r.Player.UserId));

        await ctx.EditResponseAsync(GameStatsEmbeds.LeaderboardHigherOrLower(ranks, names, loc));
        await SendVoteReminderAsync(ctx, loc);
    }

    [Command("higherorlower_duo")]
    [Description("Shows the statistics of the Higher or Lower duo game")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task HigherOrLowerDuoAsync(SlashCommandContext ctx)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await PrepareAsync(ctx, loc))
        {
            return;
        }

        List<Rank<HigherOrLowerDuoEntry>> ranks =
            LeaderboardRanking.RankHigherOrLowerDuo(await holDuoLeaderboard.GetLeaderboardAsync(ctx.Guild!.Id));
        Dictionary<ulong, string> names = await MemberNames.ResolveAsync(
            ctx.Client, ctx.Guild, ranks.SelectMany(r => new[] { r.Player.FirstUserId, r.Player.SecondUserId }));

        await ctx.EditResponseAsync(GameStatsEmbeds.LeaderboardHigherOrLowerDuo(ranks, names, loc));
        await SendVoteReminderAsync(ctx, loc);
    }

    [Command("delete")]
    [Description("Deletes user statistics on the server")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task DeleteAsync(SlashCommandContext ctx)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await PrepareAsync(ctx, loc))
        {
            return;
        }

        bool confirmar = await discordInteractivity.ConfirmAsync(
            ctx,
            loc[Keys.confirm_delete_stats],
            Formatter.Bold(loc[Keys.action_cannont_be_undone]),
            loc);

        if (!confirmar)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(loc[Keys.delete_stats_cancelled]));
            return;
        }

        foreach (Gamemode gamemode in System.Enum.GetValues<Gamemode>().Where(g => g != Gamemode.Genres))
        {
            await quizLeaderboard.DeleteStatsAsync(ctx.Guild!.Id, ctx.User.Id, gamemode);
        }

        await holLeaderboard.DeleteStatsAsync(ctx.Guild!.Id, ctx.User.Id);
        await holDuoLeaderboard.DeleteStatsAsync(ctx.Guild!.Id, ctx.User.Id);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(loc[Keys.delete_stats_done]));
    }

    [Command("bot")]
    [Description("Shows Yumiko's information and stats")]
    [InteractionLocalizer<ResxInteractionLocalizer>]
    public async Task BotAsync(SlashCommandContext ctx)
    {
        Loc loc = ctx.Loc(localizer);

        if (!await PrepareAsync(ctx, loc))
        {
            return;
        }

        // The true forces a full collection before measuring; otherwise the number includes garbage.
        string memory = $"{GC.GetTotalMemory(true) / 1024 / 1024:n0} MB";

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder
        {
            Title = loc.Format(Keys.bot_stats, ctx.Client.CurrentUser.Username),
            Color = YumikoColors.Primary,
        }
        .AddField(loc[Keys.library], $"DSharpPlus {ctx.Client.VersionString}", true)
        .AddField(loc[Keys.memory_usage], memory, true)
        .AddField(loc[Keys.latency], $"{ctx.Client.GetConnectionLatency(ctx.Guild?.Id ?? 0).TotalMilliseconds:0} ms", true)
        .AddField(loc[Keys.total_guilds], $"{ctx.Client.Guilds.Count}", true)
        .AddField(loc[Keys.total_users], $"{ctx.Client.Guilds.Values.Sum(g => g.MemberCount)}", true)
        .AddField(loc[Keys.uptime], $"{discordBotService.Uptime:d\\d\\ hh\\:mm\\:ss}", true);

        if (topgg.Enabled)
        {
            embed.AddField(loc[Keys.vote_count], $"{await topgg.GetMonthlyVotesAsync(ctx.Client)}", true);
        }

        await ctx.EditResponseAsync(embed);
    }

    private async Task<bool> PrepareAsync(SlashCommandContext ctx, Loc loc)
    {
        if (!await ctx.EnsureBotReadyAsync(discordBotService, loc))
        {
            return false;
        }

        await ctx.DeferResponseAsync();
        return true;
    }

    private async Task SendVoteReminderAsync(SlashCommandContext ctx, Loc loc)
    {
        if (await topgg.GetVoteReminderAsync(ctx.Client, ctx.User.Id, loc) is { } reminder)
        {
            await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(reminder));
        }
    }
}
