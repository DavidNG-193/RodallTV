namespace DigitalSignage.Api.DTOs.Playlists;

public class CreatePlaylistRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}