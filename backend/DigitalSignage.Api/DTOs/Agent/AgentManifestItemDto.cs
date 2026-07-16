namespace DigitalSignage.Api.DTOs.Agent;

public class AgentManifestItemDto
{
    public Guid PlaylistItemId { get; set; }

    public Guid MediaId { get; set; }

    public int Position { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string MediaType { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public string HashSha256 { get; set; } = string.Empty;

    public int? CustomDurationSeconds { get; set; }

    public string DownloadUrl { get; set; } = string.Empty;
}