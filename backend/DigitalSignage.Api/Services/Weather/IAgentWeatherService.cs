using DigitalSignage.Api.DTOs.Agent;

namespace DigitalSignage.Api.Services.Weather;

public interface IAgentWeatherService
{
    Task<AgentWeatherResponseDto> GetForDeviceAsync(
        Guid deviceId,
        CancellationToken cancellationToken);
}