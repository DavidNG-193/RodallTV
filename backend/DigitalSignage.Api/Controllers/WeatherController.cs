using DigitalSignage.Api.DTOs.Weather;
using DigitalSignage.Api.Services.Weather;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Api.Controllers;

[ApiController]
[Route("api/weather")]
[Authorize]
public sealed class WeatherController : ControllerBase
{
    private readonly IDeviceWeatherSettingsService _service;

    public WeatherController(IDeviceWeatherSettingsService service)
    {
        _service = service;
    }

    [HttpGet("devices/{deviceId:guid}")]
    public async Task<ActionResult<WeatherSettingDto?>> Get(
        Guid deviceId,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.GetAsync(deviceId, cancellationToken));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPut("devices/{deviceId:guid}")]
    public async Task<ActionResult<WeatherSettingDto>> Put(
        Guid deviceId,
        [FromBody] UpdateWeatherSettingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.UpsertAsync(
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

    [HttpDelete("devices/{deviceId:guid}")]
    public async Task<IActionResult> Delete(
        Guid deviceId,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(
            deviceId,
            cancellationToken);

        return NoContent();
    }
}
