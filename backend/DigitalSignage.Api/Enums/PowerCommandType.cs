using System.Text.Json.Serialization;

namespace DigitalSignage.Api.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<PowerCommandType>))]
public enum PowerCommandType
{
    Restart = 1,
    Shutdown = 2
}
