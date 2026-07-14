using System.Security.Claims;
using DigitalSignage.Api.DTOs.Playlists;
using DigitalSignage.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlaylistsController : ControllerBase
{
    private readonly PlaylistService _playlistService;

    public PlaylistsController(PlaylistService playlistService)
    {
        _playlistService = playlistService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool includeInactive = false)
    {
        var playlists = await _playlistService
            .GetAllAsync(includeInactive);

        return Ok(playlists);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var playlist = await _playlistService.GetByIdAsync(id);
        return playlist is null ? NotFound() : Ok(playlist);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreatePlaylistRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "El nombre es obligatorio."
            });
        }

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
            var playlist = await _playlistService
                .CreateAsync(request, email);

            return CreatedAtAction(
                nameof(GetById),
                new { id = playlist.Id },
                playlist);
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
        UpdatePlaylistRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new
            {
                message = "El nombre es obligatorio."
            });
        }

        try
        {
            var playlist = await _playlistService
                .UpdateAsync(id, request);

            return playlist is null ? NotFound() : Ok(playlist);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        bool deleted = await _playlistService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}