namespace DigitalSignage.Api.Services.Weather.Models;

public sealed record WeatherProviderResult(
    DateTime FetchedAtUtc,
    DateTime ExpiresAtUtc,
    bool IsStale,
    double TemperatureC,
    double ApparentTemperatureC,
    int RelativeHumidityPercent,
    double PrecipitationMm,
    int WeatherCode,
    double WindSpeedKmh,
    DateTime ObservationTime);