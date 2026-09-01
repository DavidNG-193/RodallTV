using System.Security.Claims;
using DigitalSignage.Api.DTOs.Auth;
using DigitalSignage.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);

        if (response == null)
        {
            return Unauthorized(new
            {
                message = "Credenciales inválidas"
            });
        }

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> Me()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetCurrentUserAsync(userId);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequestDto request)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var result =
            await _authService.ChangePasswordAsync(
                userId,
                request);

        return result switch
        {
            ChangePasswordResult.Success
                => NoContent(),

            ChangePasswordResult.UserNotFound
                => Unauthorized(),

            ChangePasswordResult.InvalidCurrentPassword
                => BadRequest(new
                {
                    message = "La contraseña actual no es correcta."
                }),

            ChangePasswordResult.PasswordsDoNotMatch
                => BadRequest(new
                {
                    message = "La nueva contraseña y su confirmación no coinciden."
                }),

            ChangePasswordResult.SamePassword
                => BadRequest(new
                {
                    message = "La nueva contraseña debe ser diferente de la actual."
                }),

            _ => BadRequest()
        };
    }
}
