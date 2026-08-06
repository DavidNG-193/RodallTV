using System.Text.Json.Serialization;

namespace DigitalSignage.Api.Services.ExchangeRates.Models;

public sealed class BanxicoResponse
{
    [JsonPropertyName("bmx")]
    public BanxicoBody? Bmx { get; set; }
}

public sealed class BanxicoBody
{
    [JsonPropertyName("series")]
    public List<BanxicoSeries> Series { get; set; } = [];
}

public sealed class BanxicoSeries
{
    [JsonPropertyName("idSerie")]
    public string IdSerie { get; set; } = string.Empty;

    [JsonPropertyName("titulo")]
    public string Titulo { get; set; } = string.Empty;

    [JsonPropertyName("datos")]
    public List<BanxicoDatum> Datos { get; set; } = [];
}

public sealed class BanxicoDatum
{
    [JsonPropertyName("fecha")]
    public string Fecha { get; set; } = string.Empty;

    [JsonPropertyName("dato")]
    public string Dato { get; set; } = string.Empty;
}