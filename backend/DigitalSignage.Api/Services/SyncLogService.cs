using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.SyncLogs;
using DigitalSignage.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Services;

public class SyncLogService
{
    private static readonly string[] AllowedResults =
    {
        "Success", "Failed", "NoChanges"
    };

    private readonly ApplicationDbContext _context;

    public SyncLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SyncLogResponseDto>> GetAllAsync(
        Guid? deviceId = null,
        string? result = null,
        int limit = 100)
    {
        limit = Math.Clamp(limit, 1, 500);

        var query = _context.SyncLogs
            .AsNoTracking()
            .AsQueryable();

        if (deviceId.HasValue)
        {
            query = query.Where(log => log.DeviceId == deviceId.Value);
        }

        if (!string.IsNullOrWhiteSpace(result))
        {
            var normalizedResult = NormalizeResult(result);
            query = query.Where(log => log.Result == normalizedResult);
        }

        return await query
            .OrderByDescending(log => log.StartedAt)
            .Take(limit)
            .Select(log => new SyncLogResponseDto
            {
                Id = log.Id,
                DeviceId = log.DeviceId,
                DeviceName = log.Device.Name,
                PlaylistId = log.PlaylistId,
                PlaylistName = log.Playlist != null ? log.Playlist.Name : null,
                SyncedVersion = log.SyncedVersion,
                StartedAt = log.StartedAt,
                FinishedAt = log.FinishedAt,
                Result = log.Result,
                Message = log.Message,
                DownloadedFilesCount = log.DownloadedFilesCount,
                DeletedFilesCount = log.DeletedFilesCount
            })
            .ToListAsync();
    }

    public async Task<SyncLogResponseDto?> GetByIdAsync(Guid id)
    {
        return await _context.SyncLogs
            .AsNoTracking()
            .Where(log => log.Id == id)
            .Select(log => new SyncLogResponseDto
            {
                Id = log.Id,
                DeviceId = log.DeviceId,
                DeviceName = log.Device.Name,
                PlaylistId = log.PlaylistId,
                PlaylistName = log.Playlist != null ? log.Playlist.Name : null,
                SyncedVersion = log.SyncedVersion,
                StartedAt = log.StartedAt,
                FinishedAt = log.FinishedAt,
                Result = log.Result,
                Message = log.Message,
                DownloadedFilesCount = log.DownloadedFilesCount,
                DeletedFilesCount = log.DeletedFilesCount
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SyncLogResponseDto> CreateAsync(
        CreateSyncLogRequestDto request)
    {
        ValidateRequest(request);

        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.Id == request.DeviceId);

        if (device is null)
        {
            throw new KeyNotFoundException("El dispositivo no existe.");
        }

        Playlist? playlist = null;

        if (request.PlaylistId.HasValue)
        {
            playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == request.PlaylistId.Value);

            if (playlist is null)
            {
                throw new KeyNotFoundException("La playlist no existe.");
            }
        }

        var startedAt = request.StartedAt ?? DateTime.UtcNow;
        var result = NormalizeResult(request.Result);

        var log = new SyncLog
        {
            Id = Guid.NewGuid(),
            DeviceId = request.DeviceId,
            PlaylistId = request.PlaylistId,
            SyncedVersion = request.SyncedVersion,
            StartedAt = startedAt,
            FinishedAt = request.FinishedAt,
            Result = result,
            Message = string.IsNullOrWhiteSpace(request.Message)
                ? null
                : request.Message.Trim(),
            DownloadedFilesCount = request.DownloadedFilesCount,
            DeletedFilesCount = request.DeletedFilesCount
        };

        _context.SyncLogs.Add(log);

        if (result == "Success" || result == "NoChanges")
        {
            device.CurrentPlaylistVersion = request.SyncedVersion;
            device.LastSyncAt = request.FinishedAt ?? DateTime.UtcNow;
            device.Status = "Online";
        }
        else
        {
            device.Status = "Error";
        }

        await _context.SaveChangesAsync();

        return new SyncLogResponseDto
        {
            Id = log.Id,
            DeviceId = device.Id,
            DeviceName = device.Name,
            PlaylistId = playlist?.Id,
            PlaylistName = playlist?.Name,
            SyncedVersion = log.SyncedVersion,
            StartedAt = log.StartedAt,
            FinishedAt = log.FinishedAt,
            Result = log.Result,
            Message = log.Message,
            DownloadedFilesCount = log.DownloadedFilesCount,
            DeletedFilesCount = log.DeletedFilesCount
        };
    }

    private static void ValidateRequest(CreateSyncLogRequestDto request)
    {
        if (request.DeviceId == Guid.Empty)
            throw new InvalidOperationException("DeviceId es obligatorio.");

        if (request.SyncedVersion < 0)
            throw new InvalidOperationException("SyncedVersion no puede ser negativo.");

        if (request.DownloadedFilesCount < 0 || request.DeletedFilesCount < 0)
            throw new InvalidOperationException("Los contadores de archivos no pueden ser negativos.");

        if (request.FinishedAt.HasValue &&
            request.StartedAt.HasValue &&
            request.FinishedAt.Value < request.StartedAt.Value)
        {
            throw new InvalidOperationException(
                "FinishedAt no puede ser anterior a StartedAt.");
        }

        NormalizeResult(request.Result);
    }

    private static string NormalizeResult(string result)
    {
        var normalized = AllowedResults.FirstOrDefault(
            allowed => allowed.Equals(result?.Trim(),
                StringComparison.OrdinalIgnoreCase));

        if (normalized is null)
        {
            throw new InvalidOperationException(
                "Result debe ser Success, Failed o NoChanges.");
        }

        return normalized;
    }
}