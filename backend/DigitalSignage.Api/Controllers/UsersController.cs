using DigitalSignage.Api.Authorization;
using DigitalSignage.Api.DTOs.Users;
using DigitalSignage.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
[HasPermission(PermissionCodes.UsersManage)]
public class UsersController : ControllerBase
{
    private readonly UsersService _usersService;

    public UsersController(UsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _usersService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _usersService.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequestDto request)
    {
        try
        {
            var user = await _usersService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = user.Id },
                user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserRequestDto request)
    {
        try
        {
            var user = await _usersService.UpdateAsync(id, request);

            if (user is null)
                return NotFound();

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateUserStatusRequestDto request)
    {
        try
        {
            var updated = await _usersService.UpdateStatusAsync(
                id,
                request.IsActive);

            if (!updated)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(
        Guid id,
        [FromBody] ResetPasswordRequestDto request)
    {
        var updated = await _usersService.ResetPasswordAsync(
            id,
            request.TemporaryPassword);

        if (!updated)
            return NotFound();

        return NoContent();
    }
}