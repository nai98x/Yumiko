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
/// Corre una partida de trivia completa. Los aciertos no se esperan con Interactivity: llegan por el
/// handler global de componentes, que marca la ronda como acertada en <see cref="TriviaState"/>; acá
/// se sondea ese estado cada 100 ms hasta que alguien acierta o se agota el tiempo.
/// </summary>
public sealed class TriviaGameRunner(
    TriviaState triviaState,
    IQuizLeaderboardRepository quizLeaderboard,
    TopggService topgg,
    TimeoutSettings timeouts)
{
    private const int PollingIntervalMs = 100;

    public async Task PlayAsync(SlashCommandContext ctx, List<TriviaItem> pool, GameSettings settings, string gameLabel, Loc loc)
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
            Title = gameLabel,
            CreatedBy = ctx.User,
        };

        if (!triviaState.TryAdd(trivia))
        {
            return;
        }

        List<GameUser> participants = [];
        int roundsPlayed = 0;

        try
        {
            for (int round = 1; round <= rounds; round++)
            {
                roundsPlayed = round;
                RoundResult result = await PlayRoundAsync(ctx, pool, round, rounds, gameLabel, participants, loc);

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

    private async Task<RoundResult> PlayRoundAsync(
        SlashCommandContext ctx,
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

        List<DiscordButtonComponent> buttons =
        [
            .. options.Select(name => new DiscordButtonComponent(
                DiscordButtonStyle.Secondary,
                $"{TriviaCustomIds.RoundPrefix}{name}",
                name.NormalizeButton())),
        ];

        RandomHelper.Shuffle(buttons, Random.Shared);

        DiscordFollowupMessageBuilder builder = new DiscordFollowupMessageBuilder()
            .AddEmbed(new DiscordEmbedBuilder
            {
                Color = DiscordColor.Gold,
                Title = $"{loc[Keys.guess_the]} {gameLabel}",
                Description = $"{loc[Keys.round]} {round} {loc[Keys.of]} {rounds}",
                ImageUrl = correct.Image,
            })
            .AddActionRowComponent(buttons)
            .AddActionRowComponent(new DiscordButtonComponent(
                DiscordButtonStyle.Danger,
                $"{TriviaCustomIds.CancelPrefix}{Guid.NewGuid()}",
                loc[Keys.finish_game]));

        DiscordMessage message = await ctx.FollowupAsync(builder);

        // El botón lleva el nombre completo; el label se recorta, así que se compara contra el nombre.
        triviaState.UpdateCurrentRound(ctx.Guild!.Id, ctx.Channel.Id, new QuizRound { Match = correct.Name });

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

            if (game.CurrentRound is { Guessed: true, Guesser: not null } currentRound)
            {
                AddPoint(participants, currentRound.Guesser);

                await ctx.FollowupAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder
                {
                    Title = loc.Format(Keys.user_has_guessed, DisplayNameOf(currentRound.Guesser)),
                    Description = reveal,
                    Color = DiscordColor.Green,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        IconUrl = currentRound.Guesser.AvatarUrl,
                        Text = $"{loc[Keys.time]}: {(currentRound.GuessTime - message.CreationTimestamp).TotalSeconds:0.##}s",
                    },
                }));

                return RoundResult.Guessed;
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

        List<TriviaRank<GameUser>> ranking = TriviaScoring.Rank(participants, x => x.Score, rounds);
        int total = TriviaScoring.TotalScore(participants, x => x.Score);

        foreach (TriviaRank<GameUser> entry in ranking)
        {
            header += $"{Medal(entry.Position)} - {entry.Participant.User.Mention}: " +
                          $"{entry.Score} {loc[Keys.guesses]} ({entry.Percentage}%)\n";

            await quizLeaderboard.AddResultAsync(
                ctx.Guild!.Id,
                entry.Participant.User.Id,
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

    private static void AddPoint(List<GameUser> participants, DiscordUser user)
    {
        GameUser? existing = participants.Find(x => x.User.Id == user.Id);

        if (existing is null)
        {
            participants.Add(new GameUser { User = user, Score = 1 });
        }
        else
        {
            existing.Score++;
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
}
