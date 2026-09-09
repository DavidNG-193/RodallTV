using DigitalSignage.Api.DTOs.Agent;
using DigitalSignage.Api.Entities;
using DigitalSignage.Api.Services;
using DigitalSignage.Api.Services.ExchangeRates;
using DigitalSignage.Api.Services.Weather;
using DigitalSignage.Api.Services.References;
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
    private readonly IAgentExchangeRateService _exchangeRateService;
    private readonly IAgentWeatherService _weatherService;
    private readonly IAgentReferenceService _referenceService;
    private readonly MediaStorageService _storage;
    private readonly ILogger<AgentController> _logger;

    public AgentController(
        DeviceAuthenticationService authenticationService,
        AgentService agentService,
        IAgentExchangeRateService exchangeRateService,
        IAgentWeatherService weatherService,
        IAgentReferenceService referenceService,
        MediaStorageService storage,
        ILogger<AgentController> logger)
    {
        _authenticationService = authenticationService;
        _agentService = agentService;
        _exchangeRateService = exchangeRateService;
        _weatherService = weatherService;
        _referenceService = referenceService;
        _storage = storage;
        _logger = logger;
    }

    [HttpGet("references")]
    public async Task<ActionResult<AgentReferencesResponseDto>> GetReferences(
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

        AgentReferencesResponseDto response =
            await _referenceService.GetAllAsync(cancellationToken);

        _logger.LogInformation(
            "Se entregaron {ReferenceCount} referencias al dispositivo {DeviceId}.",
            response.References.Count,
            device.Id);

        return Ok(response);
    }

    [HttpGet("weather")]
    public async Task<ActionResult<AgentWeatherResponseDto>> GetWeather(
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
            var response = await _weatherService.GetForDeviceAsync(
                device.Id,
                cancellationToken);

            return Ok(response);
        }
        catch (WeatherUnavailableException exception)
        {
            _logger.LogWarning(
                exception,
                "El clima no está disponible para el dispositivo {DeviceId}.",
                device.Id);

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "El clima no está disponible temporalmente."
            });
        }
    }

    [HttpGet("exchange-rates")]
    public async Task<ActionResult<AgentExchangeRatesResponseDto>>
        GetExchangeRates(CancellationToken cancellationToken)
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
            var response = await _exchangeRateService.GetForDeviceAsync(
                device.Id,
                cancellationToken);

            return Ok(response);
        }
        catch (ExchangeRateUnavailableException exception)
        {
            _logger.LogWarning(
                exception,
                "Las tasas no están disponibles para el dispositivo {DeviceId}.",
                device.Id);

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Las tasas de cambio no están disponibles temporalmente."
            });
        }
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

        var filePath = _storage.GetOriginalPath(media.StoredFileName);

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

}
