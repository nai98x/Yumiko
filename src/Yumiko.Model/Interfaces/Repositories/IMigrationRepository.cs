using Yumiko.Model.Entities.Migration;

namespace Yumiko.Model.Interfaces.Repositories;

/// <summary>
/// Bulk import of the data coming from Firestore. Every method is idempotent: importing twice leaves
/// the same rows, so a failed run can just be repeated.
/// </summary>
public interface IMigrationRepository
{
    Task<int> ImportAnilistUsersAsync(IReadOnlyList<AnilistUserRecord> records, CancellationToken cancellationToken = default);

    Task<int> ImportHigherOrLowerAsync(IReadOnlyList<HigherOrLowerRecord> records, CancellationToken cancellationToken = default);

    Task<int> ImportQuizStatsAsync(IReadOnlyList<QuizStatsRecord> records, CancellationToken cancellationToken = default);
}
