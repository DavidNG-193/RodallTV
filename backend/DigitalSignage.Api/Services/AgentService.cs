using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.Agent;
using DigitalSignage.Api.Entities;
using DigitalSignage.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Services;

public class AgentService
{
    private static readonly HashSet<string> AllowedResults =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Success",
            "Failed",
            "NoChanges"
        };

    private readonly ApplicationDbContext _context;

    public AgentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AgentHeartbeatResponseDto> ProcessHeartbeatAsync(
        Device device,
        AgentHeartbeatRequestDto request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        device.LastConnectionAt = now;

        if (!string.IsNullOrWhiteSpace(ipAddress))
        {
            device.IpAddress = ipAddress;
        }
        if (!string.IsNullOrWhiteSpace(request.AgentVersion))
        {
            device.AgentVersion = request.AgentVersion.Trim();
        }

        // No sobrescribir Error o Syncing indiscriminadamente.
        if (!string.Equals(device.Status, "Error", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(device.Status, "Syncing", StringComparison.OrdinalIgnoreCase))
        {
            device.Status = "Online";
        }
        await _context.SaveChangesAsync(cancellationToken);

        return new AgentHeartbeatResponseDto
        {
            DeviceId = device.Id,
            DeviceUuid = device.DeviceUuid,
            DeviceName = device.Name,
            ServerTimeUtc = now,
            Status = device.Status,
            CurrentPlaylistVersion = device.CurrentPlaylistVersion,
            PendingPowerCommand =
                device.PendingPowerCommandId.HasValue &&
                device.PendingPowerCommandType.HasValue &&
                device.PendingPowerCommandRequestedAt.HasValue
                    ? new PendingPowerCommandDto
                    {
                        CommandId = device.PendingPowerCommandId.Value,
                        CommandType = device.PendingPowerCommandType.Value,
                        RequestedAt =
                            device.PendingPowerCommandRequestedAt.Value
                    }
                    : null
        };
    }

    public async Task AcknowledgePowerCommandAsync(
        Device device,
        Guid commandId,
        CancellationToken cancellationToken = default)
    {
        if (!device.PendingPowerCommandId.HasValue)
        {
            return;
        }

        if (device.PendingPowerCommandId.Value != commandId)
        {
            throw new InvalidOperationException(
                "La orden confirmada no coincide con la orden pendiente.");
        }

        device.PendingPowerCommandId = null;
        device.PendingPowerCommandType = null;
        device.PendingPowerCommandRequestedAt = null;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AgentAssignmentResponseDto> GetAssignmentAsync(
        Device device,
        CancellationToken cancellationToken = default)
    {
        await TouchDeviceAsync(device, cancellationToken);

        var assignment = await _context.PlaylistAssignments
            .AsNoTracking()
            .Include(a => a.Playlist)
            .FirstOrDefaultAsync(
                a => a.DeviceId == device.Id &&
                     a.IsActive &&
                     a.Playlist.IsActive,
                cancellationToken);

        if (assignment is null)
        {
            return new AgentAssignmentResponseDto
            {
                HasAssignment = false,
                CurrentDeviceVersion = device.CurrentPlaylistVersion,
                RequiresSync = device.CurrentPlaylistVersion != 0
            };
        }
        return new AgentAssignmentResponseDto
        {
            HasAssignment = true,
            PlaylistId = assignment.PlaylistId,
            PlaylistName = assignment.Playlist.Name,
            PlaylistVersion = assignment.Playlist.Version,
            CurrentDeviceVersion = device.CurrentPlaylistVersion,
            RequiresSync =
                device.CurrentPlaylistVersion != assignment.Playlist.Version
        };
    }

    public async Task<AgentManifestResponseDto?> GetManifestAsync(
        Device device,
        string apiBaseUrl,
        CancellationToken cancellationToken = default)
    {
        await TouchDeviceAsync(device, cancellationToken);

        var assignment = await _context.PlaylistAssignments
            .AsNoTracking()
            .Include(a => a.Playlist)
            .ThenInclude(p => p.Items)
            .ThenInclude(i => i.Media)
            .FirstOrDefaultAsync(
                a => a.DeviceId == device.Id &&
                     a.IsActive &&
                     a.Playlist.IsActive,
                cancellationToken);

        if (assignment is null)
        {
            return new AgentManifestResponseDto
            {
                HasAssignment = false,
                CurrentDeviceVersion = device.CurrentPlaylistVersion,
                RequiresSync = device.CurrentPlaylistVersion != 0
            };
        }

        var orderedItems = assignment.Playlist.Items
            .OrderBy(i => i.Position)
            .ToList();

        foreach (var item in orderedItems)
        {
            if (!item.Media.IsActive ||
                string.IsNullOrWhiteSpace(item.Media.FilePath) ||
                !File.Exists(item.Media.FilePath))
            {
                return null;
            }
        }

        var items = orderedItems.Select(item => new AgentManifestItemDto
        {
            PlaylistItemId = item.Id,
            MediaId = item.MediaId,
            Position = item.Position,
            OriginalFileName = item.Media.OriginalFileName,
            StoredFileName = item.Media.StoredFileName,
            MediaType = item.Media.MediaType,
            MimeType = item.Media.MimeType,
            FileSizeBytes = item.Media.FileSizeBytes,
            HashSha256 = item.Media.HashSha256,
            CustomDurationSeconds = item.CustomDurationSeconds,
            DownloadUrl = $"{apiBaseUrl.TrimEnd('/')}/api/agent/media/{item.MediaId}/download"
        }).ToList();

        return new AgentManifestResponseDto
        {
            HasAssignment = true,
            PlaylistId = assignment.PlaylistId,
            PlaylistName = assignment.Playlist.Name,
            PlaylistVersion = assignment.Playlist.Version,
            CurrentDeviceVersion = device.CurrentPlaylistVersion,
            RequiresSync =
                device.CurrentPlaylistVersion != assignment.Playlist.Version,
            Items = items
        };
    }

    public async Task<Media?> GetAuthorizedMediaAsync(
        Device device,
        Guid mediaId,
        CancellationToken cancellationToken = default)
    {
        await TouchDeviceAsync(device, cancellationToken);

        return await _context.PlaylistAssignments
            .AsNoTracking()
            .Where(a =>
                a.DeviceId == device.Id &&
                a.IsActive &&
                a.Playlist.IsActive)
            .SelectMany(a => a.Playlist.Items)
            .Where(i =>
                i.MediaId == mediaId &&
                i.Media.IsActive)
            .Select(i => i.Media)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AgentSyncReportResponseDto> ProcessSyncReportAsync(
        Device device,
        AgentSyncReportRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!AllowedResults.Contains(request.Result))
        {
            throw new ArgumentException(
                "Result debe ser Success, Failed o NoChanges.");
        }

        if (request.FinishedAt < request.StartedAt)
        {
            throw new ArgumentException(
                "FinishedAt no puede ser anterior a StartedAt.");
        }

        var normalizedResult = AllowedResults
            .First(r => r.Equals(
                request.Result,
                StringComparison.OrdinalIgnoreCase));

        var now = DateTime.UtcNow;
        device.LastConnectionAt = now;
        switch (normalizedResult)
        {
            case "Success":
                device.CurrentPlaylistVersion = request.SyncedVersion;
                device.LastSyncAt = now;
                device.Status = "Online";
                break;
            case "Failed":
                // Se conserva CurrentPlaylistVersion.
                device.Status = "Error";
                break;
            case "NoChanges":
                if (!string.Equals(
                        device.Status,
                        "Error",
                        StringComparison.OrdinalIgnoreCase))
                {
                    device.Status = "Online";
                }
                break;
        }

        var syncLog = new SyncLog
        {
            Id = Guid.NewGuid(),
            DeviceId = device.Id,
            PlaylistId = request.PlaylistId,
            SyncedVersion = request.SyncedVersion,
            StartedAt = request.StartedAt,
            FinishedAt = request.FinishedAt,
            Result = normalizedResult,
            Message = request.Message,
            DownloadedFilesCount = request.DownloadedFilesCount,
            DeletedFilesCount = request.DeletedFilesCount
        };

        _context.SyncLogs.Add(syncLog);
        await _context.SaveChangesAsync(cancellationToken);

        return new AgentSyncReportResponseDto
        {
            Accepted = true,
            Result = normalizedResult,
            ServerTimeUtc = now,
            CurrentPlaylistVersion = device.CurrentPlaylistVersion,
            DeviceStatus = device.Status
        };
    }

    private async Task TouchDeviceAsync(
        Device device,
        CancellationToken cancellationToken)
    {
        device.LastConnectionAt = DateTime.UtcNow;

        if (!string.Equals(device.Status, "Error", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(device.Status, "Syncing", StringComparison.OrdinalIgnoreCase))
        {
            device.Status = "Online";
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}
