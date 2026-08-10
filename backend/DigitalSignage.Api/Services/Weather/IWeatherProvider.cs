using DigitalSignage.Api.Services.Weather.Models;

namespace DigitalSignage.Api.Services.Weather;

public interface IWeatherProvider
{
    Task<WeatherProviderResult> GetCurrentAsync(
        double latitude,
        double longitude,
        string timezone,
        CancellationToken cancellationToken);
}