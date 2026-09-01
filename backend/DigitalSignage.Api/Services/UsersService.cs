using DigitalSignage.Api.Authorization;
using DigitalSignage.Api.Data;
using DigitalSignage.Api.DTOs.Users;
using DigitalSignage.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Api.Services;

public class UsersService
{
    private readonly ApplicationDbContext _context;

    public UsersService(ApplicationDbContext context)
    {
        _context = context;
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static bool IsValidRole(string role)
    {
        return UserRoles.All.Contains(
            role,
            StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeRole(string role)
    {
        if (string.Equals(
                role,
                UserRoles.Administrator,
                StringComparison.OrdinalIgnoreCase))
        {
            return UserRoles.Administrator;
        }

        return UserRoles.User;
    }

    private static HashSet<string> ExpandPermissions(
        IEnumerable<string> requestedPermissions)
    {
        var result = new HashSet<string>(
            requestedPermissions
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Select(code => code.Trim().ToLowerInvariant()),
            StringComparer.OrdinalIgnoreCase);

        if (result.Contains(PermissionCodes.DevicesManage))
            result.Add(PermissionCodes.DevicesView);

        if (result.Contains(PermissionCodes.MediaManage))
            result.Add(PermissionCodes.MediaView);

        if (result.Contains(PermissionCodes.PlaylistsManage))
            result.Add(PermissionCodes.PlaylistsView);

        if (result.Contains(PermissionCodes.AssignmentsManage))
            result.Add(PermissionCodes.AssignmentsView);

        if (result.Contains(PermissionCodes.ReferencesManage))
            result.Add(PermissionCodes.ReferencesView);

        return result;
    }

    private async Task<List<Permission>> ResolvePermissionsAsync(
    IEnumerable<string> requestedPermissions,
    string role)
    {
        if (role == UserRoles.Administrator)
        {
            return await _context.Permissions
                .OrderBy(p => p.Code)
                .ToListAsync();
        }

        var requestedCodes = ExpandPermissions(requestedPermissions)
            .ToList();

        var permissions = await _context.Permissions
            .Where(p => requestedCodes.Contains(p.Code))
            .ToListAsync();

        if (permissions.Count != requestedCodes.Count)
        {
            throw new InvalidOperationException(
                "Uno o más permisos no son válidos.");
        }

        return permissions;
    }

    public async Task<List<UserListItemDto>> GetAllAsync()
    {
        return await _context.Users
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                MustChangePassword = u.MustChangePassword,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            })
            .ToListAsync();
    }

    public async Task<UserDetailDto?> GetByIdAsync(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return null;

        var permissions = user.UserPermissions
            .Select(up => up.Permission.Code)
            .OrderBy(code => code)
            .ToList();

        if (user.Role == UserRoles.Administrator)
        {
            permissions = await _context.Permissions
                .Select(p => p.Code)
                .OrderBy(code => code)
                .ToListAsync();
        }

        return new UserDetailDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            MustChangePassword = user.MustChangePassword,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Permissions = permissions
        };
    }

    public async Task<UserDetailDto> CreateAsync(
    CreateUserRequestDto request)
    {
        var email = NormalizeEmail(request.Email);

        var exists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email);

        if (exists)
            throw new InvalidOperationException(
                "Ya existe un usuario con ese correo.");

        if (!IsValidRole(request.Role))
            throw new InvalidOperationException(
                "El rol especificado no es válido.");

        var role = NormalizeRole(request.Role);
        var permissions = await ResolvePermissionsAsync(
            request.Permissions ?? [],
            role);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                request.Password),
            Role = role,
            IsActive = true,
            MustChangePassword = false,
            SessionVersion = 1,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = null
        };

        _context.Users.Add(user);

        foreach (var permission in permissions)
        {
            _context.UserPermissions.Add(new UserPermission
            {
                UserId = user.Id,
                PermissionId = permission.Id
            });
        }

        await _context.SaveChangesAsync();

        return (await GetByIdAsync(user.Id))!;
    }

    private async Task<bool> IsLastActiveAdministratorAsync(Guid userId)
    {
        var target = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.Role, u.IsActive })
            .FirstOrDefaultAsync();

        if (target is null)
            return false;

        if (target.Role != UserRoles.Administrator || !target.IsActive)
            return false;

        var activeAdmins = await _context.Users
            .CountAsync(u =>
                u.Role == UserRoles.Administrator
                && u.IsActive);

        return activeAdmins <= 1;
    }

    public async Task<UserDetailDto?> UpdateAsync(
    Guid id,
    UpdateUserRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.UserPermissions)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return null;

        var email = NormalizeEmail(request.Email);

        var duplicate = await _context.Users
            .AnyAsync(u => u.Id != id && u.Email.ToLower() == email);

        if (duplicate)
            throw new InvalidOperationException(
                "Ya existe un usuario con ese correo.");

        if (!IsValidRole(request.Role))
            throw new InvalidOperationException(
                "El rol especificado no es válido.");

        var newRole = NormalizeRole(request.Role);

        var removingLastAdministrator =
            user.Role == UserRoles.Administrator
            && user.IsActive
            && newRole != UserRoles.Administrator
            && await IsLastActiveAdministratorAsync(id);

        if (removingLastAdministrator)
            throw new InvalidOperationException(
                "No se puede quitar el rol al último administrador activo.");

        var permissions = await ResolvePermissionsAsync(
            request.Permissions ?? [],
            newRole);

        var currentPermissionIds = user.UserPermissions
            .Select(up => up.PermissionId)
            .ToHashSet();

        var newPermissionIds = permissions
            .Select(p => p.Id)
            .ToHashSet();

        var permissionsChanged =
            !currentPermissionIds.SetEquals(newPermissionIds);

        var roleChanged = user.Role != newRole;

        var emailChanged = !string.Equals(
            user.Email,
            email,
            StringComparison.OrdinalIgnoreCase);

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = email;
        user.Role = newRole;

        var removedPermissions = user.UserPermissions
            .Where(userPermission =>
                !newPermissionIds.Contains(userPermission.PermissionId))
            .ToList();

        _context.UserPermissions.RemoveRange(removedPermissions);

        foreach (var permission in permissions
                     .Where(permission =>
                         !currentPermissionIds.Contains(permission.Id)))
        {
            _context.UserPermissions.Add(new UserPermission
            {
                UserId = user.Id,
                PermissionId = permission.Id
            });
        }

        if (roleChanged || permissionsChanged || emailChanged)
            user.SessionVersion++;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<bool> UpdateStatusAsync(
    Guid id,
    bool isActive)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return false;

        if (!isActive
            && user.IsActive
            && user.Role == UserRoles.Administrator
            && await IsLastActiveAdministratorAsync(id))
        {
            throw new InvalidOperationException(
                "No se puede desactivar al último administrador activo.");
        }

        if (user.IsActive != isActive)
        {
            user.IsActive = isActive;
            user.SessionVersion++;
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> ResetPasswordAsync(
    Guid id,
    string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(
            password);

        user.MustChangePassword = false;
        user.SessionVersion++;

        await _context.SaveChangesAsync();

        return true;
    }
}
