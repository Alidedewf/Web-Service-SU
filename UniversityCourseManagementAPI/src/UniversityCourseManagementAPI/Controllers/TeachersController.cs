using Microsoft.AspNetCore.Mvc;
using UniversityCourseManagementAPI.Common;
using UniversityCourseManagementAPI.DTO.Teachers;
using UniversityCourseManagementAPI.Services;

namespace UniversityCourseManagementAPI.Controllers;

[ApiController, Route("api/[controller]")]
public class TeachersController(IUniversityService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll(CancellationToken ct) => ApiResponses.Ok(this, await service.Teachers(ct));
    [HttpGet("{id:int}")] public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var v = await service.Teacher(id, ct); return v is null ? ApiResponses.NotFound<TeacherDto>(this, "TEACHER_NOT_FOUND", "Teacher not found") : ApiResponses.Ok(this, v);
    }
    [HttpPost] public async Task<IActionResult> Create(TeacherCreateDto dto, CancellationToken ct)
    {
        var r = await service.CreateTeacher(dto, ct);
        if (r.Error == "EMAIL_CONFLICT") return ApiResponses.Conflict<TeacherDto>(this, "EMAIL_CONFLICT", "A teacher with this email already exists");
        return StatusCode(201, ReturnResult<TeacherDto>.Success(201, r.Value, HttpContext.TraceIdentifier));
    }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, TeacherUpdateDto dto, CancellationToken ct)
    {
        var r = await service.UpdateTeacher(id, dto, ct);
        if (r.Error == "NOT_FOUND") return ApiResponses.NotFound<TeacherDto>(this, "TEACHER_NOT_FOUND", "Teacher not found");
        if (r.Error == "EMAIL_CONFLICT") return ApiResponses.Conflict<TeacherDto>(this, "EMAIL_CONFLICT", "A teacher with this email already exists");
        return ApiResponses.Ok(this, r.Value);
    }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var error = await service.DeleteTeacher(id, ct);
        if (error == "NOT_FOUND") return ApiResponses.NotFound<object>(this, "TEACHER_NOT_FOUND", "Teacher not found");
        if (error == "HAS_COURSES") return ApiResponses.Conflict<object>(this, "TEACHER_HAS_COURSES", "Cannot delete a teacher who owns courses");
        return ApiResponses.Ok(this, new { message = "Teacher deleted" });
    }
}
