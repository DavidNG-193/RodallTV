namespace DigitalSignage.Api.DTOs.ExchangeRates;

public sealed record DeviceExchangeRateSettingDto(
    Guid Id,
    Guid DeviceId,
    string SeriesId,
    string DisplayName,
    string Unit,
    int Position,
    bool IsActive);