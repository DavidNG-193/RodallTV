using DigitalSignage.Api.DTOs.Weather;

namespace DigitalSignage.Api.Services.Weather;

public interface IDeviceWeatherSettingsService
{
    Task<WeatherSettingDto?> GetAsync(
        Guid deviceId,
        CancellationToken cancellationToken);

    Task<WeatherSettingDto> UpsertAsync(
        Guid deviceId,
        UpdateWeatherSettingRequest request,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Guid deviceId,
        CancellationToken cancellationToken);
}