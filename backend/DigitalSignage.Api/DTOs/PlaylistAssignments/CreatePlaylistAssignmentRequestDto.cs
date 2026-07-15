namespace DigitalSignage.Api.DTOs.PlaylistAssignments;

public class CreatePlaylistAssignmentRequestDto
{
    public Guid DeviceId { get; set; }
    public Guid PlaylistId { get; set; }
}