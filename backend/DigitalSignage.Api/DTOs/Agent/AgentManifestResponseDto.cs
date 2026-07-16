namespace DigitalSignage.Api.DTOs.Agent;

public class AgentManifestResponseDto
{

    public bool HasAssignment { get; set; }

    public Guid? PlaylistId { get; set; }

    public string? PlaylistName { get; set; }

    public int? PlaylistVersion { get; set; }

    public int CurrentDeviceVersion { get; set; }

    public bool RequiresSync { get; set; }

    public List<AgentManifestItemDto> Items { get; set; } = [];
}