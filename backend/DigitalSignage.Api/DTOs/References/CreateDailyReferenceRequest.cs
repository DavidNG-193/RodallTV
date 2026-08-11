using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Api.DTOs.References;

public sealed class CreateDailyReferenceRequest
{
    [Required]
    [MaxLength(50)]
    public string ReferenceNumber { get; set; } = string.Empty;
}