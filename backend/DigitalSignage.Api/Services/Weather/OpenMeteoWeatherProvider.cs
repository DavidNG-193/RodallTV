using System.Globalization;
using System.Net.Http.Json;
using DigitalSignage.Api.Configuration;
using DigitalSignage.Api.Services.Weather.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DigitalSignage.Api.Services.Weather;

public sealed class OpenMeteoWeatherProvider : IWeatherProvider
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly WeatherOptions _options;
    private readonly ILogger<OpenMeteoWeatherProvider> _logger;

    public OpenMeteoWeatherProvider(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<WeatherOptions> options,
        ILogger<OpenMeteoWeatherProvider> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<WeatherProviderResult> GetCurrentAsync(
        double latitude,
        double longitude,
        string timezone,
        CancellationToken cancellationToken)
    {
        string lat = latitude.ToString(
            "0.####",
            CultureInfo.InvariantCulture);

        string lon = longitude.ToString(
            "0.####",
            CultureInfo.InvariantCulture);

        string cacheKey = $"weather:{lat}:{lon}:{timezone}";

        if (_cache.TryGetValue(
            cacheKey,
            out WeatherProviderResult? cached)
            && cached is not null
            && cached.ExpiresAtUtc > DateTime.UtcNow)
        {
            _logger.LogInformation(
                "Clima obtenido de caché fresca. Ubicación={Latitude},{Longitude}",
                lat,
                lon);
            return cached with { IsStale = false };
        }

        try
        {
            _logger.LogInformation(
                "Consultando clima en Open-Meteo. Ubicación={Latitude},{Longitude}",
                lat,
                lon);
            WeatherProviderResult fresh = await FetchAsync(
                lat,
                lon,
                timezone,
                cancellationToken);

            _cache.Set(
                cacheKey,
                fresh,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromHours(24)
                });

            return fresh;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Falló la consulta de clima para {Latitude}, {Longitude}.",
                lat,
                lon);

            if (_cache.TryGetValue(
                cacheKey,
                out WeatherProviderResult? stale)
                && stale is not null)
            {
                _logger.LogWarning(
                    "Se usará el último clima disponible. " +
                    "Ubicación={Latitude},{Longitude}",
                    lat,
                    lon);
                return stale with { IsStale = true };
            }

            throw;
        }
    }

    private async Task<WeatherProviderResult> FetchAsync(
        string latitude,
        string longitude,
        string timezone,
        CancellationToken cancellationToken)
    {
        string encodedTimezone = Uri.EscapeDataString(timezone);

        string url =
            "v1/forecast" +
            $"?latitude={latitude}" +
            $"&longitude={longitude}" +
            "&current=" +
            "temperature_2m," +
            "apparent_temperature," +
            "relative_humidity_2m," +
            "precipitation," +
            "weather_code," +
            "wind_speed_10m" +
            $"&timezone={encodedTimezone}";

        OpenMeteoResponse? response =
            await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(
                url,
                cancellationToken);

        if (response?.Current is null)
            throw new InvalidOperationException(
                "Open-Meteo no devolvió current.");

        if (!DateTime.TryParse(
            response.Current.Time,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime observationTime))
        {
            throw new InvalidOperationException(
                "Open-Meteo devolvió una fecha inválida.");
        }

        DateTime fetchedAt = DateTime.UtcNow;

        return new WeatherProviderResult(
            fetchedAt,
            fetchedAt.AddMinutes(Math.Max(_options.CacheMinutes, 1)),
            false,
            response.Current.Temperature,
            response.Current.ApparentTemperature,
            response.Current.RelativeHumidity,
            response.Current.Precipitation,
            response.Current.WeatherCode,
            response.Current.WindSpeed,
            observationTime);
    }
}
