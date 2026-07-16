namespace DigitalSignage.Api.DTOs.Agent;

public class AgentSyncReportResponseDto
{
    public bool Accepted { get; set; }

    public string Result { get; set; } = string.Empty;

    public DateTime ServerTimeUtc { get; set; }

    public int CurrentPlaylistVersion { get; set; }

    public string DeviceStatus { get; set; } = string.Empty;
}