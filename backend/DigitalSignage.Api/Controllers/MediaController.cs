using System.Security.Claims;
using DigitalSignage.Api.DTOs.Media;
using DigitalSignage.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly MediaService _mediaService;

    public MediaController(MediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false,
        [FromQuery] Guid? mediaFolderId = null)
    {
        var media = await _mediaService.GetAllAsync(
            includeInactive,
            mediaFolderId);

        return Ok(media);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var media = await _mediaService.GetByIdAsync(id);
        return media is null ? NotFound() : Ok(media);
    }

    [HttpGet("{id:guid}/file")]
    public async Task<IActionResult> GetFile(Guid id)
    {
        try
        {
            var file = await _mediaService.GetFileAsync(id);

            if (file is null)
            {
                return NotFound();
            }

            return PhysicalFile(
                file.PhysicalPath,
                file.ContentType,
                file.DownloadFileName,
                enableRangeProcessing: true);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        [FromForm] UploadMediaRequestDto request)
    {
        string? email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email")
            ?? User.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new
            {
                message = "El token no contiene el correo del usuario."
            });
        }

        try
        {
            var media = await _mediaService.UploadAsync(request, email);

            return CreatedAtAction(
                nameof(GetById),
                new { id = media.Id },
                media);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        bool deleted = await _mediaService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}