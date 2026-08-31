using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Api.DTOs.Users;

public class CreateUserRequestDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(100)]
    public string TemporaryPassword { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    public List<string> Permissions { get; set; } = [];
}
