using System.Security.Claims;
using DigitalSignage.Api.Authorization;
using DigitalSignage.Api.DTOs.References;
using DigitalSignage.Api.Services.References;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Api.Controllers;

[ApiController]
[Route("api/daily-references")]
[Authorize]
public sealed class DailyReferencesController : ControllerBase
{
    private readonly IDailyReferenceService _service;

    public DailyReferencesController(IDailyReferenceService service)
    {
        _service = service;
    }

    [HttpGet]
    [HasPermission(PermissionCodes.ReferencesView)]
    public async Task<ActionResult<IReadOnlyList<DailyReferenceDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(cancellationToken));
    }

    [HttpGet("lookup")]
    [HasPermission(PermissionCodes.ReferencesManage)]
    public async Task<ActionResult<ReferenceLookupResponseDto>> Lookup(
        [FromQuery] string referenceNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            ReferenceLookupResponseDto? result = await _service.LookupAsync(
                referenceNumber,
                cancellationToken);

            return result is null
                ? NotFound(new { message = "La referencia no fue encontrada en SagaWS." })
                : Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (SagaReferenceResponseException)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "SagaWS devolvió una respuesta inválida." });
        }
        catch (HttpRequestException)
        {
            return SagaUnavailable();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return SagaUnavailable();
        }
    }

    [HttpPost]
    [HasPermission(PermissionCodes.ReferencesManage)]
    public async Task<ActionResult<DailyReferenceDto>> Create(
        [FromBody] CreateDailyReferenceRequest request,
        CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out Guid userId))
        {
            return Unauthorized();
        }

        try
        {
            DailyReferenceDto created = await _service.CreateAsync(
                request.ReferenceNumber,
                userId,
                cancellationToken);

            return CreatedAtAction(nameof(GetAll), created);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (SagaReferenceResponseException)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { message = "SagaWS devolvió una respuesta inválida." });
        }
        catch (HttpRequestException)
        {
            return SagaUnavailable();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return SagaUnavailable();
        }
    }

    [HttpDelete("{id:guid}")]
    [HasPermission(PermissionCodes.ReferencesManage)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpDelete]
    [HasPermission(PermissionCodes.ReferencesManage)]
    public async Task<IActionResult> DeleteAll(
        CancellationToken cancellationToken)
    {
        await _service.DeleteAllAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("refresh")]
    [HasPermission(PermissionCodes.ReferencesManage)]
    public async Task<ActionResult<RefreshDailyReferencesResultDto>> Refresh(
        CancellationToken cancellationToken)
    {
        RefreshDailyReferencesResultDto result =
            await _service.RefreshAllAsync(cancellationToken);

        return Ok(result);
    }

    private ObjectResult SagaUnavailable() =>
        StatusCode(
            StatusCodes.Status503ServiceUnavailable,
            new { message = "SagaWS no está disponible temporalmente." });
}
