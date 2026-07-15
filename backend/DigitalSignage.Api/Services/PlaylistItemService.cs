using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.PlaylistItems;
using DigitalSignage.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Services;

public class PlaylistItemService
{
    private readonly ApplicationDbContext _context;

    public PlaylistItemService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlaylistItemResponseDto>?> GetByPlaylistIdAsync(Guid playlistId)
    {
        var playlistExists = await _context.Playlists
            .AnyAsync(p => p.Id == playlistId);

        if (!playlistExists)
        {
            return null;
        }

        return await _context.PlaylistItems
            .AsNoTracking()
            .Where(item => item.PlaylistId == playlistId)
            .OrderBy(item => item.Position)
            .Select(item => new PlaylistItemResponseDto
            {
                Id = item.Id,
                PlaylistId = item.PlaylistId,
                MediaId = item.MediaId,
                OriginalFileName = item.Media.OriginalFileName,
                StoredFileName = item.Media.StoredFileName,
                MediaType = item.Media.MediaType,
                MimeType = item.Media.MimeType,
                Position = item.Position,
                CustomDurationSeconds = item.CustomDurationSeconds,
                CreatedAt = item.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<PlaylistItemResponseDto?> AddAsync(
        Guid playlistId,
        AddPlaylistItemRequestDto request)
    {
        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.IsActive);

        if (playlist is null)
        {
            return null;
        }

        var media = await _context.MediaFiles
            .FirstOrDefaultAsync(m => m.Id == request.MediaId && m.IsActive);

        if (media is null)
        {
            throw new InvalidOperationException("El archivo multimedia no existe o está inactivo.");
        }

        ValidateDuration(request.CustomDurationSeconds);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var lastPosition = await _context.PlaylistItems
            .Where(item => item.PlaylistId == playlistId)
            .MaxAsync(item => (int?)item.Position) ?? 0;

        var item = new PlaylistItem
        {
            Id = Guid.NewGuid(),
            PlaylistId = playlistId,
            MediaId = request.MediaId,
            Position = lastPosition + 1,
            CustomDurationSeconds = request.CustomDurationSeconds,
            CreatedAt = DateTime.UtcNow
        };

        _context.PlaylistItems.Add(item);

        playlist.Version += 1;
        playlist.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Map(item, media);
    }

    public async Task<PlaylistItemResponseDto?> UpdateAsync(
        Guid playlistId,
        Guid itemId,
        UpdatePlaylistItemRequestDto request)
    {
        ValidateDuration(request.CustomDurationSeconds);

        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.IsActive);

        if (playlist is null)
        {
            return null;
        }

        var item = await _context.PlaylistItems
            .Include(i => i.Media)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.PlaylistId == playlistId);

        if (item is null)
        {
            return null;
        }

        item.CustomDurationSeconds = request.CustomDurationSeconds;

        playlist.Version += 1;
        playlist.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Map(item, item.Media);
    }

    public async Task<bool> ReorderAsync(
        Guid playlistId,
        ReorderPlaylistItemsRequestDto request)
    {
        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.IsActive);

        if (playlist is null)
        {
            return false;
        }

        var items = await _context.PlaylistItems
            .Where(i => i.PlaylistId == playlistId)
            .ToListAsync();

        if (items.Count != request.OrderedItemIds.Count)
        {
            throw new InvalidOperationException(
                "La lista enviada debe contener todos los elementos de la playlist.");
        }

        if (request.OrderedItemIds.Distinct().Count() != request.OrderedItemIds.Count)
        {
            throw new InvalidOperationException(
                "La lista de elementos contiene identificadores repetidos.");
        }

        var currentIds = items.Select(i => i.Id).OrderBy(id => id).ToList();
        var requestedIds = request.OrderedItemIds.OrderBy(id => id).ToList();

        if (!currentIds.SequenceEqual(requestedIds))
        {
            throw new InvalidOperationException(
                "Uno o más elementos no pertenecen a la playlist.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        // Posiciones temporales negativas para evitar colisiones con el índice único.
        for (var index = 0; index < request.OrderedItemIds.Count; index++)
        {
            var item = items.First(i => i.Id == request.OrderedItemIds[index]);
            item.Position = -(index + 1);
        }

        await _context.SaveChangesAsync();

        for (var index = 0; index < request.OrderedItemIds.Count; index++)
        {
            var item = items.First(i => i.Id == request.OrderedItemIds[index]);
            item.Position = index + 1;
        }

        playlist.Version += 1;
        playlist.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(Guid playlistId, Guid itemId)
    {
        var playlist = await _context.Playlists
            .FirstOrDefaultAsync(p => p.Id == playlistId && p.IsActive);

        if (playlist is null)
        {
            return false;
        }

        var item = await _context.PlaylistItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.PlaylistId == playlistId);

        if (item is null)
        {
            return false;
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        _context.PlaylistItems.Remove(item);
        await _context.SaveChangesAsync();

        var remainingItems = await _context.PlaylistItems
            .Where(i => i.PlaylistId == playlistId)
            .OrderBy(i => i.Position)
            .ToListAsync();

        for (var index = 0; index < remainingItems.Count; index++)
        {
            remainingItems[index].Position = index + 1;
        }

        playlist.Version += 1;
        playlist.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }

    private static void ValidateDuration(int? durationSeconds)
    {
        if (durationSeconds.HasValue && durationSeconds.Value <= 0)
        {
            throw new InvalidOperationException(
                "La duración personalizada debe ser mayor que cero.");
        }
    }

    private static PlaylistItemResponseDto Map(PlaylistItem item, Media media)
    {
        return new PlaylistItemResponseDto
        {
            Id = item.Id,
            PlaylistId = item.PlaylistId,
            MediaId = item.MediaId,
            OriginalFileName = media.OriginalFileName,
            StoredFileName = media.StoredFileName,
            MediaType = media.MediaType,
            MimeType = media.MimeType,
            Position = item.Position,
            CustomDurationSeconds = item.CustomDurationSeconds,
            CreatedAt = item.CreatedAt
        };
    }
}