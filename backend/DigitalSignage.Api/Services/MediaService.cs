using System.Security.Cryptography;
using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.Media;
using DigitalSignage.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Services;

public class MediaService
{
    private const long MaxFileSizeBytes = 500L * 1024L * 1024L;

    private static readonly Dictionary<string, (string MediaType, string[] MimeTypes)>
        AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = ("Image", ["image/jpeg"]),
            [".jpeg"] = ("Image", ["image/jpeg"]),
            [".png"] = ("Image", ["image/png"]),
            [".webp"] = ("Image", ["image/webp"]),
            [".mp4"] = ("Video", ["video/mp4"]),
            [".webm"] = ("Video", ["video/webm"])
        };

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly MediaThumbnailService _thumbnailService;

    public MediaService(
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        MediaThumbnailService thumbnailService)
    {
        _context = context;
        _environment = environment;
        _thumbnailService = thumbnailService;
    }

    public async Task<PagedMediaResponseDto> GetPagedAsync(
        int page = 1,
        int pageSize = 24,
        string? search = null,
        string? mediaType = null,
        Guid? mediaFolderId = null,
        bool rootOnly = false,
        string sort = "recent")
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<Media> query = _context.MediaFiles
            .AsNoTracking()
            .Include(item => item.UploadedByUser)
            .Include(item => item.MediaFolder)
            .Where(item => item.IsActive);

        if (rootOnly)
        {
            query = query.Where(item => item.MediaFolderId == null);
        }
        else if (mediaFolderId.HasValue)
        {
            query = query.Where(item => item.MediaFolderId == mediaFolderId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            string normalizedSearch = search.Trim().ToLower();
            query = query.Where(item =>
                item.OriginalFileName.ToLower().Contains(normalizedSearch));
        }

        if (!string.IsNullOrWhiteSpace(mediaType) &&
            !mediaType.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            string normalizedType = mediaType.Equals("image", StringComparison.OrdinalIgnoreCase)
                ? "Image"
                : mediaType.Equals("video", StringComparison.OrdinalIgnoreCase)
                    ? "Video"
                    : string.Empty;

            if (!string.IsNullOrEmpty(normalizedType))
            {
                query = query.Where(item => item.MediaType == normalizedType);
            }
        }

        IOrderedQueryable<Media> orderedQuery = sort.Trim().ToLowerInvariant() switch
        {
            "nameasc" => query
                .OrderBy(item => item.OriginalFileName.ToLower())
                .ThenBy(item => item.Id),
            "namedesc" => query
                .OrderByDescending(item => item.OriginalFileName.ToLower())
                .ThenByDescending(item => item.Id),
            _ => query
                .OrderByDescending(item => item.UploadedAt)
                .ThenByDescending(item => item.Id)
        };

        int totalItems = await query.CountAsync();
        var items = await orderedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedMediaResponseDto
        {
            Items = items.Select(ToResponseDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0
                ? 0
                : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<List<MediaResponseDto>> GetAllAsync(
        bool includeInactive = false,
        Guid? mediaFolderId = null)
    {
        IQueryable<Media> query = _context.MediaFiles
            .AsNoTracking()
            .Include(item => item.UploadedByUser)
            .Include(item => item.MediaFolder);

        if (!includeInactive)
        {
            query = query.Where(item => item.IsActive);
        }

        if (mediaFolderId.HasValue)
        {
            query = query.Where(item => item.MediaFolderId == mediaFolderId.Value);
        }

        var items = await query
            .OrderByDescending(item => item.UploadedAt)
            .ToListAsync();

        return items.Select(ToResponseDto).ToList();
    }

    public async Task<MediaResponseDto?> GetByIdAsync(Guid id)
    {
        var media = await _context.MediaFiles
            .AsNoTracking()
            .Include(item => item.UploadedByUser)
            .Include(item => item.MediaFolder)
            .FirstOrDefaultAsync(item => item.Id == id);

        return media is null ? null : ToResponseDto(media);
    }

    public async Task<MediaResponseDto> UploadAsync(
        UploadMediaRequestDto request,
        string uploaderEmail)
    {
        ValidateFile(request.File);

        var uploader = await _context.Users
            .FirstOrDefaultAsync(user =>
                user.Email == uploaderEmail && user.IsActive);

        if (uploader is null)
        {
            throw new UnauthorizedAccessException(
                "No se encontró el usuario autenticado.");
        }

        MediaFolder? folder = null;

        if (request.MediaFolderId.HasValue)
        {
            folder = await _context.MediaFolders
                .FirstOrDefaultAsync(item =>
                    item.Id == request.MediaFolderId.Value && item.IsActive);

            if (folder is null)
            {
                throw new ArgumentException(
                    "La carpeta multimedia no existe o está inactiva.");
            }
        }

        string originalFileName = Path.GetFileName(request.File.FileName);
        string extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        string storedFileName = $"{Guid.NewGuid():N}{extension}";
        string storageDirectory = Path.Combine(
            _environment.ContentRootPath,
            "Storage",
            "Media");

        Directory.CreateDirectory(storageDirectory);

        string physicalPath = Path.Combine(storageDirectory, storedFileName);
        string relativePath = Path.Combine("Storage", "Media", storedFileName)
            .Replace('\\', '/');

        try
        {
            await using (var outputStream = new FileStream(
                physicalPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true))
            {
                await request.File.CopyToAsync(outputStream);
            }

            string hashSha256 = await CalculateSha256Async(physicalPath);
            string mediaType = AllowedExtensions[extension].MediaType;

            var media = new Media
            {
                Id = Guid.NewGuid(),
                OriginalFileName = originalFileName,
                StoredFileName = storedFileName,
                FileExtension = extension,
                MimeType = request.File.ContentType,
                MediaType = mediaType,
                FileSizeBytes = request.File.Length,
                DurationSeconds = null,
                FilePath = relativePath,
                HashSha256 = hashSha256,
                UploadedAt = DateTime.UtcNow,
                UploadedByUserId = uploader.Id,
                UploadedByUser = uploader,
                MediaFolderId = folder?.Id,
                MediaFolder = folder,
                IsActive = true
            };

            _context.MediaFiles.Add(media);
            await _context.SaveChangesAsync();

            await _thumbnailService.GetOrCreateAsync(media);

            return ToResponseDto(media);
        }
        catch
        {
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            throw;
        }
    }

    public async Task<MediaFileResultDto?> GetFileAsync(Guid id)
    {
        var media = await _context.MediaFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id && item.IsActive);

        if (media is null)
        {
            return null;
        }

        string physicalPath = Path.GetFullPath(Path.Combine(
            _environment.ContentRootPath,
            media.FilePath));

        string storageRoot = Path.GetFullPath(Path.Combine(
            _environment.ContentRootPath,
            "Storage",
            "Media"));

        if (!physicalPath.StartsWith(
            storageRoot,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "La ruta del archivo multimedia no es válida.");
        }

        if (!File.Exists(physicalPath))
        {
            throw new FileNotFoundException(
                "El archivo físico no existe en el servidor.");
        }

        return new MediaFileResultDto
        {
            PhysicalPath = physicalPath,
            ContentType = media.MimeType,
            DownloadFileName = media.OriginalFileName
        };
    }

    public async Task<MediaFileResultDto?> GetThumbnailAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var media = await _context.MediaFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == id && item.IsActive,
                cancellationToken);

        return media is null
            ? null
            : await _thumbnailService.GetOrCreateAsync(media, cancellationToken);
    }

    public async Task<MediaResponseDto?> UpdateAsync(
        Guid id,
        UpdateMediaRequestDto request)
    {
        var media = await _context.MediaFiles
            .Include(item => item.UploadedByUser)
            .Include(item => item.MediaFolder)
            .FirstOrDefaultAsync(item => item.Id == id && item.IsActive);

        if (media is null)
        {
            return null;
        }

        string normalizedName = NormalizeFileName(
            request.OriginalFileName,
            media.FileExtension);

        MediaFolder? folder = null;
        if (request.MediaFolderId.HasValue)
        {
            folder = await _context.MediaFolders.FirstOrDefaultAsync(item =>
                item.Id == request.MediaFolderId.Value && item.IsActive);

            if (folder is null)
            {
                throw new ArgumentException(
                    "La carpeta multimedia no existe o está inactiva.");
            }
        }

        bool nameChanged = !string.Equals(
            media.OriginalFileName,
            normalizedName,
            StringComparison.Ordinal);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        media.OriginalFileName = normalizedName;
        media.MediaFolderId = folder?.Id;
        media.MediaFolder = folder;

        if (nameChanged)
        {
            var affectedPlaylists = await _context.Playlists
                .Where(playlist => playlist.Items.Any(item => item.MediaId == id))
                .ToListAsync();

            foreach (var playlist in affectedPlaylists)
            {
                playlist.Version += 1;
                playlist.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return ToResponseDto(media);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var media = await _context.MediaFiles.FindAsync(id);

        if (media is null)
        {
            return false;
        }

        media.IsActive = false;
        await _context.SaveChangesAsync();
        _thumbnailService.Delete(id);
        return true;
    }

    private static string NormalizeFileName(string requestedName, string extension)
    {
        string name = requestedName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("El nombre del archivo es obligatorio.");
        }

        if (name.Length > 255 ||
            name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            name.Any(char.IsControl) ||
            name.EndsWith('.') ||
            Path.GetFileName(name) != name)
        {
            throw new ArgumentException("El nombre del archivo no es válido.");
        }

        string requestedExtension = Path.GetExtension(name);
        if (string.IsNullOrEmpty(requestedExtension))
        {
            name += extension;
        }
        else if (!requestedExtension.Equals(extension, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("No es posible cambiar la extensión del archivo.");
        }

        if (name.Length > 255)
        {
            throw new ArgumentException("El nombre del archivo no puede exceder 255 caracteres.");
        }

        return name;
    }

    private static void ValidateFile(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("Debe seleccionar un archivo.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new ArgumentException(
                "El archivo supera el tamaño máximo de 500 MB.");
        }

        string extension = Path.GetExtension(file.FileName);

        if (string.IsNullOrWhiteSpace(extension) ||
            !AllowedExtensions.TryGetValue(extension, out var allowed))
        {
            throw new ArgumentException(
                "La extensión del archivo no está permitida.");
        }

        if (!allowed.MimeTypes.Contains(
            file.ContentType,
            StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "El tipo MIME no coincide con un formato permitido.");
        }
    }

    private static async Task<string> CalculateSha256Async(string path)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            useAsync: true);

        byte[] hash = await SHA256.HashDataAsync(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static MediaResponseDto ToResponseDto(Media media)
    {
        return new MediaResponseDto
        {
            Id = media.Id,
            OriginalFileName = media.OriginalFileName,
            StoredFileName = media.StoredFileName,
            FileExtension = media.FileExtension,
            MimeType = media.MimeType,
            MediaType = media.MediaType,
            FileSizeBytes = media.FileSizeBytes,
            DurationSeconds = media.DurationSeconds,
            HashSha256 = media.HashSha256,
            UploadedAt = media.UploadedAt,
            UploadedByUserId = media.UploadedByUserId,
            UploadedByName =
                $"{media.UploadedByUser.FirstName} {media.UploadedByUser.LastName}"
                    .Trim(),
            MediaFolderId = media.MediaFolderId,
            MediaFolderName = media.MediaFolder?.Name,
            IsActive = media.IsActive
        };
    }
}
