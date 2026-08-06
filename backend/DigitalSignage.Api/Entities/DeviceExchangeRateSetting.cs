namespace DigitalSignage.Api.Entities;

public sealed class DeviceExchangeRateSetting
{
    public Guid Id { get; set; }

    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public string SeriesId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Unit { get; set; } = "MXN";

    public int Position { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}