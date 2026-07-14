namespace DigitalSignage.Api.DTOs.MediaFolders;

public class UpdateMediaFolderRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}