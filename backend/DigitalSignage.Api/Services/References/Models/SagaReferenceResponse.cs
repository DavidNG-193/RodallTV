using System.Text.Json.Serialization;

namespace DigitalSignage.Api.Services.References.Models;

public sealed class SagaReferenceResponse
{
    [JsonPropertyName("SDTRef")]
    public List<SagaReferenceItem> References { get; set; } = [];
}

public sealed class SagaReferenceItem
{
    [JsonPropertyName("Referencia")]
    public string Referencia { get; set; } = string.Empty;

    [JsonPropertyName("Fecha")]
    public string Fecha { get; set; } = string.Empty;

    [JsonPropertyName("Operacion")]
    public string Operacion { get; set; } = string.Empty;

    [JsonPropertyName("Documento")]
    public string Documento { get; set; } = string.Empty;

    [JsonPropertyName("AduanaNo")]
    public int AduanaNo { get; set; }

    [JsonPropertyName("Aduana")]
    public string Aduana { get; set; } = string.Empty;

    [JsonPropertyName("ClienteNo")]
    public int ClienteNo { get; set; }

    [JsonPropertyName("Cliente")]
    public string Cliente { get; set; } = string.Empty;

    [JsonPropertyName("EstadoCve")]
    public string EstadoCve { get; set; } = string.Empty;

    [JsonPropertyName("Estado")]
    public string Estado { get; set; } = string.Empty;
}