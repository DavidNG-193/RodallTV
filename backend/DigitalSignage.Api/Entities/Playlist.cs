namespace DigitalSignage.Api.Entities;

public class Playlist
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;
    public ICollection<PlaylistItem> Items { get; set; } = new List<PlaylistItem>();
}