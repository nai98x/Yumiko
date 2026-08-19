using System.Data;
using Dapper;
using Yumiko.Infrastructure.Database;
using Yumiko.Model.Entities.Migration;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Infrastructure.Repositories;

internal sealed class MigrationRepository(DbConnectionFactory connectionFactory) : IMigrationRepository
{
    // The import stored procedures take arrays: batching keeps the whole migration from travelling
    // in a single statement without falling back to one round trip per row.
    private const int BatchSize = 500;

    public Task<int> ImportAnilistUsersAsync(IReadOnlyList<AnilistUserRecord> records, CancellationToken cancellationToken = default) =>
        ImportAsync(records, "anilist_users_import", cancellationToken, batch => new
        {
            p_user_ids = batch.Select(r => (long)r.UserId).ToArray(),
            p_anilist_ids = batch.Select(r => r.AnilistId).ToArray(),
        });

    public Task<int> ImportHigherOrLowerAsync(IReadOnlyList<HigherOrLowerRecord> records, CancellationToken cancellationToken = default) =>
        ImportAsync(records, "higher_or_lower_import", cancellationToken, batch => new
        {
            p_guild_ids = batch.Select(r => (long)r.GuildId).ToArray(),
            p_user_ids = batch.Select(r => (long)r.UserId).ToArray(),
            p_scores = batch.Select(r => r.Score).ToArray(),
        });

    public Task<int> ImportQuizStatsAsync(IReadOnlyList<QuizStatsRecord> records, CancellationToken cancellationToken = default) =>
        ImportAsync(records, "quiz_stats_import", cancellationToken, batch => new
        {
            p_guild_ids = batch.Select(r => (long)r.GuildId).ToArray(),
            p_user_ids = batch.Select(r => (long)r.UserId).ToArray(),
            p_gamemodes = batch.Select(r => System.Enum.GetName(r.Gamemode)!).ToArray(),
            p_difficulties = batch.Select(r => r.Difficulty).ToArray(),
            p_games_played = batch.Select(r => r.GamesPlayed).ToArray(),
            p_correct = batch.Select(r => r.CorrectRounds).ToArray(),
            p_total = batch.Select(r => r.TotalRounds).ToArray(),
            p_accuracy = batch.Select(r => r.AccuracyPercentage).ToArray(),
        });

    private async Task<int> ImportAsync<T>(IReadOnlyList<T> records, string procedure, CancellationToken cancellationToken, Func<T[], object> parameters)
    {
        if (records.Count == 0)
        {
            return 0;
        }

        using IDbConnection connection = await connectionFactory.OpenConnectionAsync();

        int imported = 0;

        foreach (T[] batch in records.Chunk(BatchSize))
        {
            imported += await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                procedure,
                parameters(batch),
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        }

        return imported;
    }
}
