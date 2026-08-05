using System.Text.Json;
using System.Text.Json.Serialization;
using Yumiko.Model.Entities;

namespace Yumiko.Bot.Helpers;

/// <summary>
/// Country catalog for the <c>/weather</c> autocomplete. It is loaded once from
/// <c>Resources/countries.json</c>, relative to the executable directory.
/// </summary>
public sealed class CountriesCatalog
{
    private readonly Lazy<IReadOnlyList<Country>> _countries = new(Load);

    public IReadOnlyList<Country> Countries => _countries.Value;

    public IEnumerable<Country> Search(string? text, int limit = 10) =>
        Countries.Where(p => p.Matches(text)).Take(limit);

    private static IReadOnlyList<Country> Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Resources", "countries.json");

        if (!File.Exists(path))
        {
            return [];
        }

        CountriesFile? file = JsonSerializer.Deserialize<CountriesFile>(File.ReadAllText(path));

        return
        [
            .. (file?.Countries ?? [])
                .Where(c => c.NameEn is not null && c.NameEs is not null && c.Code is not null)
                .Select(c => new Country
                {
                    NameEnglish = c.NameEn!,
                    NameSpanish = c.NameEs!,
                    Code = c.Code!,
                    DialCode = c.DialCode,
                }),
        ];
    }

    private sealed class CountriesFile
    {
        [JsonPropertyName("countries")]
        public List<CountryDto>? Countries { get; set; }
    }

    private sealed class CountryDto
    {
        [JsonPropertyName("name_en")]
        public string? NameEn { get; set; }

        [JsonPropertyName("name_es")]
        public string? NameEs { get; set; }

        [JsonPropertyName("dial_code")]
        public string? DialCode { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }
    }
}
