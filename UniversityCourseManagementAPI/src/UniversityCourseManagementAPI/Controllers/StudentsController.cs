using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using UniversityCourseManagementAPI.Common;
using UniversityCourseManagementAPI.DTO.Students;
using UniversityCourseManagementAPI.Models;
using UniversityCourseManagementAPI.Services;

namespace UniversityCourseManagementAPI.Controllers;

[ApiController, ApiVersionNeutral, Route("api/[controller]")]
public class StudentsController(IUniversityService service, UserManager<ApplicationUser> users, ILogger<StudentsController> logger) : ControllerBase
{
    [Authorize(Policy = Policies.AdminOnly), HttpGet] public async Task<IActionResult> GetAll(CancellationToken ct) => ApiResponses.Ok(this, await service.Students(ct));
    [Authorize(Policy = Policies.AdminOnly), HttpGet("{id:int}")] public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var value = await service.Student(id, ct);
        if (value is null) { logger.LogWarning("Student {StudentId} not found", id); return ApiResponses.NotFound<StudentDto>(this, "STUDENT_NOT_FOUND", "Student not found"); }
        return ApiResponses.Ok(this, value);
    }
    [Authorize(Policy = Policies.CanViewEnrollment), HttpGet("{id:int}/courses")] public async Task<IActionResult> GetCourses(int id, CancellationToken ct)
    {
        // The Student role alone is not enough: a student may read grades only of the linked Student record.
        if (!User.IsInRole(Roles.Admin) && (await users.GetUserAsync(User))?.StudentId != id)
            return ApiResponses.Forbidden<StudentCourseDto>(this, "STUDENT_ACCESS_DENIED", "You can view only your own courses and grades");
        var value = await service.StudentCourses(id, ct);
        return value is null ? ApiResponses.NotFound<StudentCourseDto>(this, "STUDENT_NOT_FOUND", "Student not found") : ApiResponses.Ok(this, value);
    }
    [Authorize(Policy = Policies.AdminOnly), HttpPost] public async Task<IActionResult> Create(StudentCreateDto dto, CancellationToken ct)
    {
        var value = await service.CreateStudent(dto, ct);
        return value is null ? ApiResponses.Conflict<StudentDto>(this, "EMAIL_CONFLICT", "A student with this email already exists") : StatusCode(201, ReturnResult<StudentDto>.Success(201, value, HttpContext.TraceIdentifier));
    }
    [Authorize(Policy = Policies.AdminOnly), HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, StudentUpdateDto dto, CancellationToken ct)
    {
        var r = await service.UpdateStudent(id, dto, ct);
        if (r.Error == "NOT_FOUND") return ApiResponses.NotFound<StudentDto>(this, "STUDENT_NOT_FOUND", "Student not found");
        return r.Error == "EMAIL_CONFLICT" ? ApiResponses.Conflict<StudentDto>(this, "EMAIL_CONFLICT", "A student with this email already exists") : ApiResponses.Ok(this, r.Value);
    }
    [Authorize(Policy = Policies.AdminOnly), HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        !await service.DeleteStudent(id, ct) ? ApiResponses.NotFound<object>(this, "STUDENT_NOT_FOUND", "Student not found") : ApiResponses.Ok(this, new { message = "Student deleted" });
}
