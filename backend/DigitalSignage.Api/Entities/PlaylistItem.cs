namespace DigitalSignage.Api.Entities;

public class PlaylistItem
{
    public Guid Id { get; set; }

    public Guid PlaylistId { get; set; }

    public Guid MediaId { get; set; }

    public int Position { get; set; }

    public int? CustomDurationSeconds { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Playlist Playlist { get; set; } = null!;

    public Media Media { get; set; } = null!;
}