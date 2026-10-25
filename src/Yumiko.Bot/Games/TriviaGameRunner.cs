using DSharpPlus;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Yumiko.Application.Games;
using Yumiko.Application.Helpers;
using Yumiko.Bot.Configuration;
using Yumiko.Bot.Extensions;
using Yumiko.Bot.Helpers;
using Yumiko.Bot.Localization;
using Yumiko.Bot.Services.State;
using Yumiko.Model.Entities;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Bot.Games;

/// <summary>
/// Runs a full trivia match. The hits are not awaited with Interactivity: they arrive through the
/// global component handler, which marks the round as hit on <see cref="TriviaState"/>; here
/// that state is polled every 100 ms until someone hits or the time runs out.
/// </summary>
public sealed class TriviaGameRunner(
    TriviaState triviaState,
    IQuizLeaderboardRepository quizLeaderboard,
    TopggService topgg,
    TimeoutSettings timeouts)
{
    private const int PollingIntervalMs = 100;

    private const int ButtonsPerRow = 5;

    public async Task PlayAsync(
        SlashCommandContext ctx,
        List<TriviaItem> pool,
        GameSettings settings,
        TriviaPlayers players,
        string gameLabel,
        Loc loc)
    {
        int rounds = Math.Min(settings.Rounds, pool.Count);

        if (rounds == 0)
        {
            await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(ErrorEmbed.Unknown(loc)));
            return;
        }

        Trivia trivia = new()
        {
            GuildId = ctx.Guild!.Id,
            ChannelId = ctx.Channel.Id,
            TimeoutTotal = timeouts.Games,
            Players = players,
            Title = gameLabel,
            CreatedBy = ctx.User,
            Lobby = players == TriviaPlayers.Multiplayer ? new TriviaLobby(ctx.User.Id) : null,
        };

        if (!triviaState.TryAdd(trivia))
        {
            return;
        }

        List<GameUser> participants = [];
        int roundsPlayed = 0;

        try
        {
            if (trivia.Lobby is { } lobby && !await WaitForPlayersAsync(ctx, trivia, lobby, gameLabel, loc))
            {
                return;
            }

            // Everyone signed up is ranked, hits or not.
            participants.AddRange((trivia.Lobby?.Players ?? [ctx.User.Id]).Select(id => new GameUser { UserId = id }));

            for (int round = 1; round <= rounds; round++)
            {
                roundsPlayed = round;
                RoundResult result = await PlayRoundAsync(ctx, trivia, pool, round, rounds, gameLabel, participants, loc);

                if (result is RoundResult.Cancelled or RoundResult.NoGame)
                {
                    break;
                }
            }
        }
        finally
        {
            triviaState.Remove(trivia.GuildId, trivia.ChannelId);
            await ShowResultsAsync(ctx, participants, roundsPlayed, settings, loc);
        }
    }

    /// <summary>Returns false when the lobby is cancelled or times out before the owner starts it.</summary>
    private async Task<bool> WaitForPlayersAsync(
        SlashCommandContext ctx,
        Trivia trivia,
        TriviaLobby lobby,
        string gameLabel,
        Loc loc)
    {
        DiscordMessage message = await ctx.FollowupAsync(new DiscordFollowupMessageBuilder()
            .AddEmbed(LobbyEmbed(lobby, gameLabel, loc))
            .AddActionRowComponent(LobbyButtons(loc)));

        int version = lobby.Version;
        int iterations = (int)(timeouts.General * 1000 / PollingIntervalMs);

        for (int i = 0; i <= iterations; i++)
        {
            if (trivia.Canceled)
            {
                lobby.Close();
                await CloseLobbyAsync(ctx, message, loc[Keys.game_cancelled], DiscordColor.Red);
                return false;
            }

            if (lobby.Status == TriviaLobbyStatus.Started)
            {
                await ctx.EditFollowupAsync(message.Id, new DiscordWebhookBuilder()
                    .AddEmbed(LobbyEmbed(lobby, gameLabel, loc).WithTitle(loc[Keys.game_started]).WithColor(DiscordColor.Green)));
                return true;
            }

            if (lobby.Version != version)
            {
                version = lobby.Version;
                await ctx.EditFollowupAsync(message.Id, new DiscordWebhookBuilder()
                    .AddEmbed(LobbyEmbed(lobby, gameLabel, loc))
                    .AddActionRowComponent(LobbyButtons(loc)));
            }

            await Task.Delay(PollingIntervalMs);
        }

        if (!lobby.Close())
        {
            return true;
        }

        await CloseLobbyAsync(ctx, message, loc[Keys.lobby_timed_out], DiscordColor.Red);
        return false;
    }

    private DiscordEmbedBuilder LobbyEmbed(TriviaLobby lobby, string gameLabel, Loc loc) =>
        new DiscordEmbedBuilder
        {
            Title = loc[Keys.trivia_lobby],
            Description = $"{loc[Keys.guess_the]} {gameLabel}\n\n" +
                loc.Format(Keys.trivia_lobby_desc, $"<@{lobby.OwnerId}>", timeouts.General),
            Color = YumikoColors.Primary,
        }
        .AddField(loc[Keys.players], string.Join("\n", lobby.Players.Select(id => $"<@{id}>")).NormalizeField());

    private static DiscordButtonComponent[] LobbyButtons(Loc loc) =>
    [
        new(DiscordButtonStyle.Primary, $"{TriviaCustomIds.JoinPrefix}{Guid.NewGuid()}", loc[Keys.join]),
        new(DiscordButtonStyle.Secondary, $"{TriviaCustomIds.LeavePrefix}{Guid.NewGuid()}", loc[Keys.leave]),
        new(DiscordButtonStyle.Success, $"{TriviaCustomIds.StartPrefix}{Guid.NewGuid()}", loc[Keys.start_game]),
        new(DiscordButtonStyle.Danger, $"{TriviaCustomIds.CancelPrefix}{Guid.NewGuid()}", loc[Keys.finish_game]),
    ];

    private static async Task CloseLobbyAsync(SlashCommandContext ctx, DiscordMessage message, string title, DiscordColor color) =>
        await ctx.EditFollowupAsync(message.Id, new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
        {
            Title = title,
            Color = color,
        }));

    private async Task<RoundResult> PlayRoundAsync(
        SlashCommandContext ctx,
        Trivia trivia,
        List<TriviaItem> pool,
        int round,
        int rounds,
        string gameLabel,
        List<GameUser> participants,
        Loc loc)
    {
        List<int> indices = TriviaRound.PickOptions(pool.Count);
        TriviaItem correct = pool[indices[0]];
        List<string> options = [.. indices.Select(i => pool[i].Name)];

        pool.RemoveAt(indices[0]);

        // The names go on the label only: as a custom id they would blow past the 100 character limit and
        // two options sharing a name would collide, and Discord rejects the whole message in both cases.
        string roundId = Guid.NewGuid().ToString("N");
        List<(string Id, string Name)> choices = [.. options.Select((name, i) => ($"{roundId}-{i}", name))];

        foreach ((string id, string name) in choices)
        {
            trivia.OptionNames[id] = name;
        }

        List<DiscordButtonComponent> buttons =
        [
            .. choices.Select(choice => new DiscordButtonComponent(
                DiscordButtonStyle.Secondary,
                $"{TriviaCustomIds.RoundPrefix}{choice.Id}",
                choice.Name.NormalizeButton())),
        ];

        RandomHelper.Shuffle(buttons, Random.Shared);

        DiscordFollowupMessageBuilder builder = new DiscordFollowupMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder
            {
                Color = DiscordColor.Gold,
                Title = $"{loc[Keys.guess_the]} {gameLabel}",
                Description = $"{loc[Keys.round]} {round} {loc[Keys.of]} {rounds}",
                ImageUrl = correct.Image,
            });

        foreach (DiscordButtonComponent[] row in buttons.Chunk(ButtonsPerRow))
        {
            builder.AddActionRowComponent(row);
        }

        builder.AddActionRowComponent(new DiscordButtonComponent(
            DiscordButtonStyle.Danger,
            $"{TriviaCustomIds.CancelPrefix}{Guid.NewGuid()}",
            loc[Keys.finish_game]));

        DiscordMessage message = await ctx.FollowupAsync(builder);

        trivia.CurrentRound = new QuizRound
        {
            State = new TriviaRoundState(choices.Select(c => c.Id), choices[0].Id, trivia.Lobby?.Players ?? [ctx.User.Id]),
        };

        string reveal = correct.Description.NormalizeDescription();
        RoundResult result = await WaitForAnswerAsync(ctx, message, reveal, participants, loc);

        return result;
    }

    private async Task<RoundResult> WaitForAnswerAsync(
        SlashCommandContext ctx,
        DiscordMessage message,
        string reveal,
        List<GameUser> participants,
        Loc loc)
    {
        int iterations = (int)(timeouts.Games * 1000 / PollingIntervalMs);

        for (int i = 0; i <= iterations; i++)
        {
            Trivia? game = triviaState.Get(ctx.Guild!.Id, ctx.Channel.Id);

            if (game is null)
            {
                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(ErrorEmbed.Create(loc[Keys.error], loc[Keys.no_active_game])));
                return RoundResult.NoGame;
            }

            if (game.Canceled)
            {
                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = loc[Keys.game_cancelled],
                    Description = reveal,
                    Color = DiscordColor.Red,
                }));
                return RoundResult.Cancelled;
            }

            if (game.CurrentRound is { Resolved: true, Guesser: not null } currentRound)
            {
                TimeSpan elapsed = currentRound.GuessTime - message.CreationTimestamp;
                AddPoint(participants, currentRound.Guesser.Id, elapsed);

                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = loc.Format(Keys.user_has_guessed, DisplayNameOf(currentRound.Guesser)),
                    Description = reveal,
                    Color = DiscordColor.Green,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        IconUrl = currentRound.Guesser.AvatarUrl,
                        Text = $"{loc[Keys.time]}: {elapsed.TotalSeconds:0.##}s",
                    },
                }));

                return RoundResult.Guessed;
            }

            if (game.CurrentRound is { Resolved: true, Loser: not null } missedRound)
            {
                DiscordEmbedBuilder missed = new()
                {
                    Title = loc.Format(Keys.user_has_missed, DisplayNameOf(missedRound.Loser)),
                    Description = reveal,
                    Color = DiscordColor.Red,
                };

                if (missedRound.MissedAttempt is { } attempt)
                {
                    missed.AddField(loc[Keys.your_attempt], $"`{attempt}`");
                }

                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(missed));

                return RoundResult.NotGuessed;
            }

            if (game.CurrentRound is { Resolved: true })
            {
                break;
            }

            await Task.Delay(PollingIntervalMs);
        }

        await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
        {
            Title = loc[Keys.nobody_has_guessed],
            Description = reveal,
            Color = DiscordColor.Red,
        }));

        return RoundResult.NotGuessed;
    }

    private async Task ShowResultsAsync(
        SlashCommandContext ctx,
        List<GameUser> participants,
        int rounds,
        GameSettings settings,
        Loc loc)
    {
        if (rounds == 0)
        {
            return;
        }

        string header = settings.Gamemode == Gamemode.Genres
            ? $"{GameStatsEmbeds.GamemodeName(settings.Gamemode, loc).UppercaseFirst()}: {Formatter.Bold(settings.Genre ?? "-")}\n\n"
            : $"{loc[Keys.difficulty]}: {Formatter.Bold(GameStatsEmbeds.DifficultyLabel(settings.Difficulty, loc))}\n\n";

        List<TriviaRank<GameUser>> ranking = TriviaScoring.Rank(participants, x => x.Score, x => x.TotalTime, rounds);
        int total = TriviaScoring.TotalScore(participants, x => x.Score);

        foreach (TriviaRank<GameUser> entry in ranking)
        {
            header += $"{Medal(entry.Position)} - <@{entry.Participant.UserId}>: " +
                          $"{entry.Score} {loc[Keys.guesses]} ({entry.Percentage}%)";

            if (entry.AverageTime is { } average)
            {
                header += $" - {loc[Keys.average_time]}: {average.TotalSeconds:0.##}s";
            }

            header += "\n";

            await quizLeaderboard.AddResultAsync(
                ctx.Guild!.Id,
                entry.Participant.UserId,
                settings.Gamemode,
                settings.Difficulty,
                entry.Score,
                rounds);
        }

        header += $"\n{Formatter.Bold($"{loc[Keys.total]} ({total}/{rounds})")}";

        await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
        {
            Title = $"{loc[Keys.results]} - {loc[Keys.guess_the]} {GameStatsEmbeds.GamemodeName(settings.Gamemode, loc)}",
            Description = header.NormalizeDescription(),
            Color = YumikoColors.Primary,
            Footer = new DiscordEmbedBuilder.EmbedFooter { Text = loc[Keys.see_stats] },
        }));

        if (await topgg.GetVoteReminderAsync(ctx.Client, ctx.User.Id, loc) is { } reminder)
        {
            await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(reminder));
        }
    }

    private static void AddPoint(List<GameUser> participants, ulong userId, TimeSpan elapsed)
    {
        GameUser? existing = participants.Find(x => x.UserId == userId);

        if (existing is null)
        {
            participants.Add(new GameUser { UserId = userId, Score = 1, TotalTime = elapsed });
        }
        else
        {
            existing.Score++;
            existing.TotalTime += elapsed;
        }
    }

    private static string DisplayNameOf(DiscordUser user) =>
        user is DiscordMember member ? member.DisplayName : user.Username;

    private static string Medal(int position) => position switch
    {
        1 => "🥇",
        2 => "🥈",
        3 => "🥉",
        _ => Formatter.Bold($"#{position}"),
    };

    private enum RoundResult
    {
        Guessed,
        NotGuessed,
        Cancelled,
        NoGame,
    }
}

public static class TriviaCustomIds
{
    public const string RoundPrefix = "quiz-round-";

    public const string CancelPrefix = "quiz-cancel-";

    public const string JoinPrefix = "quiz-join-";

    public const string LeavePrefix = "quiz-leave-";

    public const string StartPrefix = "quiz-start-";
}
