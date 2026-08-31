using Microsoft.AspNetCore.Authorization;

namespace DigitalSignage.Api.Authorization;

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var hasPermission = context.User.Claims
            .Any(claim =>
                claim.Type == CustomClaimTypes.Permission
                && string.Equals(
                    claim.Value,
                    requirement.Permission,
                    StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}