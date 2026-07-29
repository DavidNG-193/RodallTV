using DigitalSignage.Api.Enums;

namespace DigitalSignage.Api.DTOs.Devices;

public sealed class SendPowerCommandRequestDto
{
    public PowerCommandType CommandType { get; set; }
}
