using Yumiko.Model.Entities.Migration;

namespace Yumiko.Model.Interfaces;

/// <summary>
/// Read-only view of the Firestore database, used only to move the data to PostgreSQL. It goes away
/// with Firebase once the migration is done.
/// </summary>
/// <remarks>
/// Everything is handed over in sections instead of one big list so the migration can import and
/// report as it goes: reading the whole database takes far longer than a Discord interaction lives.
/// </remarks>
public interface IFirestoreMigrationSource
{
    IAsyncEnumerable<MigrationBatch<AnilistUserRecord>> ReadAnilistUsersAsync(CancellationToken cancellationToken = default);

    IAsyncEnumerable<MigrationBatch<HigherOrLowerRecord>> ReadHigherOrLowerAsync(CancellationToken cancellationToken = default);

    IAsyncEnumerable<MigrationBatch<QuizStatsRecord>> ReadQuizStatsAsync(CancellationToken cancellationToken = default);
}
