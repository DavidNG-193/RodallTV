namespace DigitalSignage.Api.DTOs.PlaylistItems;

public class SavePlaylistItemsResponseDto
{
    public int Version { get; set; }
    public List<PlaylistItemResponseDto> Items { get; set; } = [];
}
