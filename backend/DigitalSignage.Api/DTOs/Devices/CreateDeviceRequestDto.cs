namespace DigitalSignage.Api.DTOs.Devices;

public class CreateDeviceRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;
}