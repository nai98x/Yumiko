using Yumiko.Model.Entities;
using Yumiko.Model.Enum;

namespace Yumiko.Model.Interfaces.Repositories;

public interface IQuizLeaderboardRepository
{
    Task<List<GameStats>> GetLeaderboardAsync(ulong guildId, Gamemode gamemode, Difficulty difficulty, int limit);

    /// <summary>
    /// En el modo géneros el nombre del género ocupa el lugar del documento de dificultad.
    /// </summary>
    Task<List<GameStats>> GetGenreLeaderboardAsync(ulong guildId, string genre, int limit);

    Task AddResultAsync(ulong guildId, ulong userId, Gamemode gamemode, Difficulty difficulty, int correctRounds, int totalRounds);

    Task DeleteStatsAsync(ulong guildId, ulong userId, Gamemode gamemode);

    Task<List<GameStatsUser>> GetStatsUserAsync(ulong guildId, ulong userId);

    Task<List<GameStats>> GetGenreStatsUserAsync(ulong guildId, ulong userId);
}
