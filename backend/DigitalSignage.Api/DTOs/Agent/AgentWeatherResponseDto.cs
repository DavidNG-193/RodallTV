namespace DigitalSignage.Api.DTOs.Agent;

public sealed record AgentWeatherResponseDto(
    bool Enabled,
    string? LocationName,
    DateTime? FetchedAtUtc,
    DateTime? ExpiresAtUtc,
    bool IsStale,
    double? TemperatureC,
    double? ApparentTemperatureC,
    int? RelativeHumidityPercent,
    double? PrecipitationMm,
    int? WeatherCode,
    string? Description,
    double? WindSpeedKmh,
    DateTime? ObservationTime);