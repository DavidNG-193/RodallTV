using System.Collections.Concurrent;
using System.Diagnostics;
using DigitalSignage.Api.DTOs.Media;
using DigitalSignage.Api.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace DigitalSignage.Api.Services;

public class MediaThumbnailService
{
    private const int ThumbnailWidth = 480;
    private const int ThumbnailHeight = 320;
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> Locks = new();
    private static readonly SemaphoreSlim GenerationThrottle = new(2, 2);

    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MediaThumbnailService> _logger;

    public MediaThumbnailService(
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<MediaThumbnailService> logger)
    {
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<MediaFileResultDto?> GetOrCreateAsync(
        Media media,
        CancellationToken cancellationToken = default)
    {
        string thumbnailDirectory = GetThumbnailDirectory();
        string webpPath = Path.Combine(thumbnailDirectory, $"{media.Id:N}.webp");
        string jpegPath = Path.Combine(thumbnailDirectory, $"{media.Id:N}.jpg");
        string pngPath = Path.Combine(thumbnailDirectory, $"{media.Id:N}.png");

        if (File.Exists(webpPath))
        {
            return ToResult(webpPath, "image/webp", media);
        }

        if (File.Exists(jpegPath))
        {
            return ToResult(jpegPath, "image/jpeg", media);
        }

        if (File.Exists(pngPath))
        {
            return ToResult(pngPath, "image/png", media);
        }

        SemaphoreSlim itemLock = Locks.GetOrAdd(media.Id, _ => new SemaphoreSlim(1, 1));
        await itemLock.WaitAsync(cancellationToken);

        try
        {
            if (File.Exists(webpPath))
            {
                return ToResult(webpPath, "image/webp", media);
            }

            if (File.Exists(jpegPath))
            {
                return ToResult(jpegPath, "image/jpeg", media);
            }

            if (File.Exists(pngPath))
            {
                return ToResult(pngPath, "image/png", media);
            }

            Directory.CreateDirectory(thumbnailDirectory);
            string sourcePath = ResolveMediaPath(media.FilePath);

            if (!File.Exists(sourcePath))
            {
                return null;
            }

            await GenerationThrottle.WaitAsync(cancellationToken);
            bool created;

            try
            {
                created = media.MediaType.Equals("Image", StringComparison.OrdinalIgnoreCase)
                    ? await CreateImageThumbnailAsync(sourcePath, webpPath, cancellationToken)
                    : await CreateVideoThumbnailAsync(sourcePath, jpegPath, cancellationToken);
            }
            finally
            {
                GenerationThrottle.Release();
            }

            if (!created)
            {
                return null;
            }

            if (media.MediaType.Equals("Image", StringComparison.OrdinalIgnoreCase))
            {
                return ToResult(webpPath, "image/webp", media);
            }

            return File.Exists(jpegPath)
                ? ToResult(jpegPath, "image/jpeg", media)
                : ToResult(pngPath, "image/png", media);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(
                ex,
                "No fue posible generar el thumbnail del archivo {MediaId}.",
                media.Id);
            return null;
        }
        finally
        {
            itemLock.Release();
        }
    }

    public void Delete(Guid mediaId)
    {
        string directory = GetThumbnailDirectory();

        foreach (string extension in new[] { ".webp", ".jpg", ".png" })
        {
            string path = Path.Combine(directory, $"{mediaId:N}{extension}");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private async Task<bool> CreateImageThumbnailAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        string temporaryPath = $"{destinationPath}.tmp";

        try
        {
            using Image image = await Image.LoadAsync(sourcePath, cancellationToken);
            image.Mutate(context => context
                .AutoOrient()
                .Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(ThumbnailWidth, ThumbnailHeight)
                }));

            await image.SaveAsWebpAsync(
                temporaryPath,
                new WebpEncoder { Quality = 78 },
                cancellationToken);
            File.Move(temporaryPath, destinationPath, true);
            return true;
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private async Task<bool> CreateVideoThumbnailAsync(
        string sourcePath,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        string ffmpegPath = _configuration["Media:FFmpegPath"] ?? "ffmpeg";
        string temporaryPath = $"{destinationPath}.tmp.jpg";
        var ffmpegStartInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (string argument in new[]
        {
            "-y", "-ss", "00:00:01", "-i", sourcePath,
            "-frames:v", "1",
            "-vf", $"scale={ThumbnailWidth}:{ThumbnailHeight}:force_original_aspect_ratio=decrease",
            "-q:v", "4", temporaryPath
        })
        {
            ffmpegStartInfo.ArgumentList.Add(argument);
        }

        if (await TryRunProcessAsync(ffmpegStartInfo, temporaryPath, cancellationToken))
        {
            File.Move(temporaryPath, destinationPath, true);
            return true;
        }

        string pngDestination = Path.ChangeExtension(destinationPath, ".png");
        string pngTemporary = $"{pngDestination}.tmp.png";
        string bundledMpvPath = Path.GetFullPath(Path.Combine(
            _environment.ContentRootPath,
            "..",
            "..",
            "mpv",
            "mpv.exe"));
        string? configuredMpvPath = _configuration["Media:MpvPath"];
        string mpvPath = !string.IsNullOrWhiteSpace(configuredMpvPath)
            ? configuredMpvPath
            : File.Exists(bundledMpvPath) ? bundledMpvPath : "mpv";

        var mpvStartInfo = new ProcessStartInfo
        {
            FileName = mpvPath,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (string argument in new[]
        {
            sourcePath,
            "--no-config",
            "--no-audio",
            "--start=1",
            "--frames=1",
            $"--vf=scale={ThumbnailWidth}:-2",
            "--ovc=png",
            "--of=image2",
            "--ofopts=update=1",
            $"--o={pngTemporary}"
        })
        {
            mpvStartInfo.ArgumentList.Add(argument);
        }

        if (!await TryRunProcessAsync(mpvStartInfo, pngTemporary, cancellationToken))
        {
            return false;
        }

        File.Move(pngTemporary, pngDestination, true);
        return true;
    }

    private async Task<bool> TryRunProcessAsync(
        ProcessStartInfo startInfo,
        string expectedOutput,
        CancellationToken cancellationToken)
    {
        try
        {
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            Task<string> outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            string output = await outputTask;
            string error = await errorTask;

            if (process.ExitCode == 0 && File.Exists(expectedOutput))
            {
                return true;
            }

            string diagnostic = string.IsNullOrWhiteSpace(error) ? output : error;
            _logger.LogInformation(
                "{Tool} no generó el thumbnail: {Diagnostic}",
                startInfo.FileName,
                diagnostic.Length > 500 ? diagnostic[..500] : diagnostic);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogInformation(
                "No fue posible ejecutar {Tool} para generar un thumbnail: {Message}",
                startInfo.FileName,
                ex.Message);
        }
        if (File.Exists(expectedOutput))
        {
            File.Delete(expectedOutput);
        }

        return false;
    }

    private string GetThumbnailDirectory() => Path.Combine(
        _environment.ContentRootPath,
        "Storage",
        "Media",
        "Thumbnails");

    private string ResolveMediaPath(string storedPath)
    {
        string path = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, storedPath));
        string storageRoot = Path.GetFullPath(Path.Combine(
            _environment.ContentRootPath,
            "Storage",
            "Media"));

        if (!path.StartsWith(storageRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("La ruta del archivo multimedia no es válida.");
        }

        return path;
    }

    private static MediaFileResultDto ToResult(
        string path,
        string contentType,
        Media media) => new()
    {
        PhysicalPath = path,
        ContentType = contentType,
        DownloadFileName = $"{Path.GetFileNameWithoutExtension(media.OriginalFileName)}-thumbnail{Path.GetExtension(path)}"
    };
}
