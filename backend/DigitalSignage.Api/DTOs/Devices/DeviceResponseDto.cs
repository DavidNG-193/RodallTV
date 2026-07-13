namespace DigitalSignage.Api.DTOs.Devices;

public class DeviceResponseDto
{
    public Guid Id { get; set; }

    public Guid DeviceUuid { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? IpAddress { get; set; }

    public int CurrentPlaylistVersion { get; set; }

    public string? AgentVersion { get; set; }

    public DateTime? LastConnectionAt { get; set; }

    public DateTime? LastSyncAt { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}