namespace DigitalSignage.Api.DTOs.Agent;

public sealed record AgentExchangeRatesResponseDto(
    bool Enabled,
    DateTime? FetchedAtUtc,
    DateTime? ExpiresAtUtc,
    string Source,
    bool IsStale,
    IReadOnlyList<AgentExchangeRateDto> Rates);