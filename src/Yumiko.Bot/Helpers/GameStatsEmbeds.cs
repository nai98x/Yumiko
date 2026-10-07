using DSharpPlus;
using DSharpPlus.Entities;
using Yumiko.Application.Games;
using Yumiko.Bot.Localization;
using Yumiko.Model.Entities;
using Yumiko.Model.Enum;

namespace Yumiko.Bot.Helpers;

public static class GameStatsEmbeds
{
    private static readonly string[] Medals = ["🥇", "🥈", "🥉"];

    public static DiscordEmbedBuilder LeaderboardQuiz(
        string title,
        IReadOnlyDictionary<Difficulty, List<GameStats>> byDifficulty,
        IReadOnlyDictionary<ulong, string> names,
        Loc loc)
    {
        DiscordEmbedBuilder embed = new()
        {
            Title = title,
            Color = YumikoColors.Primary,
        };

        foreach (Difficulty difficulty in (Difficulty[])System.Enum.GetValues<Difficulty>())
        {
            if (!byDifficulty.TryGetValue(difficulty, out List<GameStats>? players))
            {
                continue;
            }

            string table = FormatQuiz(players, names, loc);

            if (!string.IsNullOrEmpty(table))
            {
                embed.AddField(DifficultyLabel(difficulty, loc), table.NormalizeField());
            }
        }

        return embed;
    }

    public static DiscordEmbedBuilder LeaderboardGenre(
        string genre,
        List<GameStats> players,
        IReadOnlyDictionary<ulong, string> names,
        Loc loc) => new()
    {
        Title = $"{loc[Keys.stats]} - {loc[Keys.guess_the]} {genre}",
        Color = YumikoColors.Primary,
        Description = FormatQuiz(players, names, loc).NormalizeDescription(),
    };

    public static DiscordEmbedBuilder LeaderboardHigherOrLower(
        List<Rank<HigherOrLowerEntry>> ranks,
        IReadOnlyDictionary<ulong, string> names,
        Loc loc)
    {
        string table = string.Join("\n", ranks.Select(p =>
            $"{Prefix(p.Position)} - {names[p.Player.UserId]} - {loc[Keys.score]}: {Formatter.Bold($"{p.Player.Score}")}"));

        return new DiscordEmbedBuilder
        {
            Title = $"{loc[Keys.stats]} - Higher or Lower",
            Description = table.NormalizeDescription(),
            Color = YumikoColors.Primary,
        };
    }

    public static DiscordEmbedBuilder LeaderboardHigherOrLowerDuo(
        List<Rank<HigherOrLowerDuoEntry>> ranks,
        IReadOnlyDictionary<ulong, string> names,
        Loc loc)
    {
        string table = string.Join("\n", ranks.Select(p =>
            $"{Prefix(p.Position)} - {names[p.Player.FirstUserId]} & {names[p.Player.SecondUserId]} - " +
            $"{loc[Keys.score]}: {Formatter.Bold($"{p.Player.Score}")}"));

        return new DiscordEmbedBuilder
        {
            Title = $"{loc[Keys.stats]} - Higher or Lower Duo",
            Description = table.NormalizeDescription(),
            Color = YumikoColors.Primary,
        };
    }

    public static DiscordEmbedBuilder UserTriviaStats(string name, List<GameStatsUser> stats, Loc loc)
    {
        string desc = string.Join("\n", stats
            .Where(s => s.Stats.Count > 0)
            .Select(s =>
                $"**{loc[Keys.guess_the]} {GamemodeName(s.Gamemode, loc).ToLower(loc.Culture)}:**\n" +
                string.Join("\n", s.Stats.Select(d => DifficultyLine(d, loc)))));

        return string.IsNullOrEmpty(desc)
            ? new DiscordEmbedBuilder
            {
                Title = loc.Format(Keys.user_game_stats, name),
                Description = loc[Keys.no_stats_available],
                Color = DiscordColor.Red,
            }
            : new DiscordEmbedBuilder
            {
                Title = loc.Format(Keys.user_game_stats, name),
                Description = desc.NormalizeDescription(),
                Color = YumikoColors.Primary,
            };
    }

    public static DiscordEmbedBuilder UserGenreStats(List<GameStats> stats, Loc loc) =>
        stats.Count == 0
            ? new DiscordEmbedBuilder
            {
                Title = loc[Keys.genres],
                Description = loc[Keys.no_stats_available],
                Color = DiscordColor.Red,
            }
            : new DiscordEmbedBuilder
            {
                Title = loc[Keys.genres],
                Description = string.Join("\n", stats.Select(s => DifficultyLine(s, loc))).NormalizeDescription(),
                Color = YumikoColors.Primary,
            };

    public static DiscordEmbedBuilder UserHigherOrLowerStats(HigherOrLowerEntry? stats, Loc loc) => new()
    {
        Title = "Higher or Lower",
        Description = stats is null
            ? loc[Keys.no_stats_available]
            : $"{loc[Keys.score]}: {Formatter.Bold($"{stats.Score}")}",
        Color = stats is null ? DiscordColor.Red : YumikoColors.Primary,
    };

    /// <summary>Best pairs of the user, showing only the partner of each one.</summary>
    public static DiscordEmbedBuilder UserHigherOrLowerDuoStats(
        ulong userId,
        List<HigherOrLowerDuoEntry> pairs,
        IReadOnlyDictionary<ulong, string> names,
        Loc loc) => new()
    {
        Title = "Higher or Lower Duo",
        Description = pairs.Count == 0
            ? loc[Keys.no_stats_available]
            : string.Join("\n", pairs.Select(p =>
                $"{names[p.FirstUserId == userId ? p.SecondUserId : p.FirstUserId]} - {loc[Keys.score]}: {Formatter.Bold($"{p.Score}")}")),
        Color = pairs.Count == 0 ? DiscordColor.Red : YumikoColors.Primary,
    };

    /// <summary>Game name as it is displayed: in Spanish the enums have their own translation.</summary>
    public static string GamemodeName(Gamemode gamemode, Loc loc) =>
        loc.IsSpanish ? gamemode.ToSpanish() : $"{gamemode}";

    public static string DifficultyLabel(Difficulty difficulty, Loc loc) =>
        loc.IsSpanish ? difficulty.ToSpanish() : $"{difficulty}";

    private static string FormatQuiz(IEnumerable<GameStats> players, IReadOnlyDictionary<ulong, string> names, Loc loc) =>
        string.Join("\n", LeaderboardRanking
            .RankQuiz(players)
            .Select(p =>
                $"{Prefix(p.Position)} - {names[(ulong)p.Player.UserId]} - " +
                $"{loc[Keys.guesses]}: {Formatter.Bold($"{p.Player.AccuracyPercentage}%")} - " +
                $"{loc[Keys.games]}: {Formatter.Bold($"{p.Player.GamesPlayed}")}"));

    private static string DifficultyLine(GameStats stats, Loc loc) =>
        $"{loc[Keys.difficulty]}: {Formatter.Bold(stats.DifficultyName ?? "-")} - " +
        $"{loc[Keys.guesses]}: {Formatter.Bold($"{stats.AccuracyPercentage}%")} - " +
        $"{loc[Keys.games]}: {Formatter.Bold($"{stats.GamesPlayed}")}";

    private static string Prefix(int position) =>
        position is >= 1 and <= 3 ? Medals[position - 1] : Formatter.Bold($"#{position}");
}
