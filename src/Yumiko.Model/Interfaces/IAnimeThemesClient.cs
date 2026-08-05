using Yumiko.Model.Entities.AnimeThemes;

namespace Yumiko.Model.Interfaces;

public interface IAnimeThemesClient
{
    /// <summary>
    /// Searches animes on animethemes.moe including their openings/endings and the videos of each one.
    /// </summary>
    Task<List<AnimeAniTheme>> SearchAsync(string search, CancellationToken cancellationToken = default);
}
