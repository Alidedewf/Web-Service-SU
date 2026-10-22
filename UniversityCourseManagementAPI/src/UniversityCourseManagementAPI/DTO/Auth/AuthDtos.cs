using System.ComponentModel.DataAnnotations;

namespace UniversityCourseManagementAPI.DTO.Auth;

public class RegisterDto
{
    [Required, StringLength(80, MinimumLength = 3)] public string UserName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, StringLength(100, MinimumLength = 8)] public string Password { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int StudentId { get; set; }
}

public class LoginDto
{
    [Required] public string UserName { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class AssignRoleDto
{
    [Required] public string Role { get; set; } = string.Empty;
    public int? TeacherId { get; set; }
    public int? StudentId { get; set; }
}

public record AuthTokenDto(string AccessToken, DateTime ExpiresAtUtc, string UserName, IReadOnlyList<string> Roles);
public record CurrentUserDto(string Id, string UserName, string? Email, IReadOnlyList<string> Roles, int? StudentId, int? TeacherId);
