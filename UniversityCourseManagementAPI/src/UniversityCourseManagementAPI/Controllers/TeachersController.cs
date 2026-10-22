using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using UniversityCourseManagementAPI.Common;
using UniversityCourseManagementAPI.DTO.Teachers;
using UniversityCourseManagementAPI.Services;

namespace UniversityCourseManagementAPI.Controllers;

[ApiController, ApiVersionNeutral, Route("api/[controller]")]
public class TeachersController(IUniversityService service) : ControllerBase
{
    // Email is personal data: only Admin receives the full TeacherDto.
    [AllowAnonymous, HttpGet] public async Task<IActionResult> GetAll(CancellationToken ct)
    { var list = await service.Teachers(ct); return User.IsInRole(Roles.Admin) ? ApiResponses.Ok(this, list) : ApiResponses.Ok(this, list.Select(TeacherPublicDto.From).ToList()); }
    [AllowAnonymous, HttpGet("{id:int}")] public async Task<IActionResult> GetById(int id, CancellationToken ct)
    { var v = await service.Teacher(id, ct); if (v is null) return ApiResponses.NotFound<TeacherDto>(this, "TEACHER_NOT_FOUND", "Teacher not found"); return User.IsInRole(Roles.Admin) ? ApiResponses.Ok(this, v) : ApiResponses.Ok(this, TeacherPublicDto.From(v)); }
    [Authorize(Policy = Policies.AdminOnly), HttpPost] public async Task<IActionResult> Create(TeacherCreateDto dto, CancellationToken ct)
    { var r = await service.CreateTeacher(dto, ct); return r.Error is not null ? ApiResponses.Conflict<TeacherDto>(this, "EMAIL_CONFLICT", "A teacher with this email already exists") : StatusCode(201, ReturnResult<TeacherDto>.Success(201, r.Value, HttpContext.TraceIdentifier)); }
    [Authorize(Policy = Policies.AdminOnly), HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, TeacherUpdateDto dto, CancellationToken ct)
    { var r = await service.UpdateTeacher(id, dto, ct); if (r.Error == "NOT_FOUND") return ApiResponses.NotFound<TeacherDto>(this, "TEACHER_NOT_FOUND", "Teacher not found"); return r.Error is not null ? ApiResponses.Conflict<TeacherDto>(this, "EMAIL_CONFLICT", "A teacher with this email already exists") : ApiResponses.Ok(this, r.Value); }
    [Authorize(Policy = Policies.AdminOnly), HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct)
    { var error = await service.DeleteTeacher(id, ct); if (error == "NOT_FOUND") return ApiResponses.NotFound<object>(this, "TEACHER_NOT_FOUND", "Teacher not found"); return error == "HAS_COURSES" ? ApiResponses.Conflict<object>(this, "TEACHER_HAS_COURSES", "Cannot delete a teacher who owns courses") : ApiResponses.Ok(this, new { message = "Teacher deleted" }); }
}
