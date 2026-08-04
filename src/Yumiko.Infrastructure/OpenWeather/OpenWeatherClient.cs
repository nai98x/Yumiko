using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Web;
using Yumiko.Model.Entities;
using Yumiko.Model.Interfaces;

namespace Yumiko.Infrastructure.OpenWeather;

internal sealed class OpenWeatherClient(HttpClient http, string apiKey) : IWeatherClient
{
    public async Task<Weather?> GetWeatherAsync(string city, string country, string language, CancellationToken cancellationToken = default)
    {
        string url = $"weather?q={HttpUtility.UrlEncode(city)},{country}&appid={apiKey}&lang={language}&units=metric";

        using HttpResponseMessage response = await http.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        WeatherResponse? data = await response.Content.ReadFromJsonAsync<WeatherResponse>(cancellationToken);
        if (data is null)
        {
            return null;
        }

        return new Weather
        {
            CityId = data.Id,
            CityName = data.Name ?? city,
            Description = data.Weather?.FirstOrDefault()?.Description,
            Temperature = data.Main?.Temp ?? 0,
            TemperatureMin = data.Main?.TempMin ?? 0,
            TemperatureMax = data.Main?.TempMax ?? 0,
            FeelsLike = data.Main?.FeelsLike ?? 0,
            Humidity = data.Main?.Humidity ?? 0,
            Pressure = data.Main?.Pressure ?? 0,
            WindSpeed = data.Wind?.Speed ?? 0,
            Sunrise = data.Sys?.Sunrise ?? 0,
            Sunset = data.Sys?.Sunset ?? 0,
        };
    }
}

internal sealed class WeatherResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("weather")]
    public List<WeatherCondition>? Weather { get; set; }

    [JsonPropertyName("main")]
    public WeatherMain? Main { get; set; }

    [JsonPropertyName("wind")]
    public WeatherWind? Wind { get; set; }

    [JsonPropertyName("sys")]
    public WeatherSys? Sys { get; set; }
}

internal sealed class WeatherCondition
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

internal sealed class WeatherMain
{
    [JsonPropertyName("temp")]
    public decimal Temp { get; set; }

    [JsonPropertyName("feels_like")]
    public decimal FeelsLike { get; set; }

    [JsonPropertyName("temp_min")]
    public decimal TempMin { get; set; }

    [JsonPropertyName("temp_max")]
    public decimal TempMax { get; set; }

    [JsonPropertyName("pressure")]
    public int Pressure { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}

internal sealed class WeatherWind
{
    [JsonPropertyName("speed")]
    public decimal Speed { get; set; }
}

internal sealed class WeatherSys
{
    [JsonPropertyName("sunrise")]
    public long Sunrise { get; set; }

    [JsonPropertyName("sunset")]
    public long Sunset { get; set; }
}
