using System.Globalization;
using DigitalSignage.Api.Services.References.Models;

namespace DigitalSignage.Api.Services.References;

public static class SagaReferenceRequestFactory
{
    public static SagaReferenceRequest ForReference(string referenceNumber)
    {
        DateTime today = DateTime.Today;

        return new SagaReferenceRequest
        {
            Referencia = referenceNumber.Trim().ToUpperInvariant(),
            TipoFecha = 1,
            FechaInicial = today.AddYears(-1)
                .ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            FechaFinal = today
                .ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
        };
    }
}