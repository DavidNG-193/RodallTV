using DigitalSignage.Api.DTOs.ExchangeRates;

namespace DigitalSignage.Api.Services.ExchangeRates;

public interface IDeviceExchangeRateSettingsService
{
    Task<IReadOnlyList<ExchangeRateSeriesOptionDto>> GetCatalogAsync();

    Task<IReadOnlyList<DeviceExchangeRateSettingDto>> GetByDeviceAsync(
        Guid deviceId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DeviceExchangeRateSettingDto>> ReplaceAsync(
        Guid deviceId,
        UpdateDeviceExchangeRateSettingsRequest request,
        CancellationToken cancellationToken);
}