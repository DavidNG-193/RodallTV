using DigitalSignage.Api.DTOs.Devices;
using DigitalSignage.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly DeviceService _deviceService;

    public DevicesController(DeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DeviceResponseDto>>> GetAll(
        [FromQuery] string? status = "active")
    {
        var devices = await _deviceService.GetAllAsync(status);

        return Ok(devices);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeviceResponseDto>> GetById(Guid id)
    {
        var device = await _deviceService.GetByIdAsync(id);

        if (device is null)
        {
            return NotFound(new
            {
                message = "Dispositivo no encontrado."
            });
        }

        return Ok(device);
    }

    [HttpPost]
    public async Task<ActionResult<CreateDeviceResponseDto>> Create(
        CreateDeviceRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Location))
        {
            return BadRequest(new
            {
                message = "El nombre y la ubicación son obligatorios."
            });
        }

        try
        {
            var device = await _deviceService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = device.Id },
                device);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DeviceResponseDto>> Update(
        Guid id,
        UpdateDeviceRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Location))
        {
            return BadRequest(new
            {
                message = "El nombre y la ubicación son obligatorios."
            });
        }

        try
        {
            var device = await _deviceService.UpdateAsync(id, request);

            if (device is null)
            {
                return NotFound(new
                {
                    message = "Dispositivo no encontrado."
                });
            }

            return Ok(device);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new
            {
                message = exception.Message
            });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _deviceService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Dispositivo no encontrado."
            });
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/reactivate")]
    public async Task<ActionResult<DeviceResponseDto>> Reactivate(Guid id)
    {
        var device = await _deviceService.ReactivateAsync(id);

        if (device is null)
        {
            return NotFound(new
            {
                message = "Dispositivo no encontrado."
            });
        }

        return Ok(device);
    }

    [HttpPost("{deviceId:guid}/power-command")]
    public async Task<ActionResult<SendPowerCommandResponseDto>> SendPowerCommand(
        Guid deviceId,
        [FromBody] SendPowerCommandRequestDto request)
    {
        try
        {
            var result = await _deviceService.SendPowerCommandAsync(
                deviceId,
                request);

            return Ok(result);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
