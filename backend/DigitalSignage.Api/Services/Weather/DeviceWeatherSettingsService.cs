using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.Weather;
using DigitalSignage.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Services.Weather;

public sealed class DeviceWeatherSettingsService
    : IDeviceWeatherSettingsService
{
    private readonly ApplicationDbContext _dbContext;

    public DeviceWeatherSettingsService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WeatherSettingDto?> GetAsync(
        Guid deviceId,
        CancellationToken cancellationToken)
    {
        bool deviceExists = await _dbContext.Devices
            .AnyAsync(x => x.Id == deviceId, cancellationToken);

        if (!deviceExists)
            throw new KeyNotFoundException("El dispositivo no existe.");

        return await _dbContext.DeviceWeatherSettings
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId)
            .Select(x => new WeatherSettingDto(
                x.Id,
                x.DeviceId,
                x.LocationName,
                x.Latitude,
                x.Longitude,
                x.Timezone,
                x.IsActive))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<WeatherSettingDto> UpsertAsync(
        Guid deviceId,
        UpdateWeatherSettingRequest request,
        CancellationToken cancellationToken)
    {
        bool deviceExists = await _dbContext.Devices
            .AnyAsync(
                x => x.Id == deviceId && x.IsActive,
                cancellationToken);

        if (!deviceExists)
            throw new KeyNotFoundException(
                "El dispositivo no existe o está inactivo.");

        string locationName = request.LocationName.Trim();
        string timezone = request.Timezone.Trim();

        if (string.IsNullOrWhiteSpace(locationName))
            throw new InvalidOperationException(
                "LocationName es obligatorio.");

        if (string.IsNullOrWhiteSpace(timezone))
            throw new InvalidOperationException(
                "Timezone es obligatorio.");

        DeviceWeatherSetting? setting =
            await _dbContext.DeviceWeatherSettings
                .SingleOrDefaultAsync(
                    x => x.DeviceId == deviceId,
                    cancellationToken);

        DateTime now = DateTime.UtcNow;

        if (setting is null)
        {
            setting = new DeviceWeatherSetting
            {
                Id = Guid.NewGuid(),
                DeviceId = deviceId,
                CreatedAt = now
            };

            _dbContext.DeviceWeatherSettings.Add(setting);
        }

        setting.LocationName = locationName;
        setting.Latitude = request.Latitude;
        setting.Longitude = request.Longitude;
        setting.Timezone = timezone;
        setting.IsActive = request.IsActive;
        setting.UpdatedAt = now;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new WeatherSettingDto(
            setting.Id,
            setting.DeviceId,
            setting.LocationName,
            setting.Latitude,
            setting.Longitude,
            setting.Timezone,
            setting.IsActive);
    }

    public async Task DeleteAsync(
        Guid deviceId,
        CancellationToken cancellationToken)
    {
        DeviceWeatherSetting? setting =
            await _dbContext.DeviceWeatherSettings
                .SingleOrDefaultAsync(
                    x => x.DeviceId == deviceId,
                    cancellationToken);

        if (setting is null)
            return;

        _dbContext.DeviceWeatherSettings.Remove(setting);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}