using System.Data;
using Dapper;
using Yumiko.Infrastructure.Database;
using Yumiko.Infrastructure.Database.Rows;
using Yumiko.Model.Entities;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Infrastructure.Repositories;

internal sealed class HigherOrLowerLeaderboardRepository(DbConnectionFactory connectionFactory) : IHigherOrLowerLeaderboardRepository
{
    private const int LeaderboardSize = 20;

    public async Task<List<HigherOrLowerEntry>> GetLeaderboardAsync(ulong guildId)
    {
        using IDbConnection connection = await connectionFactory.OpenConnectionAsync();

        IEnumerable<HigherOrLowerRow> rows = await connection.QueryAsync<HigherOrLowerRow>(
            "higher_or_lower_leaderboard",
            new { p_guild_id = (long)guildId, p_limit = LeaderboardSize },
            commandType: CommandType.StoredProcedure);

        return [.. rows.Select(Map)];
    }

    public async Task<HigherOrLowerEntry?> GetStatsUserAsync(ulong guildId, ulong userId)
    {
        using IDbConnection connection = await connectionFactory.OpenConnectionAsync();

        HigherOrLowerRow? row = await connection.QuerySingleOrDefaultAsync<HigherOrLowerRow>(
            "higher_or_lower_user_get",
            new { p_guild_id = (long)guildId, p_user_id = (long)userId },
            commandType: CommandType.StoredProcedure);

        return row is null ? null : Map(row);
    }

    public async Task<bool> AddResultAsync(ulong guildId, ulong userId, int score)
    {
        using IDbConnection connection = await connectionFactory.OpenConnectionAsync();

        return await connection.QuerySingleAsync<bool>(
            "higher_or_lower_add_result",
            new { p_guild_id = (long)guildId, p_user_id = (long)userId, p_score = score },
            commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteStatsAsync(ulong guildId, ulong userId)
    {
        using IDbConnection connection = await connectionFactory.OpenConnectionAsync();

        await connection.ExecuteAsync(
            "higher_or_lower_delete",
            new { p_guild_id = (long)guildId, p_user_id = (long)userId },
            commandType: CommandType.StoredProcedure);
    }

    private static HigherOrLowerEntry Map(HigherOrLowerRow row) => new()
    {
        UserId = (ulong)row.UserId,
        Score = row.Score,
    };
}
