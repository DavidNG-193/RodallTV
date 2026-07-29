using DigitalSignage.Api.DTOs.Agent;
using DigitalSignage.Api.Entities;
using DigitalSignage.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Api.Controllers;

[ApiController]
[Route("api/agent")]
[AllowAnonymous]
public class AgentController : ControllerBase
{
    private const string DeviceIdHeader = "X-Device-Id";
    private const string DeviceTokenHeader = "X-Device-Token";

    private readonly DeviceAuthenticationService _authenticationService;
    private readonly AgentService _agentService;
    private readonly IWebHostEnvironment _environment;

    public AgentController(
        DeviceAuthenticationService authenticationService,
        AgentService agentService,
        IWebHostEnvironment environment)
    {
        _authenticationService = authenticationService;
        _agentService = agentService;
        _environment = environment;
    }

    [HttpPost("heartbeat")]
    public async Task<ActionResult<AgentHeartbeatResponseDto>> Heartbeat(
        [FromBody] AgentHeartbeatRequestDto request,
        CancellationToken cancellationToken)
    {
        var device = await AuthenticateDeviceAsync(cancellationToken);

        if (device is null)
        {
            return Unauthorized(new
            {
                message = "Credenciales del dispositivo inválidas."
            });
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var response = await _agentService.ProcessHeartbeatAsync(
            device,
            request,
            ipAddress,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("assignment")]
    public async Task<ActionResult<AgentAssignmentResponseDto>> GetAssignment(
        CancellationToken cancellationToken)
    {
        var device = await AuthenticateDeviceAsync(cancellationToken);

        if (device is null)
        {
            return Unauthorized(new
            {
                message = "Credenciales del dispositivo inválidas."
            });
        }

        var response = await _agentService.GetAssignmentAsync(
            device,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("manifest")]
    public async Task<ActionResult<AgentManifestResponseDto>> GetManifest(
        CancellationToken cancellationToken)
    {
        var device = await AuthenticateDeviceAsync(cancellationToken);

        if (device is null)
        {
            return Unauthorized(new
            {
                message = "Credenciales del dispositivo inválidas."
            });
        }

        var apiBaseUrl = $"{Request.Scheme}://{Request.Host}";

        var response = await _agentService.GetManifestAsync(
            device,
            apiBaseUrl,
            cancellationToken);

        if (response is null)
        {
            return StatusCode(StatusCodes.Status409Conflict, new
            {
                message =
                    "La playlist contiene archivos inactivos, inexistentes o no disponibles. " +
                    "No se entregó un manifiesto parcial."
            });
        }

        return Ok(response);
    }

    [HttpGet("media/{mediaId:guid}/download")]
    public async Task<IActionResult> DownloadMedia(
        Guid mediaId,
        CancellationToken cancellationToken)
    {
        var device = await AuthenticateDeviceAsync(cancellationToken);

        if (device is null)
        {
            return Unauthorized(new
            {
                message = "Credenciales del dispositivo inválidas."
            });
        }

        var media = await _agentService.GetAuthorizedMediaAsync(
            device,
            mediaId,
            cancellationToken);

        if (media is null)
        {
            return NotFound(new
            {
                message =
                    "El archivo no existe o no está autorizado para este dispositivo."
            });
        }

        var filePath = ResolveMediaPath(media.FilePath);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new
            {
                message = "El archivo físico no está disponible."
            });
        }

        return PhysicalFile(
            filePath,
            media.MimeType,
            media.OriginalFileName,
            enableRangeProcessing: true);
    }

    [HttpPost("sync-report")]
    public async Task<ActionResult<AgentSyncReportResponseDto>> SyncReport(
        [FromBody] AgentSyncReportRequestDto request,
        CancellationToken cancellationToken)
    {
        var device = await AuthenticateDeviceAsync(cancellationToken);

        if (device is null)
        {
            return Unauthorized(new
            {
                message = "Credenciales del dispositivo inválidas."
            });
        }

        try
        {
            var response = await _agentService.ProcessSyncReportAsync(
                device,
                request,
                cancellationToken);

            return Ok(response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPost("power-command/acknowledge")]
    public async Task<IActionResult> AcknowledgePowerCommand(
        [FromBody] AcknowledgePowerCommandRequestDto request,
        CancellationToken cancellationToken)
    {
        var device = await AuthenticateDeviceAsync(cancellationToken);

        if (device is null)
        {
            return Unauthorized(new
            {
                message = "Credenciales del dispositivo inválidas."
            });
        }

        try
        {
            await _agentService.AcknowledgePowerCommandAsync(
                device,
                request.CommandId,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    private async Task<Device?> AuthenticateDeviceAsync(
        CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue(
                DeviceIdHeader,
                out var deviceIdValue) ||
            !Guid.TryParse(deviceIdValue.ToString(), out var deviceUuid))
        {
            return null;
        }

        if (!Request.Headers.TryGetValue(
                DeviceTokenHeader,
                out var tokenValue))
        {
            return null;
        }

        return await _authenticationService.AuthenticateAsync(
            deviceUuid,
            tokenValue.ToString(),
            cancellationToken);
    }

    private string ResolveMediaPath(string storedPath)
    {
        if (Path.IsPathRooted(storedPath))
        {
            return storedPath;
        }

        return Path.GetFullPath(
            Path.Combine(_environment.ContentRootPath, storedPath));
    }
}
