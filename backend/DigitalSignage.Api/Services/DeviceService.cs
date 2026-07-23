using System.Security.Cryptography;
using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.Devices;
using DigitalSignage.Api.Entities;
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
            CreatedAt = DateTime.UtcNow
        };

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
