using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Api.DTOs.Auth;

public class ChangePasswordRequestDto
{
    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string ConfirmPassword { get; set; } = string.Empty;
}