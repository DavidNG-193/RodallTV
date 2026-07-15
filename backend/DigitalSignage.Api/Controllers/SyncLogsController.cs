using DigitalSignage.Api.DTOs.SyncLogs;
using DigitalSignage.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Api.Controllers;

[ApiController]
[Route("api/sync-logs")]
[Authorize]
public class SyncLogsController : ControllerBase
{
    private readonly SyncLogService _service;

    public SyncLogsController(SyncLogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? deviceId,
        [FromQuery] string? result,
        [FromQuery] int limit = 100)
    {
        try
        {
            var logs = await _service.GetAllAsync(deviceId, result, limit);
            return Ok(logs);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var log = await _service.GetByIdAsync(id);

        if (log is null)
            return NotFound(new { message = "Registro de sincronización no encontrado." });

        return Ok(log);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSyncLogRequestDto request)
    {
        try
        {
            var log = await _service.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = log.Id },
                log);
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