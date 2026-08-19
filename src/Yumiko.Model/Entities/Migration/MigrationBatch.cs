namespace Yumiko.Model.Entities.Migration;

/// <param name="Section">What was read: the collection name, or the guild id inside it.</param>
/// <param name="Skipped">Documents the section had but could not be understood.</param>
public sealed record MigrationBatch<T>(string Section, IReadOnlyList<T> Records, int Skipped);
