using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DigitalSignage.Api.Authorization;
using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.Auth;
using DigitalSignage.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DigitalSignage.Api.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        bool passwordIsValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!passwordIsValid)
        {
            return null;
        }

        var permissions = await GetPermissionsAsync(user);

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        string token = GenerateJwtToken(
            user.Id,
            user.Email,
            user.Role,
            user.SessionVersion,
            user.MustChangePassword,
            permissions);

        return new LoginResponseDto
        {
            Token = token,
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            User = user.Id.ToString(),
            MustChangePassword = user.MustChangePassword,
            Permissions = permissions
        };
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user is null)
        {
            return null;
        }

        var permissions = await GetPermissionsAsync(user);

        return new CurrentUserDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            MustChangePassword = user.MustChangePassword,
            Permissions = permissions
        };
    }

    private async Task<List<string>> GetPermissionsAsync(User user)
    {
        if (user.Role == "Administrator")
        {
            return await _context.Permissions
                .Select(permission => permission.Code)
                .OrderBy(code => code)
                .ToListAsync();
        }

        return user.UserPermissions
            .Select(userPermission => userPermission.Permission.Code)
            .Distinct()
            .OrderBy(code => code)
            .ToList();
    }

    private string GenerateJwtToken(
        Guid userId,
        string email,
        string role,
        int sessionVersion,
        bool mustChangePassword,
        IEnumerable<string> permissions)
    {
        var jwtKey = _configuration["Jwt:Key"]!;
        var jwtIssuer = _configuration["Jwt:Issuer"]!;
        var jwtAudience = _configuration["Jwt:Audience"]!;
        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"]!);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(CustomClaimTypes.SessionVersion, sessionVersion.ToString()),
            new Claim(
                CustomClaimTypes.MustChangePassword,
                mustChangePassword.ToString().ToLowerInvariant())
        };

        claims.AddRange(permissions.Select(permission =>
            new Claim(CustomClaimTypes.Permission, permission)));

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
