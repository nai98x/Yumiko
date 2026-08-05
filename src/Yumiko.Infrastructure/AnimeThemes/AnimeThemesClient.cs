using System.Text;
using Newtonsoft.Json;
using Yumiko.Infrastructure.AnimeThemes.Responses;
using Yumiko.Model.Entities.AnimeThemes;
using Yumiko.Model.Interfaces;

namespace Yumiko.Infrastructure.AnimeThemes;

internal sealed class AnimeThemesClient(HttpClient http) : IAnimeThemesClient
{
    // The search only feeds a Discord select menu, which holds up to 25 options.
    private const int ResultsPerSearch = 25;

    public async Task<List<AnimeAniTheme>> SearchAsync(string search, CancellationToken cancellationToken = default)
    {
        string body = JsonConvert.SerializeObject(new
        {
            query = AnimeThemesQueries.Search,
            variables = new { search, first = ResultsPerSearch },
        });

        using StringContent content = new(body, Encoding.UTF8, "application/json");
        using HttpResponseMessage response = await http.PostAsync(string.Empty, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        string payload = await response.Content.ReadAsStringAsync(cancellationToken);
        GraphQLEnvelope<SearchResponse>? envelope = JsonConvert.DeserializeObject<GraphQLEnvelope<SearchResponse>>(payload);

        // GraphQL level errors arrive with HTTP 200 and an "errors" array.
        if (envelope?.Errors is { Count: > 0 } errors)
        {
            string detail = string.Join("; ", errors.Select(e => e.Message));
            throw new HttpRequestException($"animethemes.moe returned errors: {detail}");
        }

        return AnimeThemesMapper.ToAnime(envelope?.Data?.Search?.Anime);
    }
}
