using System.Security.Cryptography;
using DigitalSignage.Api.Configuration;
using Microsoft.Extensions.Options;

namespace DigitalSignage.Api.Services;

public sealed class MediaStorageService
{
    private static readonly HashSet<string> ThumbnailExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".webp",
            ".jpg",
            ".png"
        };

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<MediaStorageService> _logger;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _initialized;

    public MediaStorageService(
        IWebHostEnvironment environment,
        IOptions<MediaStorageOptions> options,
        ILogger<MediaStorageService> logger)
    {
        _environment = environment;
        _logger = logger;

        string configuredRoot = Environment.ExpandEnvironmentVariables(
            options.Value.RootPath.Trim());
        if (!Path.IsPathRooted(configuredRoot))
        {
            throw new InvalidOperationException(
                "MediaStorage:RootPath debe ser una ruta absoluta.");
        }

        RootPath = Path.GetFullPath(configuredRoot);
        OriginalsPath = Path.Combine(RootPath, "originals");
        ThumbnailsPath = Path.Combine(RootPath, "thumbnails");

        EnsureStorageIsOutsideApplication();
    }

    public string RootPath { get; }
    public string OriginalsPath { get; }
    public string ThumbnailsPath { get; }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
            {
                return;
            }

            Directory.CreateDirectory(OriginalsPath);
            Directory.CreateDirectory(ThumbnailsPath);
            VerifyWriteAccess();

            string legacyOriginals = Path.Combine(
                _environment.ContentRootPath,
                "Storage",
                "Media");
            string legacyThumbnails = Path.Combine(
                legacyOriginals,
                "Thumbnails");

            int migratedOriginals = await MigrateDirectoryAsync(
                legacyOriginals,
                OriginalsPath,
                cancellationToken);
            int migratedThumbnails = await MigrateDirectoryAsync(
                legacyThumbnails,
                ThumbnailsPath,
                cancellationToken);

            _initialized = true;
            _logger.LogInformation(
                "Almacenamiento multimedia listo. root={RootPath} " +
                "migratedOriginals={OriginalCount} " +
                "migratedThumbnails={ThumbnailCount}",
                RootPath,
                migratedOriginals,
                migratedThumbnails);
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public string GetOriginalPath(string storedFileName)
    {
        ValidateFileName(storedFileName);
        return Path.Combine(OriginalsPath, storedFileName);
    }

    public string GetThumbnailPath(Guid mediaId, string extension)
    {
        if (!ThumbnailExtensions.Contains(extension))
        {
            throw new ArgumentException(
                "La extensión del thumbnail no está permitida.",
                nameof(extension));
        }

        return Path.Combine(ThumbnailsPath, $"{mediaId:N}{extension}");
    }

    public bool OriginalExists(string storedFileName) =>
        File.Exists(GetOriginalPath(storedFileName));

    private async Task<int> MigrateDirectoryAsync(
        string sourceDirectory,
        string destinationDirectory,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            return 0;
        }

        int migrated = 0;
        foreach (string sourcePath in Directory.EnumerateFiles(
            sourceDirectory,
            "*",
            SearchOption.TopDirectoryOnly))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string fileName = Path.GetFileName(sourcePath);
            if (fileName.Equals(".gitkeep", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            ValidateFileName(fileName);
            string destinationPath = Path.Combine(destinationDirectory, fileName);
            await MoveVerifiedAsync(
                sourcePath,
                destinationPath,
                cancellationToken);
            migrated++;
        }

        return migrated;
    }

    private static async Task MoveVerifiedAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        if (File.Exists(destinationPath))
        {
            if (!await FilesMatchAsync(
                    sourcePath,
                    destinationPath,
                    cancellationToken))
            {
                throw new IOException(
                    $"Ya existe un archivo distinto en {destinationPath}.");
            }

            File.Delete(sourcePath);
            return;
        }

        string temporaryPath = $"{destinationPath}.migrating";
        try
        {
            await using (var source = new FileStream(
                sourcePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                useAsync: true))
            await using (var destination = new FileStream(
                temporaryPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true))
            {
                await source.CopyToAsync(destination, cancellationToken);
                await destination.FlushAsync(cancellationToken);
            }

            if (!await FilesMatchAsync(
                    sourcePath,
                    temporaryPath,
                    cancellationToken))
            {
                throw new IOException(
                    $"La copia de {Path.GetFileName(sourcePath)} no superó " +
                    "la validación SHA-256.");
            }

            File.Move(temporaryPath, destinationPath);
            File.Delete(sourcePath);
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }

    private static async Task<bool> FilesMatchAsync(
        string firstPath,
        string secondPath,
        CancellationToken cancellationToken)
    {
        var firstInfo = new FileInfo(firstPath);
        var secondInfo = new FileInfo(secondPath);
        if (firstInfo.Length != secondInfo.Length)
        {
            return false;
        }

        byte[] firstHash = await CalculateSha256Async(
            firstPath,
            cancellationToken);
        byte[] secondHash = await CalculateSha256Async(
            secondPath,
            cancellationToken);
        return CryptographicOperations.FixedTimeEquals(firstHash, secondHash);
    }

    private static async Task<byte[]> CalculateSha256Async(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            81920,
            useAsync: true);
        return await SHA256.HashDataAsync(stream, cancellationToken);
    }

    private void VerifyWriteAccess()
    {
        string probePath = Path.Combine(
            RootPath,
            $".write-test-{Guid.NewGuid():N}");
        try
        {
            File.WriteAllText(probePath, string.Empty);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"La API no tiene permisos de escritura en {RootPath}.",
                exception);
        }
        finally
        {
            File.Delete(probePath);
        }
    }

    private void EnsureStorageIsOutsideApplication()
    {
        string contentRoot = Path.GetFullPath(_environment.ContentRootPath);
        string relative = Path.GetRelativePath(contentRoot, RootPath);
        if (Path.IsPathRooted(relative))
        {
            return;
        }

        if (relative == "." ||
            (!relative.Equals("..", StringComparison.Ordinal) &&
             !relative.StartsWith(
                 $"..{Path.DirectorySeparatorChar}",
                 StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                "MediaStorage:RootPath debe estar fuera de la carpeta de la API.");
        }
    }

    private static void ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) ||
            !string.Equals(
                Path.GetFileName(fileName),
                fileName,
                StringComparison.Ordinal) ||
            fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new InvalidOperationException(
                "El nombre físico del archivo multimedia no es válido.");
        }
    }
}
