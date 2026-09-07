using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DigitalSignage.Api.Configuration;
using DigitalSignage.Api.Services.ExchangeRates.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DigitalSignage.Api.Services.ExchangeRates;

public sealed class BanxicoExchangeRateProvider : IExchangeRateProvider
{
    private const string CacheKey = "banxico:latest:catalog";
    private const string FailureCacheKey = "banxico:latest:failure";
    private static readonly ConcurrentDictionary<string, SemaphoreSlim>
        RefreshLocks = new();

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
        string[] requestedSeries = seriesIds
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct()
            .OrderBy(x => x)
            .ToArray();

        if (requestedSeries.Length == 0)
        {
            DateTime now = DateTime.UtcNow;

            return new ExchangeRateFetchResult(
                now,
                now.AddMinutes(_options.CacheMinutes),
                false,
                new Dictionary<string, ExchangeRateValue>());
        }

        string[] unsupported = requestedSeries
            .Where(seriesId =>
                !ExchangeRateSeriesCatalog.TryGet(seriesId, out _))
            .ToArray();

        if (unsupported.Length > 0)
        {
            throw new InvalidOperationException(
                $"Series de Banxico no soportadas: {string.Join(", ", unsupported)}");
        }

        if (TryGetFreshCache(out ExchangeRateFetchResult? cached))
        {
            _logger.LogInformation(
                "Tasas obtenidas de caché fresca. Series={SeriesIds}",
                string.Join(",", requestedSeries));
            return cached with { IsStale = false };
        }

        SemaphoreSlim refreshLock = RefreshLocks.GetOrAdd(
            CacheKey,
            _ => new SemaphoreSlim(1, 1));

        await refreshLock.WaitAsync(cancellationToken);

        try
        {
            if (TryGetFreshCache(out cached))
            {
                _logger.LogInformation(
                    "Tasas obtenidas de la consulta concurrente ya completada.");
                return cached with { IsStale = false };
            }

            if (TryServeFailureCooldown(out ExchangeRateFetchResult? stale))
            {
                return stale;
            }

            string[] catalogSeries = ExchangeRateSeriesCatalog.GetAll()
                .Select(definition => definition.SeriesId)
                .ToArray();

            _logger.LogInformation(
                "Consultando tasas en Banxico. Series={SeriesIds}",
                string.Join(",", catalogSeries));

            ExchangeRateFetchResult fresh = await FetchFromBanxicoAsync(
                catalogSeries,
                cancellationToken);

            _memoryCache.Set(
                CacheKey,
                fresh,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(
                        _options.StaleCacheHours)
                });
            _memoryCache.Remove(FailureCacheKey);

            return fresh;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (BanxicoCooldownException)
        {
            throw;
        }
        catch (Exception exception)
        {
            FailureState failure = RegisterFailure(exception);

            _logger.LogError(
                exception,
                "No fue posible consultar Banxico. Reintento disponible en {RetryAtUtc}.",
                failure.RetryAtUtc);

            if (_memoryCache.TryGetValue(
                CacheKey,
                out ExchangeRateFetchResult? stale)
                && stale is not null)
            {
                _logger.LogWarning(
                    "Se usarán tasas vencidas hasta el siguiente reintento.");
                return stale with { IsStale = true };
            }

            throw new HttpRequestException(
                "No fue posible consultar Banxico y no existe una caché previa.",
                exception);
        }
        finally
        {
            refreshLock.Release();
        }
    }

    private bool TryGetFreshCache(
        [NotNullWhen(true)] out ExchangeRateFetchResult? cached)
    {
        return _memoryCache.TryGetValue(CacheKey, out cached)
            && cached is not null
            && cached.ExpiresAtUtc > DateTime.UtcNow;
    }

    private bool TryServeFailureCooldown(
        [NotNullWhen(true)] out ExchangeRateFetchResult? staleResult)
    {
        staleResult = null;

        if (!_memoryCache.TryGetValue(
                FailureCacheKey,
                out FailureState? failure)
            || failure is null
            || failure.RetryAtUtc <= DateTime.UtcNow)
        {
            return false;
        }

        _logger.LogWarning(
            "Consulta a Banxico en enfriamiento hasta {RetryAtUtc}.",
            failure.RetryAtUtc);

        if (_memoryCache.TryGetValue(
                CacheKey,
                out ExchangeRateFetchResult? stale)
            && stale is not null)
        {
            staleResult = stale with { IsStale = true };
            return true;
        }

        throw new BanxicoCooldownException(
            $"Banxico está temporalmente en enfriamiento hasta " +
            $"{failure.RetryAtUtc:O}. Último error: {failure.Message}");
    }

    private FailureState RegisterFailure(Exception exception)
    {
        int failureCount = 1;
        if (_memoryCache.TryGetValue(
                FailureCacheKey,
                out FailureState? previous)
            && previous is not null)
        {
            failureCount = previous.Count + 1;
        }

        int retryIndex = Math.Min(
            failureCount - 1,
            _options.RetryDelayMinutes.Length - 1);
        DateTime retryAtUtc = DateTime.UtcNow.AddMinutes(
            _options.RetryDelayMinutes[retryIndex]);
        var failure = new FailureState(
            failureCount,
            retryAtUtc,
            exception.Message);

        _memoryCache.Set(
            FailureCacheKey,
            failure,
            TimeSpan.FromHours(_options.StaleCacheHours));

        return failure;
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

    private sealed record FailureState(
        int Count,
        DateTime RetryAtUtc,
        string Message);

    private sealed class BanxicoCooldownException(string message)
        : HttpRequestException(message);
}
