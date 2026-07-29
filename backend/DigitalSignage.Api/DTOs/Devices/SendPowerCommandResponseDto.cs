using DigitalSignage.Api.Enums;

namespace DigitalSignage.Api.DTOs.Devices;

public sealed class SendPowerCommandResponseDto
{
    public Guid CommandId { get; set; }

    public Guid DeviceId { get; set; }

    public PowerCommandType CommandType { get; set; }

    public DateTime RequestedAt { get; set; }

    public string Message { get; set; } = string.Empty;
}
