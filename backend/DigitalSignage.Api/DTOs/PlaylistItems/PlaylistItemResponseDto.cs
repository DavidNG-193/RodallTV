namespace DigitalSignage.Api.DTOs.PlaylistItems;

public class PlaylistItemResponseDto
{
    public Guid Id { get; set; }

    public Guid PlaylistId { get; set; }

    public Guid MediaId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string MediaType { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public int Position { get; set; }

    public int? CustomDurationSeconds { get; set; }

    public DateTime CreatedAt { get; set; }
}