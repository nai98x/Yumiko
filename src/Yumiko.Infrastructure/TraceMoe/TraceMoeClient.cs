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

        // 400 and 404 both mean trace.moe could not download the image of the given link.
        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
        {
            TraceMoeResponse? failure = await ReadResponseAsync(response, cancellationToken);
            throw new TraceMoeImageFetchException((int)response.StatusCode, failure?.Error ?? "could not fetch the image.");
        }

        response.EnsureSuccessStatusCode();

        TraceMoeResponse? data = await ReadResponseAsync(response, cancellationToken);

        return [.. (data?.Result ?? []).Select(r => new TraceMoeMatch
        {
            AnilistId = r.Anilist,
            Episode = FormatEpisode(r.Episode),
            Similarity = r.Similarity,
            From = r.From,
            Video = r.Video,
        })];
    }

    private static Task<TraceMoeResponse?> ReadResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
        response.Content.ReadFromJsonAsync<TraceMoeResponse>(cancellationToken);

    // trace.moe sends the episode as a number, an array of numbers or null depending on the case.
    private static string? FormatEpisode(JsonElement episode) => episode.ValueKind switch
    {
        JsonValueKind.Undefined or JsonValueKind.Null => null,
        JsonValueKind.String => episode.GetString(),
        _ => episode.ToString(),
    };
}

internal sealed class TraceMoeResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }

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
