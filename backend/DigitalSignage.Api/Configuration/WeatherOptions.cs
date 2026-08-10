namespace DigitalSignage.Api.Configuration;

public sealed class WeatherOptions
{
    public const string SectionName = "Weather";

    public string BaseUrl { get; set; } =
        "https://api.open-meteo.com/";

    public int CacheMinutes { get; set; } = 15;
    public int RequestTimeoutSeconds { get; set; } = 15;

    public WeatherDefaultSettingOptions DefaultSetting { get; set; } = new();
}

public sealed class WeatherDefaultSettingOptions
{
    public string LocationName { get; set; } = "Veracruz, Veracruz";
    public double Latitude { get; set; } = 19.1738;
    public double Longitude { get; set; } = -96.1342;
    public string Timezone { get; set; } = "America/Mexico_City";
    public bool IsActive { get; set; } = true;
}
