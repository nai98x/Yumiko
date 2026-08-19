namespace Yumiko.Model.Entities.Migration;

/// <param name="Read">Documents found in Firestore, skipped ones included.</param>
/// <param name="Written">Rows written to PostgreSQL.</param>
/// <param name="Skipped">Documents Firestore had but could not be understood (unknown ids, broken documents).</param>
public sealed record MigrationResult(string Table, int Read, int Written, int Skipped);
