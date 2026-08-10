namespace DigitalSignage.Api.Entities;

public sealed class DeviceWeatherSetting
{
    public Guid Id { get; set; }

    public Guid DeviceId { get; set; }
    public Device Device { get; set; } = null!;

    public string LocationName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Timezone { get; set; } = "America/Mexico_City";
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}