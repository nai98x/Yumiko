using System.Data;
using Dapper;
using Yumiko.Infrastructure.Database;
using Yumiko.Infrastructure.Database.Rows;
using Yumiko.Model.Entities;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Infrastructure.Repositories;

internal sealed class HigherOrLowerDuoLeaderboardRepository(DbConnectionFactory connectionFactory) : IHigherOrLowerDuoLeaderboardRepository
{
    private const int LeaderboardSize = 20;

    public async Task<List<HigherOrLowerDuoEntry>> GetLeaderboardAsync(ulong guildId)
    {
        using IDbConnection connection = await connectionFactory.OpenConnectionAsync();

        IEnumerable<HigherOrLowerDuoRow> rows = await connection.QueryAsync<HigherOrLowerDuoRow>(
            "higher_or_lower_duo_leaderboard",
            new { p_guild_id = (long)guildId, p_limit = LeaderboardSize },
            commandType: CommandType.StoredProcedure);

        return [.. rows.Select(Map)];
    }

    public async Task<List<HigherOrLowerDuoEntry>> GetStatsUserAsync(ulong guildId, ulong userId, int limit)
    {
        using IDbConnection connection = await connectionFactory.OpenConnectionAsync();

        IEnumerable<HigherOrLowerDuoRow> rows = await connection.QueryAsync<HigherOrLowerDuoRow>(
            "higher_or_lower_duo_user_get",
            new { p_guild_id = (long)guildId, p_user_id = (long)userId, p_limit = limit },
            commandType: CommandType.StoredProcedure);

        return [.. rows.Select(Map)];
    }

    public async Task<bool> AddResultAsync(ulong guildId, ulong userId1, ulong userId2, int score)
    {
        using IDbConnection connection = await connectionFactory.OpenConnectionAsync();

        return await connection.QuerySingleAsync<bool>(
            "higher_or_lower_duo_add_result",
            new { p_guild_id = (long)guildId, p_user_id1 = (long)userId1, p_user_id2 = (long)userId2, p_score = score },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteStatsAsync(ulong guildId, ulong userId)
    {
        using IDbConnection connection = await connectionFactory.OpenConnectionAsync();

        await connection.ExecuteAsync(
            "higher_or_lower_duo_delete",
            new { p_guild_id = (long)guildId, p_user_id = (long)userId },
            commandType: CommandType.StoredProcedure);
    }

    private static HigherOrLowerDuoEntry Map(HigherOrLowerDuoRow row) => new()
    {
        FirstUserId = (ulong)row.FirstUserId,
        SecondUserId = (ulong)row.SecondUserId,
        Score = row.Score,
    };
}
