namespace DigitalSignage.Api.DTOs.PlaylistAssignments;

public class PlaylistAssignmentResponseDto
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public Guid PlaylistId { get; set; }
    public string PlaylistName { get; set; } = string.Empty;
    public int PlaylistVersion { get; set; }
    public Guid AssignedByUserId { get; set; }
    public string AssignedByEmail { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public DateTime? UnassignedAt { get; set; }
    public bool IsActive { get; set; }
}