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

    public MediaService(
        ApplicationDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
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

    public async Task<bool> DeleteAsync(Guid id)
    {
        var media = await _context.MediaFiles.FindAsync(id);

        if (media is null)
        {
            return false;
        }

        media.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
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