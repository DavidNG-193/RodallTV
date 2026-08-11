namespace DigitalSignage.Api.Services.References;

public sealed class SagaReferenceResponseException : Exception
{
    public SagaReferenceResponseException(string message)
        : base(message)
    {
    }

    public SagaReferenceResponseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
