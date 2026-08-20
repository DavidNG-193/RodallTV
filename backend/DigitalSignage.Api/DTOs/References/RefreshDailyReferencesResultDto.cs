namespace DigitalSignage.Api.DTOs.References;

public sealed record RefreshDailyReferencesResultDto(
    int TotalCount,
    int RefreshedCount,
    int NotFoundCount,
    int FailedCount,
    DateTime RefreshedAtUtc);
