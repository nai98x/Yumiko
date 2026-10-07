using Yumiko.Model.Entities;

namespace Yumiko.Model.Interfaces.Repositories;

/// <summary>Records of the Higher or Lower duo mode: one per pair of users, regardless of their order.</summary>
public interface IHigherOrLowerDuoLeaderboardRepository
{
    Task<List<HigherOrLowerDuoEntry>> GetLeaderboardAsync(ulong guildId);

    /// <summary>Best pairs the user takes part of, best first.</summary>
    Task<List<HigherOrLowerDuoEntry>> GetStatsUserAsync(ulong guildId, ulong userId, int limit);

    /// <returns><c>true</c> if the score beat the previous record of the pair and was saved.</returns>
    Task<bool> AddResultAsync(ulong guildId, ulong userId1, ulong userId2, int score);

    /// <summary>Deletes every pair the user takes part of.</summary>
    Task DeleteStatsAsync(ulong guildId, ulong userId);
}
