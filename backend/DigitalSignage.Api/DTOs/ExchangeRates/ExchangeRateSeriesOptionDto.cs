namespace DigitalSignage.Api.DTOs.ExchangeRates;

public sealed record ExchangeRateSeriesOptionDto(
    string SeriesId,
    string DisplayName,
    string Unit);