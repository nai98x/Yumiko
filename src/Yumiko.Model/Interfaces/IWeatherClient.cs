using Yumiko.Model.Entities;

namespace Yumiko.Model.Interfaces;

public interface IWeatherClient
{
    /// <returns><c>null</c> if the location does not exist.</returns>
    Task<Weather?> GetWeatherAsync(string city, string country, string language, CancellationToken cancellationToken = default);
}
