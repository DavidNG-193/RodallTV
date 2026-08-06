using DigitalSignage.Api.Services.ExchangeRates.Models;

namespace DigitalSignage.Api.Services.ExchangeRates;

public interface IExchangeRateProvider
{
    Task<ExchangeRateFetchResult> GetLatestAsync(
        IReadOnlyCollection<string> seriesIds,
        CancellationToken cancellationToken);
}