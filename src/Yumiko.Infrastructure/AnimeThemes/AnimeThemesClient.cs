using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json;
using Yumiko.Infrastructure.AnimeThemes.Responses;
using Yumiko.Model.Entities.AnimeThemes;
using Yumiko.Model.Interfaces;

namespace Yumiko.Infrastructure.AnimeThemes;

internal sealed class AnimeThemesClient(HttpClient http) : IAnimeThemesClient
{
    public async Task<List<AnimeAniTheme>> SearchAsync(string search, CancellationToken cancellationToken = default)
    {
        string url = $"anime?q={HttpUtility.UrlEncode(search)}&include=animethemes.animethemeentries.videos";

        using HttpResponseMessage response = await http.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        SearchResponse? data = JsonConvert.DeserializeObject<SearchResponse>(content);

        return data?.Anime ?? [];
    }
}
