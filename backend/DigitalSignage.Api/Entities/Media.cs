namespace DigitalSignage.Api.Entities;

public class Media
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
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public Guid UploadedByUserId { get; set; }
    public Guid? MediaFolderId { get; set; }
    public bool IsActive { get; set; } = true;

    public User UploadedByUser { get; set; } = null!;
    public MediaFolder? MediaFolder { get; set; }
}
