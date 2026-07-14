using Microsoft.AspNetCore.Http;

namespace DigitalSignage.Api.DTOs.Media;

public class UploadMediaRequestDto
{
    public IFormFile File { get; set; } = null!;
    public Guid? MediaFolderId { get; set; }
}