using Yumiko.Model.Entities.AnimeThemes;

namespace Yumiko.Model.Interfaces;

public interface IAnimeThemesClient
{
    /// <summary>
    /// Busca animes en animethemes.moe incluyendo sus openings/endings y los videos de cada uno.
    /// </summary>
    Task<List<AnimeAniTheme>> SearchAsync(string search, CancellationToken cancellationToken = default);
}
