namespace DigitalSignage.Api.Configuration;

public sealed class SagaOptions
{
    public const string SectionName = "Saga";
    public string BaseUrl { get; set; } = string.Empty;
    public string ReferencePath { get; set; } = "SagaWS.NetEnvironmet/rest/sagaWSRef";
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int RequestTimeoutSeconds { get; set; } = 30;
    public int RefreshMinutes { get; set; } = 10;
}