namespace DigitalSignage.Api.Entities;

public class PlaylistAssignment
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public Guid PlaylistId { get; set; }
    public Guid AssignedByUserId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnassignedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public Device Device { get; set; } = null!;
    public Playlist Playlist { get; set; } = null!;
    public User AssignedByUser { get; set; } = null!;
}