using System.Security.Claims;
using DigitalSignage.Api.Authorization;
using DigitalSignage.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Services;

public class UserSessionValidationService
{
    private readonly ApplicationDbContext _context;

    public UserSessionValidationService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsSessionValidAsync(
        ClaimsPrincipal principal)
    {
        var userIdValue =
            principal.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

        var sessionVersionValue =
            principal.FindFirstValue(
                CustomClaimTypes.SessionVersion
            );

        if (!Guid.TryParse(
                userIdValue,
                out var userId))
        {
            return false;
        }

        if (!int.TryParse(
                sessionVersionValue,
                out var tokenSessionVersion))
        {
            return false;
        }

        var userState = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.IsActive,
                u.SessionVersion
            })
            .FirstOrDefaultAsync();

        if (userState is null)
        {
            return false;
        }

        if (!userState.IsActive)
        {
            return false;
        }

        return userState.SessionVersion
            == tokenSessionVersion;
    }
}