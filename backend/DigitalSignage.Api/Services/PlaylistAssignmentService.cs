using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.PlaylistAssignments;
using DigitalSignage.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Services;

public class PlaylistAssignmentService
{
    private readonly ApplicationDbContext _context;

    public PlaylistAssignmentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlaylistAssignmentResponseDto>> GetAllAsync(bool includeInactive = true)
    {
        var query = _context.PlaylistAssignments.AsNoTracking().AsQueryable();

        if (!includeInactive)
            query = query.Where(a => a.IsActive);

        return await query
            .OrderByDescending(a => a.AssignedAt)
            .Select(a => new PlaylistAssignmentResponseDto
            {
                Id = a.Id,
                DeviceId = a.DeviceId,
                DeviceName = a.Device.Name,
                PlaylistId = a.PlaylistId,
                PlaylistName = a.Playlist.Name,
                PlaylistVersion = a.Playlist.Version,
                AssignedByUserId = a.AssignedByUserId,
                AssignedByEmail = a.AssignedByUser.Email,
                AssignedAt = a.AssignedAt,
                UnassignedAt = a.UnassignedAt,
                IsActive = a.IsActive
            })
            .ToListAsync();
    }

    public async Task<PlaylistAssignmentResponseDto?> GetActiveByDeviceIdAsync(Guid deviceId)
    {
        return await _context.PlaylistAssignments
            .AsNoTracking()
            .Where(a => a.DeviceId == deviceId && a.IsActive)
            .Select(a => new PlaylistAssignmentResponseDto
            {
                Id = a.Id,
                DeviceId = a.DeviceId,
                DeviceName = a.Device.Name,
                PlaylistId = a.PlaylistId,
                PlaylistName = a.Playlist.Name,
                PlaylistVersion = a.Playlist.Version,
                AssignedByUserId = a.AssignedByUserId,
                AssignedByEmail = a.AssignedByUser.Email,
                AssignedAt = a.AssignedAt,
                UnassignedAt = a.UnassignedAt,
                IsActive = a.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PlaylistAssignmentResponseDto> AssignAsync(
        Guid deviceId,
        Guid playlistId,
        Guid assignedByUserId)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.Id == deviceId && d.IsActive)
            ?? throw new KeyNotFoundException("El dispositivo no existe o está inactivo.");

        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.IsActive)
            ?? throw new KeyNotFoundException("La playlist no existe o está inactiva.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == assignedByUserId && u.IsActive)
            ?? throw new KeyNotFoundException("El usuario autenticado no existe o está inactivo.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var current = await _context.PlaylistAssignments
            .FirstOrDefaultAsync(a => a.DeviceId == deviceId && a.IsActive);

        if (current is not null)
        {
            if (current.PlaylistId == playlistId)
                throw new InvalidOperationException("La playlist ya está asignada actualmente al dispositivo.");

            current.IsActive = false;
            current.UnassignedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        var assignment = new PlaylistAssignment
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            PlaylistId = playlistId,
            AssignedByUserId = assignedByUserId,
            AssignedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.PlaylistAssignments.Add(assignment);
        device.CurrentPlaylistVersion = 0;
        device.Status = "NotSynced";
        device.LastSyncAt = null;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new PlaylistAssignmentResponseDto
        {
            Id = assignment.Id,
            DeviceId = device.Id,
            DeviceName = device.Name,
            PlaylistId = playlist.Id,
            PlaylistName = playlist.Name,
            PlaylistVersion = playlist.Version,
            AssignedByUserId = user.Id,
            AssignedByEmail = user.Email,
            AssignedAt = assignment.AssignedAt,
            UnassignedAt = assignment.UnassignedAt,
            IsActive = assignment.IsActive
        };
    }

    public async Task<bool> UnassignAsync(Guid deviceId)
    {
        var assignment = await _context.PlaylistAssignments
            .FirstOrDefaultAsync(a => a.DeviceId == deviceId && a.IsActive);

        if (assignment is null)
            return false;

        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == deviceId);
        await using var transaction = await _context.Database.BeginTransactionAsync();

        assignment.IsActive = false;
        assignment.UnassignedAt = DateTime.UtcNow;

        if (device is not null)
        {
            device.CurrentPlaylistVersion = 0;
            device.Status = "NotSynced";
            device.LastSyncAt = null;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        return true;
    }
}