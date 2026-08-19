using Yumiko.Model.Enum;

namespace Yumiko.Model.Entities.Migration;

/// <param name="Difficulty">
/// The name of a <see cref="Enum.Difficulty"/>, or the genre name when <paramref name="Gamemode"/>
/// is <see cref="Enum.Gamemode.Genres"/>.
/// </param>
public sealed record QuizStatsRecord(
    ulong GuildId,
    ulong UserId,
    Gamemode Gamemode,
    string Difficulty,
    int GamesPlayed,
    int CorrectRounds,
    int TotalRounds,
    int AccuracyPercentage);
