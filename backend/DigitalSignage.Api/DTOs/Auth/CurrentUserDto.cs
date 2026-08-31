namespace DigitalSignage.Api.DTOs.Auth;

public class CurrentUserDto
{
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool MustChangePassword { get; set; }

    public List<string> Permissions { get; set; } = [];
}