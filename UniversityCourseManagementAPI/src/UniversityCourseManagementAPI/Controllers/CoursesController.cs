using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using UniversityCourseManagementAPI.Common;
using UniversityCourseManagementAPI.DTO.Courses;
using UniversityCourseManagementAPI.Services;

namespace UniversityCourseManagementAPI.Controllers;

[ApiController, ApiVersionNeutral, Route("api/[controller]")]
public class CoursesController(IUniversityService service) : ControllerBase
{
    [AllowAnonymous, HttpGet] public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? teacherId, [FromQuery] int? minCredits, [FromQuery] int? maxCredits, CancellationToken ct)
    { if (minCredits < 1 || maxCredits > 30 || (minCredits.HasValue && maxCredits.HasValue && minCredits > maxCredits)) return ApiResponses.BadRequest<IReadOnlyList<CourseDto>>(this, "INVALID_FILTER", "Credit filters must be within 1..30 and minCredits must not exceed maxCredits"); return ApiResponses.Ok(this, await service.Courses(search, teacherId, minCredits, maxCredits, ct)); }
    [AllowAnonymous, HttpGet("{id:int}")] public async Task<IActionResult> GetById(int id, CancellationToken ct)
    { var v = await service.Course(id, ct); return v is null ? ApiResponses.NotFound<CourseDto>(this, "COURSE_NOT_FOUND", "Course not found") : ApiResponses.Ok(this, v); }
    [Authorize(Policy = Policies.AdminOnly), HttpPost] public async Task<IActionResult> Create(CourseCreateDto dto, CancellationToken ct)
    { var r = await service.CreateCourse(dto, ct); return r.Error is not null ? ApiResponses.NotFound<CourseDto>(this, "TEACHER_NOT_FOUND", "Teacher does not exist") : StatusCode(201, ReturnResult<CourseDto>.Success(201, r.Value, HttpContext.TraceIdentifier)); }
    [Authorize(Policy = Policies.AdminOnly), HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, CourseUpdateDto dto, CancellationToken ct)
    { var r = await service.UpdateCourse(id, dto, ct); if (r.Error == "NOT_FOUND") return ApiResponses.NotFound<CourseDto>(this, "COURSE_NOT_FOUND", "Course not found"); return r.Error is not null ? ApiResponses.NotFound<CourseDto>(this, "TEACHER_NOT_FOUND", "Teacher does not exist") : ApiResponses.Ok(this, r.Value); }
    [Authorize(Policy = Policies.AdminOnly), HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct) => !await service.DeleteCourse(id, ct) ? ApiResponses.NotFound<object>(this, "COURSE_NOT_FOUND", "Course not found") : ApiResponses.Ok(this, new { message = "Course deleted" });
}
