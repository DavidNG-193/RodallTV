using DigitalSignage.Api.DTOs.References;

namespace DigitalSignage.Api.Services.References;

public interface IDailyReferenceService
{
    Task<ReferenceLookupResponseDto?> LookupAsync(
        string referenceNumber,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DailyReferenceDto>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<DailyReferenceDto> CreateAsync(
        string referenceNumber,
        Guid createdByUserId,
        CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task DeleteAllAsync(CancellationToken cancellationToken);
    Task<int> RefreshAllAsync(CancellationToken cancellationToken);
}