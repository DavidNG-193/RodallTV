using DigitalSignage.Api.Configuration;
using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.ExchangeRates;
using DigitalSignage.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Services.ExchangeRates;

public sealed class DeviceExchangeRateSettingsService
    : IDeviceExchangeRateSettingsService
{
    private readonly ApplicationDbContext _dbContext;

    public DeviceExchangeRateSettingsService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IReadOnlyList<ExchangeRateSeriesOptionDto>> GetCatalogAsync()
    {
        IReadOnlyList<ExchangeRateSeriesOptionDto> result =
            ExchangeRateSeriesCatalog.GetAll()
                .Select(x => new ExchangeRateSeriesOptionDto(
                    x.SeriesId,
                    x.DefaultDisplayName,
                    x.Unit))
                .OrderBy(x => x.DisplayName)
                .ToArray();

        return Task.FromResult(result);
    }

    public async Task<IReadOnlyList<DeviceExchangeRateSettingDto>>
        GetByDeviceAsync(
            Guid deviceId,
            CancellationToken cancellationToken)
    {
        bool deviceExists = await _dbContext.Devices
            .AnyAsync(x => x.Id == deviceId, cancellationToken);

        if (!deviceExists)
        {
            throw new KeyNotFoundException("El dispositivo no existe.");
        }

        return await _dbContext.DeviceExchangeRateSettings
            .AsNoTracking()
            .Where(x => x.DeviceId == deviceId)
            .OrderBy(x => x.Position)
            .Select(x => new DeviceExchangeRateSettingDto(
                x.Id,
                x.DeviceId,
                x.SeriesId,
                x.DisplayName,
                x.Unit,
                x.Position,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DeviceExchangeRateSettingDto>> ReplaceAsync(
        Guid deviceId,
        UpdateDeviceExchangeRateSettingsRequest request,
        CancellationToken cancellationToken)
    {
        bool deviceExists = await _dbContext.Devices
            .AnyAsync(
                x => x.Id == deviceId && x.IsActive,
                cancellationToken);

        if (!deviceExists)
        {
            throw new KeyNotFoundException(
                "El dispositivo no existe o está inactivo.");
        }

        string[] normalizedIds = request.Items
            .Select(x => x.SeriesId.Trim().ToUpperInvariant())
            .ToArray();

        if (normalizedIds.Length != normalizedIds.Distinct().Count())
        {
            throw new InvalidOperationException(
                "No se puede repetir una serie para el mismo dispositivo.");
        }

        var validated = new List<(
            UpdateDeviceExchangeRateSettingItem Item,
            ExchangeRateSeriesDefinition Definition)>();

        foreach (UpdateDeviceExchangeRateSettingItem item in request.Items)
        {
            string seriesId = item.SeriesId.Trim().ToUpperInvariant();

            if (!ExchangeRateSeriesCatalog.TryGet(seriesId, out var definition))
            {
                throw new InvalidOperationException(
                    $"La serie {seriesId} no está permitida.");
            }

            validated.Add((item, definition));
        }

        var current = await _dbContext.DeviceExchangeRateSettings
            .Where(x => x.DeviceId == deviceId)
            .ToListAsync(cancellationToken);

        _dbContext.DeviceExchangeRateSettings.RemoveRange(current);

        DateTime now = DateTime.UtcNow;

        for (int index = 0; index < validated.Count; index++)
        {
            var entry = validated[index];

            string displayName =
                string.IsNullOrWhiteSpace(entry.Item.DisplayName)
                    ? entry.Definition.DefaultDisplayName
                    : entry.Item.DisplayName.Trim();

            _dbContext.DeviceExchangeRateSettings.Add(
                new DeviceExchangeRateSetting
                {
                    Id = Guid.NewGuid(),
                    DeviceId = deviceId,
                    SeriesId = entry.Definition.SeriesId,
                    DisplayName = displayName,
                    Unit = entry.Definition.Unit,
                    Position = index + 1,
                    IsActive = entry.Item.IsActive,
                    CreatedAt = now,
                    UpdatedAt = now
                });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByDeviceAsync(deviceId, cancellationToken);
    }
}