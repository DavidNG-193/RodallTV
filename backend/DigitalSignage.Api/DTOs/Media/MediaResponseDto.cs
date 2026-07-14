namespace DigitalSignage.Api.DTOs.Media;

public class MediaResponseDto
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int? DurationSeconds { get; set; }
    public string HashSha256 { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public Guid UploadedByUserId { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public Guid? MediaFolderId { get; set; }
    public string? MediaFolderName { get; set; }
    public bool IsActive { get; set; }
}