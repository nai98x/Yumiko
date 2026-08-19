using System.Data;
using Dapper;
using Yumiko.Infrastructure.Database;
using Yumiko.Infrastructure.Database.Rows;
using Yumiko.Model.Entities;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Infrastructure.Repositories;

internal sealed class QuizLeaderboardRepository(DbConnectionFactory connectionFactory) : IQuizLeaderboardRepository
{
    public Task<List<GameStats>> GetLeaderboardAsync(ulong guildId, Gamemode gamemode, Difficulty difficulty, int limit) =>
        LeaderboardAsync(guildId, Name(gamemode), Name(difficulty), limit);

    public Task<List<GameStats>> GetGenreLeaderboardAsync(ulong guildId, string genre, int limit) =>
        LeaderboardAsync(guildId, Name(Gamemode.Genres), genre, limit);

    private async Task<List<GameStats>> LeaderboardAsync(ulong guildId, string gamemode, string difficulty, int limit)
    {
        using IDbConnection connection = await connectionFactory.OpenConnectionAsync();

        IEnumerable<QuizStatsRow> rows = await connection.QueryAsync<QuizStatsRow>(
            "quiz_stats_leaderboard",
            new
            {
                p_guild_id = (long)guildId,
                p_gamemode = gamemode,
                p_difficulty = difficulty,
                p_limit = limit,
            },
            commandType: CommandType.StoredProcedure);

        return [.. rows.Select(row => Map(row, difficultyName: null))];
    }

    public async Task AddResultAsync(ulong guildId, ulong userId, Gamemode gamemode, Difficulty difficulty, int correctRounds, int totalRounds)
    {
        using IDbConnection connection = await connectionFactory.OpenConnectionAsync();

        await connection.ExecuteAsync(
            "quiz_stats_add_result",
            new
            {
                p_guild_id = (long)guildId,
                p_user_id = (long)userId,
                p_gamemode = Name(gamemode),
                p_difficulty = Name(difficulty),
                p_correct_rounds = correctRounds,
                p_total_rounds = totalRounds,
            },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteStatsAsync(ulong guildId, ulong userId, Gamemode gamemode)
    {
        using IDbConnection connection = await connectionFactory.OpenConnectionAsync();

        await connection.ExecuteAsync(
            "quiz_stats_delete",
            new { p_guild_id = (long)guildId, p_user_id = (long)userId, p_gamemode = Name(gamemode) },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<List<GameStatsUser>> GetStatsUserAsync(ulong guildId, ulong userId)
    {
        List<QuizStatsRow> rows = await UserRowsAsync(guildId, userId);

        return
        [
            .. System.Enum.GetValues<Gamemode>().Select(gamemode => new GameStatsUser
            {
                Gamemode = gamemode,
                Stats =
                [
                    .. System.Enum.GetValues<Difficulty>()
                        .Select(difficulty => rows.Find(row => row.Gamemode == Name(gamemode) && row.Difficulty == Name(difficulty)))
                        .Where(row => row is not null)
                        .Select(row => Map(row!, row!.Difficulty)),
                ],
            }),
        ];
    }

    public async Task<List<GameStats>> GetGenreStatsUserAsync(ulong guildId, ulong userId)
    {
        List<QuizStatsRow> rows = await UserRowsAsync(guildId, userId);

        // In genres mode the difficulty column holds the genre name, which is what gets shown.
        return
        [
            .. rows
                .Where(row => row.Gamemode == Name(Gamemode.Genres))
                .OrderBy(row => row.Difficulty, StringComparer.Ordinal)
                .Select(row => Map(row, row.Difficulty)),
        ];
    }

    private async Task<List<QuizStatsRow>> UserRowsAsync(ulong guildId, ulong userId)
    {
        using IDbConnection connection = await connectionFactory.OpenConnectionAsync();

        IEnumerable<QuizStatsRow> rows = await connection.QueryAsync<QuizStatsRow>(
            "quiz_stats_user",
            new { p_guild_id = (long)guildId, p_user_id = (long)userId },
            commandType: CommandType.StoredProcedure);

        return [.. rows];
    }

    private static string Name<T>(T value)
        where T : struct, System.Enum =>
        System.Enum.GetName(value)!;

    private static GameStats Map(QuizStatsRow row, string? difficultyName) => new()
    {
        UserId = row.UserId,
        GamesPlayed = row.GamesPlayed,
        TotalRounds = row.TotalRounds,
        CorrectRounds = row.CorrectRounds,
        AccuracyPercentage = row.AccuracyPercentage,
        DifficultyName = difficultyName,
    };
}
