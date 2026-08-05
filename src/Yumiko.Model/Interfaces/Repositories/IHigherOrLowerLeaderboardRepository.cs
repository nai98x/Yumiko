using Yumiko.Model.Entities;

namespace Yumiko.Model.Interfaces.Repositories;

public interface IHigherOrLowerLeaderboardRepository
{
    Task<List<HigherOrLowerEntry>> GetLeaderboardAsync(ulong guildId);

    Task<HigherOrLowerEntry?> GetStatsUserAsync(ulong guildId, ulong userId);

    /// <returns><c>true</c> if the score beat the previous record and was saved.</returns>
    Task<bool> AddResultAsync(ulong guildId, ulong userId, int score);

    Task DeleteStatsAsync(ulong guildId, ulong userId);
}
