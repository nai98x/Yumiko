using Yumiko.Model.Entities.Migration;
using Yumiko.Model.Interfaces.Repositories;

namespace Yumiko.Application.Tests.Migration;

/// <summary>
/// Double of <see cref="IMigrationRepository"/>. It writes nowhere: it keeps what it was handed so
/// the test can assert what would have been imported.
/// </summary>
internal sealed class FakeMigrationRepository : IMigrationRepository
{
    public List<AnilistUserRecord> AnilistUsers { get; } = [];

    public List<HigherOrLowerRecord> HigherOrLower { get; } = [];

    public List<QuizStatsRecord> QuizStats { get; } = [];

    public Task<int> ImportAnilistUsersAsync(IReadOnlyList<AnilistUserRecord> records, CancellationToken cancellationToken = default)
    {
        AnilistUsers.AddRange(records);
        return Task.FromResult(records.Count);
    }

    public Task<int> ImportHigherOrLowerAsync(IReadOnlyList<HigherOrLowerRecord> records, CancellationToken cancellationToken = default)
    {
        HigherOrLower.AddRange(records);
        return Task.FromResult(records.Count);
    }

    public Task<int> ImportQuizStatsAsync(IReadOnlyList<QuizStatsRecord> records, CancellationToken cancellationToken = default)
    {
        QuizStats.AddRange(records);
        return Task.FromResult(records.Count);
    }
}
