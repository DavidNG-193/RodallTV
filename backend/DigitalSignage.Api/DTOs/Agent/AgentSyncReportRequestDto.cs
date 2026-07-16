using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Api.DTOs.Agent;

public class AgentSyncReportRequestDto
{
    [Required]

    public string Result { get; set; } = string.Empty;

    public Guid? PlaylistId { get; set; }

    [Range(0, int.MaxValue)]

    public int SyncedVersion { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime FinishedAt { get; set; }

    public string? Message { get; set; }

    [Range(0, int.MaxValue)]

    public int DownloadedFilesCount { get; set; }

    [Range(0, int.MaxValue)]

    public int DeletedFilesCount { get; set; }
}