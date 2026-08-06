using System.Globalization;
using DigitalSignage.Api.Configuration;
using DigitalSignage.Api.Services.ExchangeRates.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DigitalSignage.Api.Services.ExchangeRates;

public sealed class BanxicoExchangeRateProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly BanxicoOptions _options;
    private readonly ILogger<BanxicoExchangeRateProvider> _logger;

    public BanxicoExchangeRateProvider(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        IOptions<BanxicoOptions> options,
        ILogger<BanxicoExchangeRateProvider> logger)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ExchangeRateFetchResult> GetLatestAsync(
        IReadOnlyCollection<string> seriesIds,
        CancellationToken cancellationToken)
    {
        string[] normalized = seriesIds
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        if (normalized.Length == 0)
        {
            DateTime now = DateTime.UtcNow;

            return new ExchangeRateFetchResult(
                now,
                now.AddMinutes(_options.CacheMinutes),
                false,
                new Dictionary<string, ExchangeRateValue>());
        }

        string cacheKey = $"banxico:latest:{string.Join(",", normalized)}";

        if (_memoryCache.TryGetValue(
            cacheKey,
            out ExchangeRateFetchResult? cached)
            && cached is not null
            && cached.ExpiresAtUtc > DateTime.UtcNow)
        {
            _logger.LogInformation(
                "Tasas obtenidas de caché fresca. Series={SeriesIds}",
                string.Join(",", normalized));
            return cached with { IsStale = false };
        }

        try
        {
            _logger.LogInformation(
                "Consultando tasas en Banxico. Series={SeriesIds}",
                string.Join(",", normalized));

            ExchangeRateFetchResult fresh = await FetchFromBanxicoAsync(
                normalized,
                cancellationToken);

            _memoryCache.Set(
                cacheKey,
                fresh,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                });

            return fresh;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "No fue posible consultar las series {SeriesIds} en Banxico.",
                string.Join(",", normalized));

            if (_memoryCache.TryGetValue(
                cacheKey,
                out ExchangeRateFetchResult? stale)
                && stale is not null)
            {
                _logger.LogWarning(
                    "Se usarán tasas vencidas. Series={SeriesIds}",
                    string.Join(",", normalized));
                return stale with { IsStale = true };
            }

            throw;
        }
    }

    private async Task<ExchangeRateFetchResult> FetchFromBanxicoAsync(
        IReadOnlyCollection<string> seriesIds,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiToken))
        {
            throw new InvalidOperationException(
                "No se configuró Banxico:ApiToken.");
        }

        string joinedIds = string.Join(",", seriesIds);
        DateOnly endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly startDate = endDate.AddDays(-Math.Max(_options.HistoryDays, 7));
        string relativeUrl =
            $"series/{joinedIds}/datos/" +
            $"{startDate:yyyy-MM-dd}/{endDate:yyyy-MM-dd}";

        using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.Add("Bmx-Token", _options.ApiToken);

        using HttpResponseMessage response =
            await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Banxico respondió con HTTP {(int)response.StatusCode}.");
        }

        string rawBody = await response.Content.ReadAsStringAsync(
            cancellationToken);

        BanxicoResponse? payload =
            System.Text.Json.JsonSerializer.Deserialize<BanxicoResponse>(
                rawBody,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

        if (payload?.Bmx?.Series is null)
        {
            throw new InvalidOperationException(
                "La respuesta de Banxico no contiene bmx.series.");
        }

        var values = new Dictionary<string, ExchangeRateValue>(
            StringComparer.OrdinalIgnoreCase);

        foreach (BanxicoSeries series in payload.Bmx.Series)
        {
            var observations = series.Datos
                .Select(datum => TryParseObservation(datum, out var parsed)
                    ? parsed
                    : null)
                .Where(item => item is not null)
                .Select(item => item!.Value)
                .OrderByDescending(item => item.EffectiveDate)
                .ToArray();

            if (observations.Length == 0)
            {
                continue;
            }

            var latest = observations[0];
            var previous = observations
                .Skip(1)
                .FirstOrDefault(item => item.EffectiveDate < latest.EffectiveDate);

            decimal? changePercent = previous.Value == 0
                ? null
                : Math.Round(
                    ((latest.Value - previous.Value) / previous.Value) * 100m,
                    4,
                    MidpointRounding.AwayFromZero);

            values[series.IdSerie] = new ExchangeRateValue(
                series.IdSerie,
                latest.Value,
                latest.EffectiveDate,
                changePercent);
        }

        string[] missing = seriesIds
            .Where(x => !values.ContainsKey(x))
            .ToArray();

        if (missing.Length > 0)
        {
            throw new InvalidOperationException(
                $"Banxico no devolvió datos para: {string.Join(", ", missing)}");
        }

        DateTime fetchedAtUtc = DateTime.UtcNow;

        return new ExchangeRateFetchResult(
            fetchedAtUtc,
            fetchedAtUtc.AddMinutes(_options.CacheMinutes),
            false,
            values);
    }

    private static bool TryParseObservation(
        BanxicoDatum datum,
        out (decimal Value, DateOnly EffectiveDate)? observation)
    {
        observation = null;

        if (!decimal.TryParse(
                datum.Dato,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal value)
            || !DateOnly.TryParseExact(
                datum.Fecha,
                "dd/MM/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly effectiveDate))
        {
            return false;
        }

        observation = (value, effectiveDate);
        return true;
    }
}
