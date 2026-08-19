using Yumiko.Model.Entities.Migration;
using Yumiko.Model.Interfaces;

namespace Yumiko.Application.Tests.Migration;

/// <summary>
/// Double of <see cref="IFirestoreMigrationSource"/>. Every collection is empty unless the test
/// fills it.
/// </summary>
internal sealed class FakeFirestoreMigrationSource : IFirestoreMigrationSource
{
    public List<AnilistUserRecord> AnilistUsers { get; init; } = [];

    public List<HigherOrLowerRecord> HigherOrLower { get; init; } = [];

    public List<QuizStatsRecord> QuizStats { get; init; } = [];

    public int Skipped { get; init; }

    public Task<(List<AnilistUserRecord> Records, int Skipped)> ReadAnilistUsersAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult((AnilistUsers, Skipped));

    public Task<(List<HigherOrLowerRecord> Records, int Skipped)> ReadHigherOrLowerAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult((HigherOrLower, Skipped));

    public Task<(List<QuizStatsRecord> Records, int Skipped)> ReadQuizStatsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult((QuizStats, Skipped));
}
