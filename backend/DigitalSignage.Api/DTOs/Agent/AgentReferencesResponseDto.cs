namespace DigitalSignage.Api.DTOs.Agent;

public sealed record AgentReferencesResponseDto(
    DateTime FetchedAtUtc,
    IReadOnlyList<AgentReferenceDto> References);