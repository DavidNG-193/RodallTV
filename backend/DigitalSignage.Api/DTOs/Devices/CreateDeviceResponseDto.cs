namespace DigitalSignage.Api.DTOs.Devices;

public class CreateDeviceResponseDto
{
    public Guid Id { get; set; }

    public Guid DeviceUuid { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}