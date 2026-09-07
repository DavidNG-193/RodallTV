namespace DigitalSignage.Api.Services.Weather.Models;

public sealed record WeatherProviderResult(
    DateTime FetchedAtUtc,
    DateTime ExpiresAtUtc,
    bool IsStale,
    double TemperatureC,
    double ApparentTemperatureC,
    int RelativeHumidityPercent,
    double PrecipitationMm,
    double RainMm,
    double ShowersMm,
    int WeatherCode,
    int CloudCoverPercent,
    bool IsDay,
    double WindSpeedKmh,
    DateTime ObservationTime);
