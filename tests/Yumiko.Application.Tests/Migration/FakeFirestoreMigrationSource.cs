using Yumiko.Model.Entities.Migration;
using Yumiko.Model.Interfaces;

namespace Yumiko.Application.Tests.Migration;

/// <summary>
/// Double of <see cref="IFirestoreMigrationSource"/>. Every collection hands over the sections the
/// test loads into it, in order.
/// </summary>
internal sealed class FakeFirestoreMigrationSource : IFirestoreMigrationSource
{
    public List<MigrationBatch<AnilistUserRecord>> AnilistUsers { get; init; } = [];

    public List<MigrationBatch<HigherOrLowerRecord>> HigherOrLower { get; init; } = [];

    public List<MigrationBatch<QuizStatsRecord>> QuizStats { get; init; } = [];

    public IAsyncEnumerable<MigrationBatch<AnilistUserRecord>> ReadAnilistUsersAsync(CancellationToken cancellationToken = default) =>
        Stream(AnilistUsers);

    public IAsyncEnumerable<MigrationBatch<HigherOrLowerRecord>> ReadHigherOrLowerAsync(CancellationToken cancellationToken = default) =>
        Stream(HigherOrLower);

    public IAsyncEnumerable<MigrationBatch<QuizStatsRecord>> ReadQuizStatsAsync(CancellationToken cancellationToken = default) =>
        Stream(QuizStats);

    private static async IAsyncEnumerable<MigrationBatch<T>> Stream<T>(List<MigrationBatch<T>> batches)
    {
        foreach (MigrationBatch<T> batch in batches)
        {
            yield return batch;
        }

        await Task.CompletedTask;
    }
}
