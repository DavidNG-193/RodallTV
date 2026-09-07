namespace DigitalSignage.Api.Services;

public sealed class PlaylistVersionConflictException : Exception
{
    public PlaylistVersionConflictException(string message)
        : base(message)
    {
    }
}
