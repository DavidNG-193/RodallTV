namespace DigitalSignage.Api.DTOs.Weather;

public sealed record WeatherSettingDto(
    Guid Id,
    Guid DeviceId,
    string LocationName,
    double Latitude,
    double Longitude,
    string Timezone,
    bool IsActive);