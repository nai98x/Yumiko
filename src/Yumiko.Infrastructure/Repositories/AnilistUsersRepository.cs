using System.Data;
using Dapper;
using Yumiko.Infrastructure.Database;
using Yumiko.Infrastructure.Database.Rows;
using Yumiko.Model.Entities;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Infrastructure.Repositories;

internal sealed class AnilistUsersRepository(DbConnectionFactory connectionFactory) : IAnilistUsersRepository
{
    public async Task<AnilistUserLink?> GetLinkAsync(ulong userId)
    {
        using IDbConnection connection = await connectionFactory.OpenConnectionAsync();

        AnilistUserRow? row = await connection.QuerySingleOrDefaultAsync<AnilistUserRow>(
            "anilist_user_get",
            new { p_user_id = (long)userId },
            commandType: CommandType.StoredProcedure);

        return row is null ? null : new AnilistUserLink
        {
            AnilistId = row.AnilistId,
            UserId = (ulong)row.UserId,
        };
    }

    public async Task SetAnilistAsync(int anilistId, ulong userId)
    {
        using IDbConnection connection = await connectionFactory.OpenConnectionAsync();

        await connection.ExecuteAsync(
            "anilist_user_upsert",
            new { p_user_id = (long)userId, p_anilist_id = anilistId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> DeleteAnilistAsync(ulong userId)
    {
        using IDbConnection connection = await connectionFactory.OpenConnectionAsync();

        return await connection.QuerySingleAsync<bool>(
            "anilist_user_delete",
            new { p_user_id = (long)userId },
            commandType: CommandType.StoredProcedure);
    }
}
