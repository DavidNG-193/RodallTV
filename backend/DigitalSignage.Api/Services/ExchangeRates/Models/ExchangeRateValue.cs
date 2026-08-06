namespace DigitalSignage.Api.Services.ExchangeRates.Models;

public sealed record ExchangeRateValue(
    string SeriesId,
    decimal Value,
    DateOnly EffectiveDate,
    decimal? ChangePercent);
