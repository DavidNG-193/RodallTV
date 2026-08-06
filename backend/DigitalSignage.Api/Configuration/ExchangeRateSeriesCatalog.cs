namespace DigitalSignage.Api.Configuration;

public sealed record ExchangeRateSeriesDefinition(
    string SeriesId,
    string DefaultDisplayName,
    string Unit);

public static class ExchangeRateSeriesCatalog
{
    private static readonly string[] DefaultOrder =
    [
        "SF43718",
        "SF46410",
        "SF46406",
        "SF46407",
        "SF60632"
    ];

    private static readonly IReadOnlyDictionary<string, ExchangeRateSeriesDefinition>
        Definitions =
            new Dictionary<string, ExchangeRateSeriesDefinition>(
                StringComparer.OrdinalIgnoreCase)
            {
                ["SF43718"] = new("SF43718", "USD / MXN", "MXN"),
                ["SF46410"] = new("SF46410", "EUR / MXN", "MXN"),
                ["SF46406"] = new("SF46406", "JPY / MXN", "MXN"),
                ["SF46407"] = new("SF46407", "GBP / MXN", "MXN"),
                ["SF60632"] = new("SF60632", "CAD / MXN", "MXN")
            };

    public static IReadOnlyCollection<ExchangeRateSeriesDefinition> GetAll() =>
        DefaultOrder.Select(seriesId => Definitions[seriesId]).ToArray();

    public static IReadOnlyList<ExchangeRateSeriesDefinition> GetDefaults() =>
        DefaultOrder.Select(seriesId => Definitions[seriesId]).ToArray();

    public static bool TryGet(
        string seriesId,
        out ExchangeRateSeriesDefinition definition) =>
        Definitions.TryGetValue(seriesId, out definition!);
}
