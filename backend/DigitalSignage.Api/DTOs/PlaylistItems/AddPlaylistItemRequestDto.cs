namespace DigitalSignage.Api.DTOs.PlaylistItems;

public class AddPlaylistItemRequestDto
{
    public Guid MediaId { get; set; }

    public int? CustomDurationSeconds { get; set; }
}