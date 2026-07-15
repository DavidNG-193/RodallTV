using System.Security.Claims;
using DigitalSignage.Api.DTOs.PlaylistAssignments;
using DigitalSignage.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Api.Controllers;

[ApiController]
[Route("api/playlist-assignments")]
[Authorize]
public class PlaylistAssignmentsController : ControllerBase
{
    private readonly PlaylistAssignmentService _service;

    public PlaylistAssignmentsController(PlaylistAssignmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = true)
    {
        return Ok(await _service.GetAllAsync(includeInactive));
    }

    [HttpGet("device/{deviceId:guid}/active")]
    public async Task<IActionResult> GetActiveByDevice(Guid deviceId)
    {
        var assignment = await _service.GetActiveByDeviceIdAsync(deviceId);
        return assignment is null
            ? NotFound(new { message = "El dispositivo no tiene una playlist activa asignada." })
            : Ok(assignment);
    }

    [HttpPost]
    public async Task<IActionResult> Assign(CreatePlaylistAssignmentRequestDto request)
    {
        if (request.DeviceId == Guid.Empty || request.PlaylistId == Guid.Empty)
            return BadRequest(new { message = "DeviceId y PlaylistId son obligatorios." });

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized(new { message = "El token no contiene un identificador de usuario válido." });

        try
        {
            var assignment = await _service.AssignAsync(request.DeviceId, request.PlaylistId, userId);
            return CreatedAtAction(nameof(GetActiveByDevice), new { deviceId = request.DeviceId }, assignment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("device/{deviceId:guid}/active")]
    public async Task<IActionResult> Unassign(Guid deviceId)
    {
        var result = await _service.UnassignAsync(deviceId);
        return result
            ? NoContent()
            : NotFound(new { message = "El dispositivo no tiene una asignación activa." });
    }
}