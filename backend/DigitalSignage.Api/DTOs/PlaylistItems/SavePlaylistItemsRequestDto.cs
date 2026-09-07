namespace DigitalSignage.Api.DTOs.PlaylistItems;

public class SavePlaylistItemsRequestDto
{
    public int ExpectedVersion { get; set; }
    public List<SavePlaylistItemDto> Items { get; set; } = [];
}

public class SavePlaylistItemDto
{
    public Guid? Id { get; set; }
    public Guid MediaId { get; set; }
    public int? CustomDurationSeconds { get; set; }
}
