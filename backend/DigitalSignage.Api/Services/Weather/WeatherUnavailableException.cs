namespace DigitalSignage.Api.Services.Weather;

public sealed class WeatherUnavailableException : Exception
{
    public WeatherUnavailableException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
