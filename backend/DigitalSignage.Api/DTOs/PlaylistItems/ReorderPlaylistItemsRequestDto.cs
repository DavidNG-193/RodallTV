namespace DigitalSignage.Api.DTOs.PlaylistItems;

public class ReorderPlaylistItemsRequestDto
{
    public List<Guid> OrderedItemIds { get; set; } = new();
}