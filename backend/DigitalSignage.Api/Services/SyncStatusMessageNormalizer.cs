namespace DigitalSignage.Api.Services;

public static class SyncStatusMessageNormalizer
{
    private const int MaximumMessageLength = 500;

    private static readonly string[] ConnectTimeoutMarkers =
    [
        "ConnectTimeoutError",
        "ConnectTimeout",
        "connection timed out",
        "timed out while connecting"
    ];

    private static readonly string[] ReadTimeoutMarkers =
    [
        "ReadTimeoutError",
        "ReadTimeout",
        "read timed out"
    ];

    private static readonly string[] ConnectionMarkers =
    [
        "Max retries exceeded",
        "Failed to establish a new connection",
        "NameResolutionError",
        "Temporary failure in name resolution",
        "Network is unreachable",
        "Connection refused"
    ];

    public static string? Normalize(string result, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        string normalized = message.ReplaceLineEndings(" ").Trim();

        if (result.Equals("Failed", StringComparison.OrdinalIgnoreCase))
        {
            if (ContainsAny(normalized, ConnectTimeoutMarkers))
            {
                return "No fue posible conectar con el backend dentro del tiempo esperado.";
            }

            if (ContainsAny(normalized, ReadTimeoutMarkers))
            {
                return "El backend no respondió dentro del tiempo esperado.";
            }

            if (ContainsAny(normalized, ConnectionMarkers))
            {
                return "No fue posible establecer conexión con el backend.";
            }
        }

        return normalized.Length <= MaximumMessageLength
            ? normalized
            : normalized[..MaximumMessageLength];
    }

    private static bool ContainsAny(string message, IEnumerable<string> markers) =>
        markers.Any(marker => message.Contains(
            marker,
            StringComparison.OrdinalIgnoreCase));
}
