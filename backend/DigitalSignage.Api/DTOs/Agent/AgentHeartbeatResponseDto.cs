namespace DigitalSignage.Api.DTOs.Agent;

public class AgentHeartbeatResponseDto
{

    public Guid DeviceId { get; set; }

    public Guid DeviceUuid { get; set; }

    public string DeviceName { get; set; } = string.Empty;

    public DateTime ServerTimeUtc { get; set; }

    public string Status { get; set; } = string.Empty;

    public int CurrentPlaylistVersion { get; set; }
}