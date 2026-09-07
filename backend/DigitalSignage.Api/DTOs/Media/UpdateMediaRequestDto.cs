namespace DigitalSignage.Api.DTOs.Media;

public class UpdateMediaRequestDto
{
    public string OriginalFileName { get; set; } = string.Empty;
    public Guid? MediaFolderId { get; set; }
}
