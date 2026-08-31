namespace DigitalSignage.Api.Authorization;

public static class UserRoles
{
    public const string Administrator = "Administrator";
    public const string User = "User";

    public static readonly string[] All =
    [
        Administrator,
        User
    ];
}