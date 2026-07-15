namespace DigitalSignage.Api.Entities;

public class SyncLog
{
    public Guid Id { get; set; }

    public Guid DeviceId { get; set; }

    public Guid? PlaylistId { get; set; }

    public int SyncedVersion { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? FinishedAt { get; set; }

    public string Result { get; set; } = "NoChanges";

    public string? Message { get; set; }

    public int DownloadedFilesCount { get; set; }

    public int DeletedFilesCount { get; set; }

    public Device Device { get; set; } = null!;

    public Playlist? Playlist { get; set; }
}