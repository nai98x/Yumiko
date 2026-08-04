using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Yumiko.Model.Interfaces;

namespace Yumiko.Infrastructure.Topgg;

internal sealed class TopggClient(HttpClient http) : ITopggClient
{
    public async Task<bool> HasVotedAsync(ulong applicationId, ulong userId, CancellationToken cancellationToken = default)
    {
        VotedResponse? response = await http.GetFromJsonAsync<VotedResponse>(
            $"bots/{applicationId}/check?userId={userId}",
            cancellationToken);

        return response?.Voted == 1;
    }

    public async Task<int> GetMonthlyVotesCountAsync(ulong applicationId, CancellationToken cancellationToken = default)
    {
        List<VoterResponse>? voters = await http.GetFromJsonAsync<List<VoterResponse>>(
            $"bots/{applicationId}/votes",
            cancellationToken);

        return voters?.Count ?? 0;
    }

    public async Task UpdateStatsAsync(ulong applicationId, int guildCount, int shardCount, CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await http.PostAsJsonAsync(
            $"bots/{applicationId}/stats",
            new StatsRequest { ServerCount = guildCount, ShardCount = shardCount },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}

internal sealed class VotedResponse
{
    [JsonPropertyName("voted")]
    public int Voted { get; set; }
}

internal sealed class VoterResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

internal sealed class StatsRequest
{
    [JsonPropertyName("server_count")]
    public int ServerCount { get; set; }

    [JsonPropertyName("shard_count")]
    public int ShardCount { get; set; }
}
