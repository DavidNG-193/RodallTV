namespace DigitalSignage.Api.DTOs.Media;

public class PagedMediaResponseDto
{
    public List<MediaResponseDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
