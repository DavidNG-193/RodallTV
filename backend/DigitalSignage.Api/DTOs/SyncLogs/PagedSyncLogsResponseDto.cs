namespace DigitalSignage.Api.DTOs.SyncLogs;

public class PagedSyncLogsResponseDto
{
    public List<SyncLogResponseDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
