using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Api.DTOs.ExchangeRates;

public sealed class UpdateDeviceExchangeRateSettingsRequest
{
    [Required]
    public List<UpdateDeviceExchangeRateSettingItem> Items { get; set; } = [];
}

public sealed class UpdateDeviceExchangeRateSettingItem
{
    [Required]
    [MaxLength(30)]
    public string SeriesId { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? DisplayName { get; set; }

    public bool IsActive { get; set; } = true;
}