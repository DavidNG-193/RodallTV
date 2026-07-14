namespace DigitalSignage.Api.DTOs.Playlists;

public class UpdatePlaylistRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}