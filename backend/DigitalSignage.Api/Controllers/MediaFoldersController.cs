using System.Security.Claims;
using DigitalSignage.Api.DTOs.MediaFolders;
using DigitalSignage.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MediaFoldersController : ControllerBase
{
    private readonly MediaFolderService _mediaFolderService;

    public MediaFoldersController(MediaFolderService mediaFolderService)
    {
        _mediaFolderService = mediaFolderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var folders = await _mediaFolderService.GetAllAsync(includeInactive);
        return Ok(folders);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var folder = await _mediaFolderService.GetByIdAsync(id);
        return folder is null ? NotFound() : Ok(folder);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMediaFolderRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "El nombre es obligatorio." });
        }

        string? email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email")
            ?? User.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized(new { message = "El token no contiene el correo del usuario." });
        }

        try
        {
            var folder = await _mediaFolderService.CreateAsync(request, email);

            return CreatedAtAction(
                nameof(GetById),
                new { id = folder.Id },
                folder);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateMediaFolderRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "El nombre es obligatorio." });
        }

        try
        {
            var folder = await _mediaFolderService.UpdateAsync(id, request);
            return folder is null ? NotFound() : Ok(folder);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        bool deleted = await _mediaFolderService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}