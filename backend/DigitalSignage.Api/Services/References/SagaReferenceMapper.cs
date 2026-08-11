using System.Globalization;
using DigitalSignage.Api.Services.References.Models;

namespace DigitalSignage.Api.Services.References;

public static class SagaReferenceMapper
{
    public static ExternalReferenceData Map(SagaReferenceItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!DateOnly.TryParseExact(
            item.Fecha,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateOnly referenceDate))
        {
            throw new SagaReferenceResponseException(
                $"Saga devolvió una fecha inválida: {item.Fecha}");
        }

        string referenceNumber = Normalize(item.Referencia);
        string client = Normalize(item.Cliente);
        string operationCode = Normalize(item.Operacion).ToUpperInvariant();
        string document = Normalize(item.Documento);
        string customsOffice = Normalize(item.Aduana);
        string statusCode = Normalize(item.EstadoCve);
        string statusDescription = Normalize(item.Estado);

        EnsureMaxLength(referenceNumber, 50, "Referencia");
        EnsureMaxLength(client, 200, "Cliente");
        EnsureMaxLength(operationCode, 5, "Operacion");
        EnsureMaxLength(document, 30, "Documento");
        EnsureMaxLength(customsOffice, 200, "Aduana");
        EnsureMaxLength(statusCode, 20, "EstadoCve");
        EnsureMaxLength(statusDescription, 200, "Estado");

        string operationDisplayName = operationCode switch
        {
            "I" => "Importación",
            "E" => "Exportación",
            _ => "No especificada"
        };

        return new ExternalReferenceData(
            referenceNumber.ToUpperInvariant(),
            referenceDate,
            client,
            operationCode,
            operationDisplayName,
            document,
            item.AduanaNo,
            customsOffice,
            statusCode,
            statusDescription);
    }

    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;

    private static void EnsureMaxLength(
        string value,
        int maxLength,
        string fieldName)
    {
        if (value.Length > maxLength)
        {
            throw new SagaReferenceResponseException(
                $"Saga devolvió {fieldName} con más de {maxLength} caracteres.");
        }
    }
}
