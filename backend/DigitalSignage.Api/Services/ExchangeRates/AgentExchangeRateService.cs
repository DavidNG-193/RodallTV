using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.Agent;
using DigitalSignage.Api.Services.ExchangeRates.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Services.ExchangeRates;

public sealed class AgentExchangeRateService : IAgentExchangeRateService
{
    private const string Source = "Banco de México SIE";

    private readonly ApplicationDbContext _dbContext;
    private readonly IExchangeRateProvider _provider;

    public AgentExchangeRateService(
        ApplicationDbContext dbContext,
        IExchangeRateProvider provider)
    {
        _dbContext = dbContext;
        _provider = provider;
    }

    public async Task<AgentExchangeRatesResponseDto> GetForDeviceAsync(
        Guid deviceId,
        CancellationToken cancellationToken)
    {
        var settings = await _dbContext.DeviceExchangeRateSettings
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId && x.IsActive)
            .OrderBy(x => x.Position)
            .ToListAsync(cancellationToken);

        if (settings.Count == 0)
        {
            return new AgentExchangeRatesResponseDto(
                false,
                null,
                null,
                Source,
                false,
                Array.Empty<AgentExchangeRateDto>());
        }

        ExchangeRateFetchResult result;

        try
        {
            result = await _provider.GetLatestAsync(
                settings.Select(x => x.SeriesId).ToArray(),
                cancellationToken);
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new ExchangeRateUnavailableException(
                "La consulta de tasas excedió el tiempo de espera.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new ExchangeRateUnavailableException(
                "No fue posible consultar el proveedor de tasas.",
                exception);
        }
        catch (InvalidOperationException exception)
        {
            throw new ExchangeRateUnavailableException(
                "El proveedor devolvió datos inválidos.",
                exception);
        }

        var rates = settings
            .Select(setting =>
            {
                var value = result.Values[setting.SeriesId];

                return new AgentExchangeRateDto(
                    setting.SeriesId,
                    setting.DisplayName,
                    value.Value,
                    setting.Unit,
                    value.EffectiveDate,
                    value.ChangePercent,
                    setting.Position);
            })
            .ToArray();

        return new AgentExchangeRatesResponseDto(
            true,
            result.FetchedAtUtc,
            result.ExpiresAtUtc,
            Source,
            result.IsStale,
            rates);
    }
}
