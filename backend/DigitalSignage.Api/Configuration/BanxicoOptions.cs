namespace DigitalSignage.Api.Configuration;

public sealed class BanxicoOptions
{
    public const string SectionName = "Banxico";

    public string BaseUrl { get; set; } =
        "https://www.banxico.org.mx/SieAPIRest/service/v1/";

    public string ApiToken { get; set; } = string.Empty;
    public int CacheMinutes { get; set; } = 10;
    public int RequestTimeoutSeconds { get; set; } = 15;
    public int HistoryDays { get; set; } = 15;
    public int StaleCacheHours { get; set; } = 24;
    public int[] RetryDelayMinutes { get; set; } = [2, 5, 15];
}
