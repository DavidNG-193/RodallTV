namespace DigitalSignage.Api.DTOs.Agent;

public sealed record AgentReferenceDto(
    Guid Id,
    string ReferenceNumber,
    DateOnly ReferenceDate,
    string Client,
    string OperationCode,
    string Operation,
    string Document,
    int CustomsOfficeNumber,
    string CustomsOffice,
    string StatusCode,
    string Status,
    DateTime LastExternalUpdateAt);