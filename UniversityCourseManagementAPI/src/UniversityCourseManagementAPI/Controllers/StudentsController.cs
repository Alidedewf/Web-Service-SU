using Microsoft.AspNetCore.Mvc;
using UniversityCourseManagementAPI.Common;
using UniversityCourseManagementAPI.DTO.Students;
using UniversityCourseManagementAPI.Services;

namespace UniversityCourseManagementAPI.Controllers;

[ApiController, Route("api/[controller]")]
public class StudentsController(IUniversityService service, ILogger<StudentsController> logger) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll(CancellationToken ct) => ApiResponses.Ok(this, await service.Students(ct));
    [HttpGet("{id:int}")] public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var value = await service.Student(id, ct); if (value is null) { logger.LogWarning("Student {StudentId} not found", id); return ApiResponses.NotFound<StudentDto>(this, "STUDENT_NOT_FOUND", "Student not found"); }
        return ApiResponses.Ok(this, value);
    }
    [HttpGet("{id:int}/courses")] public async Task<IActionResult> GetCourses(int id, CancellationToken ct)
    {
        var value = await service.StudentCourses(id, ct); if (value is null) return ApiResponses.NotFound<StudentCourseDto>(this, "STUDENT_NOT_FOUND", "Student not found");
        return ApiResponses.Ok(this, value);
    }
    [HttpPost] public async Task<IActionResult> Create(StudentCreateDto dto, CancellationToken ct)
    {
        var value = await service.CreateStudent(dto, ct); if (value is null) return ApiResponses.Conflict<StudentDto>(this, "EMAIL_CONFLICT", "A student with this email already exists");
        return StatusCode(201, ReturnResult<StudentDto>.Success(201, value, HttpContext.TraceIdentifier));
    }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, StudentUpdateDto dto, CancellationToken ct)
    {
        var r = await service.UpdateStudent(id, dto, ct);
        if (r.Error == "NOT_FOUND") return ApiResponses.NotFound<StudentDto>(this, "STUDENT_NOT_FOUND", "Student not found");
        if (r.Error == "EMAIL_CONFLICT") return ApiResponses.Conflict<StudentDto>(this, "EMAIL_CONFLICT", "A student with this email already exists");
        return ApiResponses.Ok(this, r.Value);
    }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!await service.DeleteStudent(id, ct)) return ApiResponses.NotFound<object>(this, "STUDENT_NOT_FOUND", "Student not found");
        return ApiResponses.Ok(this, new { message = "Student deleted" });
    }
}
