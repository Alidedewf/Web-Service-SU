using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UniversityCourseManagementAPI.Data;
using UniversityCourseManagementAPI.DTO.Auth;
using UniversityCourseManagementAPI.Models;

namespace UniversityCourseManagementAPI.Services;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";
    public static readonly string[] All = [Admin, Teacher, Student];
}

public static class Policies
{
    public const string AdminOnly = "AdminOnly";
    public const string CanGrade = "CanGrade";
    public const string CanViewEnrollment = "CanViewEnrollment";
    public const string StudentOnly = "StudentOnly";
}

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = "UniversityCourseManagementAPI";
    public string Audience { get; init; } = "UniversityCourseManagementClient";
    public int AccessTokenMinutes { get; init; } = 30;
    // Value comes only from Jwt__Key environment variable / user secrets.
    public string? Key { get; init; }
}

public interface IAuthService
{
    Task<(string? Error, string? Description)> RegisterAsync(RegisterDto dto, CancellationToken ct);
    Task<(string? Error, AuthTokenDto? Token)> LoginAsync(LoginDto dto);
    Task<CurrentUserDto?> MeAsync(ClaimsPrincipal principal);
    Task<(string? Error, string? Description)> AssignRoleAsync(string userId, AssignRoleDto dto, CancellationToken ct);
}

public sealed class AuthService(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn,
    RoleManager<IdentityRole> roles, ApplicationDbContext db, IOptions<JwtOptions> jwtOptions,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<(string? Error, string? Description)> RegisterAsync(RegisterDto dto, CancellationToken ct)
    {
        if (await users.FindByNameAsync(dto.UserName) is not null) return ("USERNAME_CONFLICT", "Username already exists");
        if (await users.FindByEmailAsync(dto.Email) is not null) return ("EMAIL_CONFLICT", "Email already exists");
        var student = await db.Students.AnyAsync(x => x.Id == dto.StudentId, ct);
        if (!student) return ("STUDENT_NOT_FOUND", "Student does not exist");
        if (await db.Users.AnyAsync(x => x.StudentId == dto.StudentId, ct)) return ("STUDENT_ALREADY_LINKED", "Student already has an account");

        var user = new ApplicationUser { UserName = dto.UserName, Email = dto.Email, StudentId = dto.StudentId, EmailConfirmed = true };
        var result = await users.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return ("REGISTRATION_FAILED", string.Join("; ", result.Errors.Select(x => x.Description)));
        await users.AddToRoleAsync(user, Roles.Student);
        logger.LogInformation("Registered user {UserId} for student {StudentId}", user.Id, dto.StudentId);
        return (null, null);
    }

    public async Task<(string? Error, AuthTokenDto? Token)> LoginAsync(LoginDto dto)
    {
        var user = await users.FindByNameAsync(dto.UserName);
        if (user is null) return ("INVALID_CREDENTIALS", null);
        var result = await signIn.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (result.IsLockedOut) return ("ACCOUNT_LOCKED", null);
        if (!result.Succeeded) return ("INVALID_CREDENTIALS", null);
        var userRoles = await users.GetRolesAsync(user);
        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty)
        };
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key!)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_jwt.Issuer, _jwt.Audience, claims, expires: expires, signingCredentials: credentials);
        logger.LogInformation("User {UserId} successfully authenticated", user.Id);
        return (null, new AuthTokenDto(new JwtSecurityTokenHandler().WriteToken(token), expires, user.UserName ?? string.Empty, userRoles.ToArray()));
    }

    public async Task<CurrentUserDto?> MeAsync(ClaimsPrincipal principal)
    {
        var user = await users.GetUserAsync(principal);
        if (user is null) return null;
        return new CurrentUserDto(user.Id, user.UserName ?? string.Empty, user.Email, (await users.GetRolesAsync(user)).ToArray(), user.StudentId, user.TeacherId);
    }

    public async Task<(string? Error, string? Description)> AssignRoleAsync(string userId, AssignRoleDto dto, CancellationToken ct)
    {
        if (!Roles.All.Contains(dto.Role, StringComparer.Ordinal) || !await roles.RoleExistsAsync(dto.Role)) return ("INVALID_ROLE", "Role must be Admin, Teacher or Student");
        var user = await users.FindByIdAsync(userId);
        if (user is null) return ("USER_NOT_FOUND", "User not found");
        if (dto.TeacherId.HasValue && !await db.Teachers.AnyAsync(x => x.Id == dto.TeacherId, ct)) return ("TEACHER_NOT_FOUND", "Teacher does not exist");
        if (dto.StudentId.HasValue && !await db.Students.AnyAsync(x => x.Id == dto.StudentId, ct)) return ("STUDENT_NOT_FOUND", "Student does not exist");
        if (dto.Role == Roles.Teacher && !dto.TeacherId.HasValue) return ("TEACHER_LINK_REQUIRED", "TeacherId is required for Teacher role");
        if (dto.Role == Roles.Student && !dto.StudentId.HasValue) return ("STUDENT_LINK_REQUIRED", "StudentId is required for Student role");

        await users.RemoveFromRolesAsync(user, await users.GetRolesAsync(user));
        user.TeacherId = dto.Role == Roles.Teacher ? dto.TeacherId : null;
        user.StudentId = dto.Role == Roles.Student ? dto.StudentId : null;
        var update = await users.UpdateAsync(user);
        if (!update.Succeeded) return ("UPDATE_FAILED", string.Join("; ", update.Errors.Select(x => x.Description)));
        await users.AddToRoleAsync(user, dto.Role);
        logger.LogInformation("Assigned role {Role} to user {UserId}", dto.Role, userId);
        return (null, null);
    }
}
