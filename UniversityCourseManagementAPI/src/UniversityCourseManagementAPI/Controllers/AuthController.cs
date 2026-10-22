using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using UniversityCourseManagementAPI.Common;
using UniversityCourseManagementAPI.DTO.Auth;
using UniversityCourseManagementAPI.Services;

namespace UniversityCourseManagementAPI.Controllers;

[ApiController, ApiVersionNeutral, Route("api/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [AllowAnonymous, HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto, CancellationToken ct)
    {
        var result = await auth.RegisterAsync(dto, ct);
        if (result.Error is null) return StatusCode(201, ReturnResult<object>.Success(201, new { message = "User registered" }, HttpContext.TraceIdentifier));
        return result.Error is "USERNAME_CONFLICT" or "EMAIL_CONFLICT" or "STUDENT_ALREADY_LINKED"
            ? ApiResponses.Conflict<object>(this, result.Error, result.Description!)
            : ApiResponses.BadRequest<object>(this, result.Error, result.Description!);
    }

    [AllowAnonymous, HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await auth.LoginAsync(dto);
        if (result.Error is null) return ApiResponses.Ok(this, result.Token);
        return result.Error == "ACCOUNT_LOCKED"
            ? ApiResponses.Unauthorized<AuthTokenDto>(this, result.Error, "Account is temporarily locked after several failed login attempts. Try again later")
            : ApiResponses.Unauthorized<AuthTokenDto>(this, result.Error, "Invalid username or password");
    }

    [Authorize, HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var current = await auth.MeAsync(User);
        return current is null ? ApiResponses.Unauthorized<CurrentUserDto>(this, "UNAUTHORIZED", "Authentication is required") : ApiResponses.Ok(this, current);
    }

    [Authorize(Policy = Policies.AdminOnly), HttpPut("users/{userId}/role")]
    public async Task<IActionResult> AssignRole(string userId, AssignRoleDto dto, CancellationToken ct)
    {
        var result = await auth.AssignRoleAsync(userId, dto, ct);
        if (result.Error is null) return ApiResponses.Ok(this, new { message = "Role assigned" });
        return result.Error == "USER_NOT_FOUND"
            ? ApiResponses.NotFound<object>(this, result.Error, result.Description!)
            : ApiResponses.BadRequest<object>(this, result.Error, result.Description!);
    }
}
