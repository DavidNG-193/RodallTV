using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Api.DTOs.Weather;

public sealed class UpdateWeatherSettingRequest
{
    [Required]
    [MaxLength(120)]
    public string LocationName { get; set; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    [Required]
    [MaxLength(80)]
    public string Timezone { get; set; } = "America/Mexico_City";

    public bool IsActive { get; set; } = true;
}