namespace DigitalSignage.Api.Entities;

public class User
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Administrator";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public bool MustChangePassword { get; set; } = false;

    public int SessionVersion { get; set; } = 1;

    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}