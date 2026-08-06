using System.Security.Cryptography;
using DigitalSignage.Api.Configuration;
using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.Devices;
using DigitalSignage.Api.Entities;
using DigitalSignage.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Services;

public class DeviceService
{
    private readonly ApplicationDbContext _context;

    public DeviceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DeviceResponseDto>> GetAllAsync(
        string? status = "active")
    {
        var query = _context.Devices
            .AsNoTracking()
            .AsQueryable();

        query = (status ?? "active").Trim().ToLowerInvariant() switch
        {
            "inactive" => query.Where(device => !device.IsActive),
            "all" => query,
            _ => query.Where(device => device.IsActive)
        };

        return await query
            .OrderBy(device => device.Name)
            .Select(device => MapToResponseDto(device))
            .ToListAsync();
    }

    public async Task<DeviceResponseDto?> GetByIdAsync(Guid id)
    {
        var device = await _context.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(device => device.Id == id);

        return device is null
            ? null
            : MapToResponseDto(device);
    }

    public async Task<CreateDeviceResponseDto> CreateAsync(
        CreateDeviceRequestDto request)
    {
        var normalizedName = request.Name.Trim();
        var normalizedLocation = request.Location.Trim();

        var nameExists = await _context.Devices
            .AnyAsync(device => device.Name.ToLower() == normalizedName.ToLower());

        if (nameExists)
        {
            throw new InvalidOperationException(
                "Ya existe un dispositivo con ese nombre.");
        }

        var accessToken = GenerateAccessToken();

        var now = DateTime.UtcNow;
        var device = new Device
        {
            Id = Guid.NewGuid(),
            DeviceUuid = Guid.NewGuid(),
            Name = normalizedName,
            Location = normalizedLocation,
            AccessTokenHash = BCrypt.Net.BCrypt.HashPassword(accessToken),
            Status = "NotSynced",
            CurrentPlaylistVersion = 0,
            IsActive = true,
            CreatedAt = now
        };

        var defaultSeries = ExchangeRateSeriesCatalog.GetDefaults();

        for (var index = 0; index < defaultSeries.Count; index++)
        {
            var definition = defaultSeries[index];

            device.ExchangeRateSettings.Add(
                new DeviceExchangeRateSetting
                {
                    Id = Guid.NewGuid(),
                    DeviceId = device.Id,
                    SeriesId = definition.SeriesId,
                    DisplayName = definition.DefaultDisplayName,
                    Unit = definition.Unit,
                    Position = index + 1,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                });
        }

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        return new CreateDeviceResponseDto
        {
            Id = device.Id,
            DeviceUuid = device.DeviceUuid,
            Name = device.Name,
            Location = device.Location,
            AccessToken = accessToken,
            Status = device.Status,
            IsActive = device.IsActive,
            CreatedAt = device.CreatedAt
        };
    }

    public async Task<DeviceResponseDto?> UpdateAsync(
        Guid id,
        UpdateDeviceRequestDto request)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(device => device.Id == id);

        if (device is null)
        {
            return null;
        }

        var normalizedName = request.Name.Trim();
        var normalizedLocation = request.Location.Trim();

        var nameExists = await _context.Devices.AnyAsync(otherDevice =>
            otherDevice.Id != id &&
            otherDevice.Name.ToLower() == normalizedName.ToLower());

        if (nameExists)
        {
            throw new InvalidOperationException(
                "Ya existe otro dispositivo con ese nombre.");
        }

        device.Name = normalizedName;
        device.Location = normalizedLocation;

        if (request.IsActive.HasValue)
        {
            device.IsActive = request.IsActive.Value;
        }

        await _context.SaveChangesAsync();

        return MapToResponseDto(device);
    }

    public async Task<DeviceResponseDto?> ReactivateAsync(Guid id)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(device => device.Id == id);

        if (device is null)
        {
            return null;
        }

        if (!device.IsActive)
        {
            device.IsActive = true;
            device.Status = "NotSynced";
            device.CurrentPlaylistVersion = 0;
            device.LastSyncAt = null;

            await _context.SaveChangesAsync();
        }

        return MapToResponseDto(device);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(device => device.Id == id);

        if (device is null)
        {
            return false;
        }

        device.IsActive = false;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<SendPowerCommandResponseDto> SendPowerCommandAsync(
        Guid deviceId,
        SendPowerCommandRequestDto request)
    {
        var device = await _context.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == deviceId);

        if (device is null)
        {
            throw new KeyNotFoundException("El dispositivo no existe.");
        }

        if (!device.IsActive)
        {
            throw new InvalidOperationException(
                "No se pueden enviar comandos a un dispositivo inactivo.");
        }

        if (!Enum.IsDefined(request.CommandType))
        {
            throw new ArgumentException(
                "El comando solicitado no es válido.");
        }

        if (device.PendingPowerCommandId.HasValue)
        {
            throw new InvalidOperationException(
                "El dispositivo ya tiene una orden de energía pendiente.");
        }

        var commandId = Guid.NewGuid();
        var requestedAt = DateTime.UtcNow;

        var updatedRows = await _context.Devices
            .Where(item =>
                item.Id == deviceId &&
                item.IsActive &&
                item.PendingPowerCommandId == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    item => item.PendingPowerCommandId,
                    commandId)
                .SetProperty(
                    item => item.PendingPowerCommandType,
                    request.CommandType)
                .SetProperty(
                    item => item.PendingPowerCommandRequestedAt,
                    requestedAt));

        if (updatedRows == 0)
        {
            throw new InvalidOperationException(
                "El dispositivo ya tiene una orden de energía pendiente.");
        }

        return new SendPowerCommandResponseDto
        {
            CommandId = commandId,
            DeviceId = device.Id,
            CommandType = request.CommandType,
            RequestedAt = requestedAt,
            Message = request.CommandType == PowerCommandType.Restart
                ? "La orden de reinicio fue registrada."
                : "La orden de apagado fue registrada."
        };
    }

    private static string GenerateAccessToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToHexString(tokenBytes);
    }

    private static DeviceResponseDto MapToResponseDto(Device device)
    {
        return new DeviceResponseDto
        {
            Id = device.Id,
            DeviceUuid = device.DeviceUuid,
            Name = device.Name,
            Location = device.Location,
            Status = device.Status,
            IpAddress = device.IpAddress,
            CurrentPlaylistVersion = device.CurrentPlaylistVersion,
            AgentVersion = device.AgentVersion,
            LastConnectionAt = device.LastConnectionAt,
            LastSyncAt = device.LastSyncAt,
            IsActive = device.IsActive,
            CreatedAt = device.CreatedAt
        };
    }
}
