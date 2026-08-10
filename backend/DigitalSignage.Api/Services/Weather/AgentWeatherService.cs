using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.Agent;
using DigitalSignage.Api.Services.Weather.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Services.Weather;

public sealed class AgentWeatherService : IAgentWeatherService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IWeatherProvider _provider;

    public AgentWeatherService(
        ApplicationDbContext dbContext,
        IWeatherProvider provider)
    {
        _dbContext = dbContext;
        _provider = provider;
    }

    public async Task<AgentWeatherResponseDto> GetForDeviceAsync(
        Guid deviceId,
        CancellationToken cancellationToken)
    {
        var setting = await _dbContext.DeviceWeatherSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.DeviceId == deviceId && x.IsActive,
                cancellationToken);

        if (setting is null)
        {
            return new AgentWeatherResponseDto(
                false,
                null,
                null,
                null,
                false,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        WeatherProviderResult weather;

        try
        {
            weather = await _provider.GetCurrentAsync(
                setting.Latitude,
                setting.Longitude,
                setting.Timezone,
                cancellationToken);
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new WeatherUnavailableException(
                "La consulta del clima excedió el tiempo de espera.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new WeatherUnavailableException(
                "No fue posible consultar el proveedor meteorológico.",
                exception);
        }
        catch (InvalidOperationException exception)
        {
            throw new WeatherUnavailableException(
                "El proveedor meteorológico devolvió datos inválidos.",
                exception);
        }
        catch (Exception exception)
        {
            throw new WeatherUnavailableException(
                "El proveedor meteorológico no está disponible.",
                exception);
        }

        return new AgentWeatherResponseDto(
            true,
            setting.LocationName,
            weather.FetchedAtUtc,
            weather.ExpiresAtUtc,
            weather.IsStale,
            weather.TemperatureC,
            weather.ApparentTemperatureC,
            weather.RelativeHumidityPercent,
            weather.PrecipitationMm,
            weather.WeatherCode,
            WeatherCodeMapper.ToDescription(weather.WeatherCode),
            weather.WindSpeedKmh,
            weather.ObservationTime);
    }
}
