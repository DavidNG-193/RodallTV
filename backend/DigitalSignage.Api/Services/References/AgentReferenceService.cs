using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.Agent;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Services.References;

public sealed class AgentReferenceService : IAgentReferenceService
{
    private readonly ApplicationDbContext _dbContext;

    public AgentReferenceService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AgentReferencesResponseDto> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var references = await _dbContext.DailyReferences
            .AsNoTracking()
            .OrderBy(x => x.CreatedAt)
            .Select(x => new AgentReferenceDto(
                x.Id,
                x.ReferenceNumber,
                x.ReferenceDate,
                x.Client,
                x.OperationCode,
                x.OperationDisplayName,
                x.Document,
                x.CustomsOfficeNumber,
                x.CustomsOffice,
                x.StatusCode,
                x.StatusDescription,
                x.LastExternalUpdateAt))
            .ToListAsync(cancellationToken);

        return new AgentReferencesResponseDto(
            DateTime.UtcNow,
            references);
    }
}