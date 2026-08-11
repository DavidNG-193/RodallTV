using System.Text.Json.Serialization;

namespace DigitalSignage.Api.Services.References.Models;

public sealed class SagaReferenceRequest
{
    [JsonPropertyName("Referencia")]
    public string Referencia { get; set; } = string.Empty;

    [JsonPropertyName("ClienteNo")]
    public int ClienteNo { get; set; }

    [JsonPropertyName("Cliente")]
    public string Cliente { get; set; } = string.Empty;

    [JsonPropertyName("ImportadorNo")]
    public int ImportadorNo { get; set; }

    [JsonPropertyName("Importador")]
    public string Importador { get; set; } = string.Empty;

    [JsonPropertyName("TipoContenedor")]
    public int TipoContenedor { get; set; }

    [JsonPropertyName("Pedimento")]
    public int Pedimento { get; set; }

    [JsonPropertyName("Estado")]
    public string Estado { get; set; } = string.Empty;

    [JsonPropertyName("TipoFecha")]
    public int TipoFecha { get; set; } = 1;

    [JsonPropertyName("FechaInicial")]
    public string FechaInicial { get; set; } = string.Empty;

    [JsonPropertyName("FechaFinal")]
    public string FechaFinal { get; set; } = string.Empty;

    [JsonPropertyName("Aduana")]
    public int Aduana { get; set; }

    [JsonPropertyName("Operacion")]
    public string Operacion { get; set; } = string.Empty;

    [JsonPropertyName("Buque")]
    public string Buque { get; set; } = string.Empty;

    [JsonPropertyName("GuiaMaster")]
    public string GuiaMaster { get; set; } = string.Empty;

    [JsonPropertyName("GuiaHouse")]
    public string GuiaHouse { get; set; } = string.Empty;

    [JsonPropertyName("AgenteNo")]
    public int AgenteNo { get; set; }

    [JsonPropertyName("Agente")]
    public string Agente { get; set; } = string.Empty;

    [JsonPropertyName("Ejecutivo")]
    public string Ejecutivo { get; set; } = string.Empty;

    [JsonPropertyName("Transportista")]
    public string Transportista { get; set; } = string.Empty;
}