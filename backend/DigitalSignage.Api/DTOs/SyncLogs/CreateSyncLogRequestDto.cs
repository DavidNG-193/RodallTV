namespace DigitalSignage.Api.DTOs.SyncLogs;

public class CreateSyncLogRequestDto
{
    public Guid DeviceId { get; set; }

    public Guid? PlaylistId { get; set; }

    public int SyncedVersion { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public string Result { get; set; } = "NoChanges";

    public string? Message { get; set; }

    public int DownloadedFilesCount { get; set; }

    public int DeletedFilesCount { get; set; }
}