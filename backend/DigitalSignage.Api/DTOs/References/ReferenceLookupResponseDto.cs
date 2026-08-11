namespace DigitalSignage.Api.DTOs.References;

public sealed record ReferenceLookupResponseDto(
    string ReferenceNumber,
    DateOnly ReferenceDate,
    string Client,
    string OperationCode,
    string Operation,
    string Document,
    int CustomsOfficeNumber,
    string CustomsOffice,
    string StatusCode,
    string Status);