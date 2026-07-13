namespace DigitalSignage.Api.DTOs.Devices;

public class UpdateDeviceRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}