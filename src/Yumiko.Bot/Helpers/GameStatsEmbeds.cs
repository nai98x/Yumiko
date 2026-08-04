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

            string table = FormatQuiz(players, loc);

            if (!string.IsNullOrEmpty(table))
            {
                embed.AddField(DifficultyLabel(difficulty, loc), table.NormalizeField());
            }
        }

        return embed;
    }

    public static DiscordEmbedBuilder LeaderboardGenre(string genre, List<GameStats> players, Loc loc) => new()
    {
        Title = $"{loc[Keys.stats]} - {loc[Keys.guess_the]} {genre}",
        Color = YumikoColors.Primary,
        Description = FormatQuiz(players, loc).NormalizeDescription(),
    };

    public static DiscordEmbedBuilder LeaderboardHigherOrLower(List<HigherOrLowerEntry> players, Loc loc)
    {
        string table = string.Join("\n", LeaderboardRanking
            .RankHigherOrLower(players)
            .Select(p => $"{Prefix(p.Position)} - <@{p.Player.UserId}> - {loc[Keys.score]}: {Formatter.Bold($"{p.Player.Score}")}"));

        return new DiscordEmbedBuilder
        {
            Title = $"{loc[Keys.stats]} - Higher or Lower",
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

    /// <summary>Nombre del juego tal como se muestra: en español los enums tienen traducción propia.</summary>
    public static string GamemodeName(Gamemode gamemode, Loc loc) =>
        loc.IsSpanish ? gamemode.ToSpanish() : $"{gamemode}";

    public static string DifficultyLabel(Difficulty difficulty, Loc loc) =>
        loc.IsSpanish ? difficulty.ToSpanish() : $"{difficulty}";

    private static string FormatQuiz(IEnumerable<GameStats> players, Loc loc) =>
        string.Join("\n", LeaderboardRanking
            .RankQuiz(players)
            .Select(p =>
                $"{Prefix(p.Position)} - <@{p.Player.UserId}> - " +
                $"{loc[Keys.guesses]}: {Formatter.Bold($"{p.Player.AccuracyPercentage}%")} - " +
                $"{loc[Keys.games]}: {Formatter.Bold($"{p.Player.GamesPlayed}")}"));

    private static string DifficultyLine(GameStats stats, Loc loc) =>
        $"{loc[Keys.difficulty]}: {Formatter.Bold(stats.DifficultyName ?? "-")} - " +
        $"{loc[Keys.guesses]}: {Formatter.Bold($"{stats.AccuracyPercentage}%")} - " +
        $"{loc[Keys.games]}: {Formatter.Bold($"{stats.GamesPlayed}")}";

    private static string Prefix(int position) =>
        position is >= 1 and <= 3 ? Medals[position - 1] : Formatter.Bold($"#{position}");
}
