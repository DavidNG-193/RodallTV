namespace DigitalSignage.Api.DTOs.SyncLogs;

public class SyncLogResponseDto
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public Guid? PlaylistId { get; set; }
    public string? PlaylistName { get; set; }
    public int SyncedVersion { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? Message { get; set; }
    public int DownloadedFilesCount { get; set; }
    public int DeletedFilesCount { get; set; }
}