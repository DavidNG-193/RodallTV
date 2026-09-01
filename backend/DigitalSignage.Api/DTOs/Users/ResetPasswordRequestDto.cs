using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Api.DTOs.Users;

public class ResetPasswordRequestDto
{
    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
