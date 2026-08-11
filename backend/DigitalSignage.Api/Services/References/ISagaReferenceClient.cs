using DigitalSignage.Api.Services.References.Models;

namespace DigitalSignage.Api.Services.References;

public interface ISagaReferenceClient
{
    Task<ExternalReferenceData?> FindByReferenceAsync(
        string referenceNumber,
        CancellationToken cancellationToken);
}