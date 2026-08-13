using DigitalSignage.Api.DTOs.Agent;

namespace DigitalSignage.Api.Services.References;

public interface IAgentReferenceService
{
    Task<AgentReferencesResponseDto> GetAllAsync(
        CancellationToken cancellationToken);
}