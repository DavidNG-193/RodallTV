using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.Playlists;
using DigitalSignage.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Services;

public class PlaylistService
{
    private readonly ApplicationDbContext _context;

    public PlaylistService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlaylistResponseDto>> GetAllAsync(
        bool includeInactive = false)
    {
        IQueryable<Playlist> query = _context.Playlists
            .AsNoTracking()
            .Include(playlist => playlist.CreatedByUser);

        if (!includeInactive)
        {
            query = query.Where(playlist => playlist.IsActive);
        }

        return await query
            .OrderBy(playlist => playlist.Name)
            .Select(playlist => ToResponseDto(playlist))
            .ToListAsync();
    }

    public async Task<PlaylistResponseDto?> GetByIdAsync(Guid id)
    {
        var playlist = await _context.Playlists
            .AsNoTracking()
            .Include(item => item.CreatedByUser)
            .FirstOrDefaultAsync(item => item.Id == id);

        return playlist is null ? null : ToResponseDto(playlist);
    }

    public async Task<PlaylistResponseDto> CreateAsync(
        CreatePlaylistRequestDto request,
        string creatorEmail)
    {
        string normalizedName = request.Name.Trim();

        bool nameExists = await _context.Playlists.AnyAsync(playlist =>
            playlist.Name.ToLower() == normalizedName.ToLower());

        if (nameExists)
        {
            throw new InvalidOperationException(
                "Ya existe una playlist con ese nombre.");
        }

        var creator = await _context.Users.FirstOrDefaultAsync(user =>
            user.Email == creatorEmail && user.IsActive);

        if (creator is null)
        {
            throw new UnauthorizedAccessException(
                "No se encontró el usuario autenticado.");
        }

        DateTime now = DateTime.UtcNow;

        var playlist = new Playlist
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            Version = 1,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedByUserId = creator.Id,
            CreatedByUser = creator
        };

        _context.Playlists.Add(playlist);
        await _context.SaveChangesAsync();

        return ToResponseDto(playlist);
    }

    public async Task<PlaylistResponseDto?> UpdateAsync(
        Guid id,
        UpdatePlaylistRequestDto request)
    {
        var playlist = await _context.Playlists
            .Include(item => item.CreatedByUser)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (playlist is null)
        {
            return null;
        }

        string normalizedName = request.Name.Trim();

        bool nameExists = await _context.Playlists.AnyAsync(item =>
            item.Id != id &&
            item.Name.ToLower() == normalizedName.ToLower());

        if (nameExists)
        {
            throw new InvalidOperationException(
                "Ya existe una playlist con ese nombre.");
        }

        playlist.Name = normalizedName;
        playlist.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        playlist.Version += 1;
        playlist.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ToResponseDto(playlist);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var playlist = await _context.Playlists.FindAsync(id);

        if (playlist is null)
        {
            return false;
        }

        playlist.IsActive = false;
        playlist.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private static PlaylistResponseDto ToResponseDto(Playlist playlist)
    {
        return new PlaylistResponseDto
        {
            Id = playlist.Id,
            Name = playlist.Name,
            Description = playlist.Description,
            Version = playlist.Version,
            IsActive = playlist.IsActive,
            CreatedAt = playlist.CreatedAt,
            UpdatedAt = playlist.UpdatedAt,
            CreatedByUserId = playlist.CreatedByUserId,
            CreatedByName =
                $"{playlist.CreatedByUser.FirstName} {playlist.CreatedByUser.LastName}".Trim()
        };
    }
}