using DigitalSignage.Api.Authorization;
using DigitalSignage.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Tools;

public static class AdminPasswordResetTool
{
    public static async Task<bool> ResetAsync(
        ApplicationDbContext context,
        string email,
        string password)
    {
        if (string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(password)
            || password.Length is < 8 or > 100)
        {
            return false;
        }

        var normalizedEmail =
            email.Trim().ToLowerInvariant();

        var user = await context.Users
            .FirstOrDefaultAsync(u =>
                u.Email.ToLower() == normalizedEmail
                && u.Role == UserRoles.Administrator);

        if (user is null)
            return false;

        user.PasswordHash =
            BCrypt.Net.BCrypt.HashPassword(
                password);

        user.MustChangePassword = false;
        user.SessionVersion++;
        user.IsActive = true;

        await context.SaveChangesAsync();

        return true;
    }
}
