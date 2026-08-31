using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Api.DTOs.Users;

public class ResetPasswordRequestDto
{
    [Required, MinLength(8), MaxLength(100)]
    public string TemporaryPassword { get; set; } = string.Empty;
}