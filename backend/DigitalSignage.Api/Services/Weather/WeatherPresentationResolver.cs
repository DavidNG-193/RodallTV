using DigitalSignage.Api.Services.Weather.Models;

namespace DigitalSignage.Api.Services.Weather;

public sealed record WeatherPresentation(
    int DisplayWeatherCode,
    string Description);

public static class WeatherPresentationResolver
{
    private const double MinimalPrecipitationMm = 0.2;
    private const int LowCloudCoverPercent = 75;

    public static WeatherPresentation Resolve(WeatherProviderResult weather)
    {
        bool isDrizzle = weather.WeatherCode is 51 or 53 or 55;
        bool isThunderstorm = weather.WeatherCode is 95 or 96 or 99;

        if (!isDrizzle && !isThunderstorm)
        {
            return new WeatherPresentation(
                weather.WeatherCode,
                WeatherCodeMapper.ToDescription(weather.WeatherCode));
        }

        bool hasWeakVisualEvidence =
            weather.PrecipitationMm <= MinimalPrecipitationMm
            && weather.RainMm <= MinimalPrecipitationMm
            && weather.ShowersMm <= MinimalPrecipitationMm
            && weather.CloudCoverPercent < LowCloudCoverPercent;

        if (!hasWeakVisualEvidence)
        {
            return new WeatherPresentation(
                weather.WeatherCode,
                WeatherCodeMapper.ToDescription(weather.WeatherCode));
        }

        int displayCode = weather.CloudCoverPercent switch
        {
            <= 20 => weather.IsDay ? 0 : 1,
            <= 70 => 2,
            _ => 3
        };

        string description = weather.WeatherCode switch
        {
            51 or 53 or 55 => "Posible llovizna",
            96 or 99 => "Posible tormenta con granizo",
            _ => "Posible tormenta"
        };

        return new WeatherPresentation(displayCode, description);
    }
}
