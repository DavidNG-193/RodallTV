using DigitalSignage.Api.DTOs.PlaylistItems;
using DigitalSignage.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Api.Controllers;

[ApiController]
[Route("api/playlists/{playlistId:guid}/items")]
[Authorize]
public class PlaylistItemsController : ControllerBase
{
    private readonly PlaylistItemService _playlistItemService;

    public PlaylistItemsController(PlaylistItemService playlistItemService)
    {
        _playlistItemService = playlistItemService;
    }

    [HttpGet]
    public async Task<IActionResult> GetByPlaylistId(Guid playlistId)
    {
        var items = await _playlistItemService.GetByPlaylistIdAsync(playlistId);

        if (items is null)
        {
            return NotFound(new { message = "Playlist no encontrada." });
        }

        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Add(
        Guid playlistId,
        AddPlaylistItemRequestDto request)
    {
        if (request.MediaId == Guid.Empty)
        {
            return BadRequest(new { message = "MediaId es obligatorio." });
        }

        try
        {
            var item = await _playlistItemService.AddAsync(playlistId, request);

            if (item is null)
            {
                return NotFound(new { message = "Playlist no encontrada o inactiva." });
            }

            return CreatedAtAction(
                nameof(GetByPlaylistId),
                new { playlistId },
                item);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{itemId:guid}")]
    public async Task<IActionResult> Update(
        Guid playlistId,
        Guid itemId,
        UpdatePlaylistItemRequestDto request)
    {
        try
        {
            var item = await _playlistItemService.UpdateAsync(
                playlistId,
                itemId,
                request);

            if (item is null)
            {
                return NotFound(new { message = "Playlist o elemento no encontrado." });
            }

            return Ok(item);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder(
        Guid playlistId,
        ReorderPlaylistItemsRequestDto request)
    {
        if (request.OrderedItemIds.Count == 0)
        {
            return BadRequest(new
            {
                message = "OrderedItemIds debe contener al menos un elemento."
            });
        }

        try
        {
            var reordered = await _playlistItemService.ReorderAsync(
                playlistId,
                request);

            if (!reordered)
            {
                return NotFound(new { message = "Playlist no encontrada o inactiva." });
            }

            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{itemId:guid}")]
    public async Task<IActionResult> Delete(Guid playlistId, Guid itemId)
    {
        var deleted = await _playlistItemService.DeleteAsync(
            playlistId,
            itemId);

        if (!deleted)
        {
            return NotFound(new { message = "Playlist o elemento no encontrado." });
        }

        return NoContent();
    }
}