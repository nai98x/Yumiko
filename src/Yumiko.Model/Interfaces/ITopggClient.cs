namespace Yumiko.Model.Interfaces;

public interface ITopggClient
{
    Task<bool> HasVotedAsync(ulong applicationId, ulong userId, CancellationToken cancellationToken = default);

    Task<int> GetMonthlyVotesCountAsync(ulong applicationId, CancellationToken cancellationToken = default);

    Task UpdateStatsAsync(ulong applicationId, int guildCount, int shardCount, CancellationToken cancellationToken = default);
}
