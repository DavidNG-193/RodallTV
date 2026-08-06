using DigitalSignage.Api.DTOs.ExchangeRates;
using DigitalSignage.Api.Services.ExchangeRates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Api.Controllers;

[ApiController]
[Route("api/exchange-rates")]
[Authorize]
public sealed class ExchangeRatesController : ControllerBase
{
    private readonly IDeviceExchangeRateSettingsService _settingsService;

    public ExchangeRatesController(
        IDeviceExchangeRateSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet("catalog")]
    public async Task<ActionResult<
        IReadOnlyList<ExchangeRateSeriesOptionDto>>> GetCatalog()
    {
        return Ok(await _settingsService.GetCatalogAsync());
    }

    [HttpGet("devices/{deviceId:guid}")]
    public async Task<ActionResult<
        IReadOnlyList<DeviceExchangeRateSettingDto>>> GetByDevice(
            Guid deviceId,
            CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _settingsService.GetByDeviceAsync(
                deviceId,
                cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPut("devices/{deviceId:guid}")]
    public async Task<ActionResult<
        IReadOnlyList<DeviceExchangeRateSettingDto>>> Replace(
            Guid deviceId,
            [FromBody] UpdateDeviceExchangeRateSettingsRequest request,
            CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _settingsService.ReplaceAsync(
                deviceId,
                request,
                cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
