namespace DigitalSignage.Api.Services.ExchangeRates.Models;

public sealed record ExchangeRateFetchResult(
    DateTime FetchedAtUtc,
    DateTime ExpiresAtUtc,
    bool IsStale,
    IReadOnlyDictionary<string, ExchangeRateValue> Values);