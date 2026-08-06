using DigitalSignage.Api.DTOs.Agent;

namespace DigitalSignage.Api.Services.ExchangeRates;

public interface IAgentExchangeRateService
{
    Task<AgentExchangeRatesResponseDto> GetForDeviceAsync(
        Guid deviceId,
        CancellationToken cancellationToken);
}