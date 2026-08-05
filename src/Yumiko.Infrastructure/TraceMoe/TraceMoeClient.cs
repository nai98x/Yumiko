using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Yumiko.Model.Entities;
using Yumiko.Model.Exceptions;
using Yumiko.Model.Interfaces;

namespace Yumiko.Infrastructure.TraceMoe;

internal sealed class TraceMoeClient(HttpClient http) : ITraceMoeClient
{
    public async Task<List<TraceMoeMatch>> SearchAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await http.GetAsync($"search?url={HttpUtility.UrlEncode(imageUrl)}", cancellationToken);

        // 402 = quota depleted, 429 = too many searches. In both cases there are no results to show.
        if (response.StatusCode is HttpStatusCode.PaymentRequired or HttpStatusCode.TooManyRequests)
        {
            throw new TraceMoeQuotaException((int)response.StatusCode);
        }

        response.EnsureSuccessStatusCode();

        TraceMoeResponse? data = await response.Content.ReadFromJsonAsync<TraceMoeResponse>(cancellationToken);

        return [.. (data?.Result ?? []).Select(r => new TraceMoeMatch
        {
            AnilistId = r.Anilist,
            Episode = FormatearEpisodio(r.Episode),
            Similarity = r.Similarity,
            From = r.From,
            Video = r.Video,
        })];
    }

    // trace.moe sends the episode as a number, an array of numbers or null depending on the case.
    private static string? FormatearEpisodio(JsonElement episode) => episode.ValueKind switch
    {
        JsonValueKind.Undefined or JsonValueKind.Null => null,
        JsonValueKind.String => episode.GetString(),
        _ => episode.ToString(),
    };
}

internal sealed class TraceMoeResponse
{
    [JsonPropertyName("result")]
    public List<TraceMoeResult>? Result { get; set; }
}

internal sealed class TraceMoeResult
{
    [JsonPropertyName("anilist")]
    public int Anilist { get; set; }

    [JsonPropertyName("episode")]
    public JsonElement Episode { get; set; }

    [JsonPropertyName("similarity")]
    public double Similarity { get; set; }

    [JsonPropertyName("from")]
    public double From { get; set; }

    [JsonPropertyName("video")]
    public string? Video { get; set; }
}
