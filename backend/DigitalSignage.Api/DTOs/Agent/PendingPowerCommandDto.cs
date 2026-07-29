using DigitalSignage.Api.Enums;

namespace DigitalSignage.Api.DTOs.Agent;

public sealed class PendingPowerCommandDto
{
    public Guid CommandId { get; set; }

    public PowerCommandType CommandType { get; set; }

    public DateTime RequestedAt { get; set; }
}
