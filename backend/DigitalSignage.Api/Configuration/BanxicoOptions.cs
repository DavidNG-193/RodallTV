namespace DigitalSignage.Api.Configuration;

public sealed class BanxicoOptions
{
    public const string SectionName = "Banxico";

    public string BaseUrl { get; set; } =
        "https://www.banxico.org.mx/SieAPIRest/service/v1/";

    public string ApiToken { get; set; } = string.Empty;
    public int CacheMinutes { get; set; } = 60;
    public int RequestTimeoutSeconds { get; set; } = 15;
    public int HistoryDays { get; set; } = 15;
}
