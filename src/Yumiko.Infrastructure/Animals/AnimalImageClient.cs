using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Yumiko.Model.Enum;
using Yumiko.Model.Interfaces;

namespace Yumiko.Infrastructure.Animals;

// TheCatAPI y TheDogAPI son la misma API con distinto host y token.
internal sealed class AnimalImageClient(HttpClient http, string catApiKey, string dogApiKey) : IAnimalImageClient
{
    public async Task<byte[]?> GetRandomImageAsync(AnimalKind kind, CancellationToken cancellationToken = default)
    {
        (string host, string apiKey) = kind switch
        {
            AnimalKind.Cat => ("https://api.thecatapi.com", catApiKey),
            AnimalKind.Dog => ("https://api.thedogapi.com", dogApiKey),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

        using HttpRequestMessage request = new(HttpMethod.Get, $"{host}/v1/images/search?limit=1");
        request.Headers.Add("x-api-key", apiKey);

        using HttpResponseMessage response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        List<AnimalImageResponse>? data = await response.Content.ReadFromJsonAsync<List<AnimalImageResponse>>(cancellationToken);
        string? imageUrl = data?.FirstOrDefault()?.Url;

        if (string.IsNullOrEmpty(imageUrl))
        {
            return null;
        }

        return await http.GetByteArrayAsync(imageUrl, cancellationToken);
    }
}

internal sealed class AnimalImageResponse
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}
