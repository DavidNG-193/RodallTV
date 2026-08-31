using Microsoft.AspNetCore.Authorization;

namespace DigitalSignage.Api.Authorization;

public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public HasPermissionAttribute(string permission)
    {
        Permission = permission;
    }

    public string Permission
    {
        get => Policy!.Substring(PolicyPrefix.Length);
        set => Policy = $"{PolicyPrefix}{value}";
    }
}
