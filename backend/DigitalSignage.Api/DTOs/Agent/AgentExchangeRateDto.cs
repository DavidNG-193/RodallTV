namespace DigitalSignage.Api.DTOs.Agent;

public sealed record AgentExchangeRateDto(
    string SeriesId,
    string DisplayName,
    decimal Value,
    string Unit,
    DateOnly EffectiveDate,
    decimal? ChangePercent,
    int Position);
