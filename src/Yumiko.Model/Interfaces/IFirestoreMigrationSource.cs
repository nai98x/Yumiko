using Yumiko.Model.Entities.Migration;

namespace Yumiko.Model.Interfaces;

/// <summary>
/// Read-only view of the Firestore database, used only to move the data to PostgreSQL. It goes away
/// with Firebase once the migration is done.
/// </summary>
public interface IFirestoreMigrationSource
{
    Task<(List<AnilistUserRecord> Records, int Skipped)> ReadAnilistUsersAsync(CancellationToken cancellationToken = default);

    Task<(List<HigherOrLowerRecord> Records, int Skipped)> ReadHigherOrLowerAsync(CancellationToken cancellationToken = default);

    Task<(List<QuizStatsRecord> Records, int Skipped)> ReadQuizStatsAsync(CancellationToken cancellationToken = default);
}
