using DigitalSignage.Api.Data;
using DigitalSignage.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Services;

public class DeviceAuthenticationService
{
    private readonly ApplicationDbContext _context;

    public DeviceAuthenticationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Device?> AuthenticateAsync(
        Guid deviceUuid,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (deviceUuid == Guid.Empty || string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        var device = await _context.Devices
            .FirstOrDefaultAsync(device =>
                device.DeviceUuid == deviceUuid,
                cancellationToken);

        if (device is null)
        {
            return null;
        }

        if (!device.IsActive)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(device.AccessTokenHash))
        {
            return null;
        }

        var isTokenValid = BCrypt.Net.BCrypt.Verify(
            accessToken,
            device.AccessTokenHash);

        return isTokenValid ? device : null;
    }
}
