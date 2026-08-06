namespace DigitalSignage.Api.Services.ExchangeRates;

public sealed class ExchangeRateUnavailableException : Exception
{
    public ExchangeRateUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
