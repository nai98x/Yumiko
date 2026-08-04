namespace Yumiko.Model.Entities;

public class Weather
{
    public required long CityId { get; init; }

    public required string CityName { get; init; }

    public string? Description { get; init; }

    public decimal Temperature { get; init; }

    public decimal TemperatureMin { get; init; }

    public decimal TemperatureMax { get; init; }

    public decimal FeelsLike { get; init; }

    public int Humidity { get; init; }

    public int Pressure { get; init; }

    public decimal WindSpeed { get; init; }

    public long Sunrise { get; init; }

    public long Sunset { get; init; }
}
