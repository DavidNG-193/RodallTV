namespace DigitalSignage.Api.DTOs.MediaFolders;

public class CreateMediaFolderRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}