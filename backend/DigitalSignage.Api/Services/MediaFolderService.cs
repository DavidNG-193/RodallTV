using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.MediaFolders;
using DigitalSignage.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Services;

public class MediaFolderService
{
    private readonly ApplicationDbContext _context;

    public MediaFolderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MediaFolderResponseDto>> GetAllAsync(bool includeInactive = false)
    {
        IQueryable<MediaFolder> query = _context.MediaFolders
            .AsNoTracking()
            .Include(folder => folder.CreatedByUser);

        if (!includeInactive)
        {
            query = query.Where(folder => folder.IsActive);
        }

        return await query
            .OrderBy(folder => folder.Name)
            .Select(folder => ToResponseDto(folder))
            .ToListAsync();
    }

    public async Task<MediaFolderResponseDto?> GetByIdAsync(Guid id)
    {
        var folder = await _context.MediaFolders
            .AsNoTracking()
            .Include(item => item.CreatedByUser)
            .FirstOrDefaultAsync(item => item.Id == id);

        return folder is null ? null : ToResponseDto(folder);
    }

    public async Task<MediaFolderResponseDto> CreateAsync(
        CreateMediaFolderRequestDto request,
        string creatorEmail)
    {
        string normalizedName = request.Name.Trim();

        bool nameExists = await _context.MediaFolders
            .AnyAsync(folder => folder.Name.ToLower() == normalizedName.ToLower());

        if (nameExists)
        {
            throw new InvalidOperationException("Ya existe una carpeta con ese nombre.");
        }

        var creator = await _context.Users
            .FirstOrDefaultAsync(user => user.Email == creatorEmail && user.IsActive);

        if (creator is null)
        {
            throw new UnauthorizedAccessException("No se encontró el usuario autenticado.");
        }

        var folder = new MediaFolder
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = creator.Id,
            CreatedByUser = creator,
            IsActive = true
        };

        _context.MediaFolders.Add(folder);
        await _context.SaveChangesAsync();

        return ToResponseDto(folder);
    }

    public async Task<MediaFolderResponseDto?> UpdateAsync(
        Guid id,
        UpdateMediaFolderRequestDto request)
    {
        var folder = await _context.MediaFolders
            .Include(item => item.CreatedByUser)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (folder is null)
        {
            return null;
        }

        string normalizedName = request.Name.Trim();

        bool nameExists = await _context.MediaFolders.AnyAsync(item =>
            item.Id != id && item.Name.ToLower() == normalizedName.ToLower());

        if (nameExists)
        {
            throw new InvalidOperationException("Ya existe una carpeta con ese nombre.");
        }

        folder.Name = normalizedName;
        folder.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        await _context.SaveChangesAsync();
        return ToResponseDto(folder);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var folder = await _context.MediaFolders.FindAsync(id);

        if (folder is null)
        {
            return false;
        }

        var mediaFiles = await _context.MediaFiles
            .Where(media => media.MediaFolderId == id)
            .ToListAsync();

        foreach (var media in mediaFiles)
        {
            media.MediaFolderId = null;
            media.MediaFolder = null;
        }

        folder.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    private static MediaFolderResponseDto ToResponseDto(MediaFolder folder)
    {
        return new MediaFolderResponseDto
        {
            Id = folder.Id,
            Name = folder.Name,
            Description = folder.Description,
            CreatedAt = folder.CreatedAt,
            CreatedByUserId = folder.CreatedByUserId,
            CreatedByName = $"{folder.CreatedByUser.FirstName} {folder.CreatedByUser.LastName}".Trim(),
            IsActive = folder.IsActive
        };
    }
}
