using DigitalSignage.Api.Enums;

namespace DigitalSignage.Api.Entities;

public class Device
{
    public Guid Id { get; set; }

    public Guid DeviceUuid { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string AccessTokenHash { get; set; } = string.Empty;

    public string Status { get; set; } = "NotSynced";

    public string? IpAddress { get; set; }

    public int CurrentPlaylistVersion { get; set; } = 0;

    public string? AgentVersion { get; set; }

    public DateTime? LastConnectionAt { get; set; }

    public DateTime? LastSyncAt { get; set; }

    public Guid? PendingPowerCommandId { get; set; }

    public PowerCommandType? PendingPowerCommandType { get; set; }

    public DateTime? PendingPowerCommandRequestedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<DeviceExchangeRateSetting> ExchangeRateSettings { get; set; } = new List<DeviceExchangeRateSetting>();

    public DeviceWeatherSetting? WeatherSetting { get; set; }
}
