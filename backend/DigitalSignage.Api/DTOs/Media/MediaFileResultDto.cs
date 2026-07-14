namespace DigitalSignage.Api.DTOs.Media;

public class MediaFileResultDto
{
    public string PhysicalPath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string DownloadFileName { get; set; } = string.Empty;
}