namespace DigitalSignage.Api.Services.References.Models;

public sealed record ExternalReferenceData(
    string ReferenceNumber,
    DateOnly ReferenceDate,
    string Client,
    string OperationCode,
    string OperationDisplayName,
    string Document,
    int CustomsOfficeNumber,
    string CustomsOffice,
    string StatusCode,
    string StatusDescription);