using Microsoft.AspNetCore.Mvc;
using UniversityCourseManagementAPI.Common;
using UniversityCourseManagementAPI.DTO.Courses;
using UniversityCourseManagementAPI.Services;

namespace UniversityCourseManagementAPI.Controllers;

[ApiController, Route("api/[controller]")]
public class CoursesController(IUniversityService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? teacherId,
        [FromQuery] int? minCredits, [FromQuery] int? maxCredits, CancellationToken ct)
    {
        if (minCredits < 1 || maxCredits > 30 || (minCredits.HasValue && maxCredits.HasValue && minCredits > maxCredits))
            return ApiResponses.BadRequest<IReadOnlyList<CourseDto>>(this, "INVALID_FILTER", "Credit filters must be within 1..30 and minCredits must not exceed maxCredits");
        return ApiResponses.Ok(this, await service.Courses(search, teacherId, minCredits, maxCredits, ct));
    }
    [HttpGet("{id:int}")] public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var v = await service.Course(id, ct); return v is null ? ApiResponses.NotFound<CourseDto>(this, "COURSE_NOT_FOUND", "Course not found") : ApiResponses.Ok(this, v);
    }
    [HttpPost] public async Task<IActionResult> Create(CourseCreateDto dto, CancellationToken ct)
    {
        var r = await service.CreateCourse(dto, ct);
        if (r.Error == "TEACHER_NOT_FOUND") return ApiResponses.BadRequest<CourseDto>(this, "TEACHER_NOT_FOUND", "Teacher does not exist");
        return StatusCode(201, ReturnResult<CourseDto>.Success(201, r.Value, HttpContext.TraceIdentifier));
    }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, CourseUpdateDto dto, CancellationToken ct)
    {
        var r = await service.UpdateCourse(id, dto, ct);
        if (r.Error == "NOT_FOUND") return ApiResponses.NotFound<CourseDto>(this, "COURSE_NOT_FOUND", "Course not found");
        if (r.Error == "TEACHER_NOT_FOUND") return ApiResponses.BadRequest<CourseDto>(this, "TEACHER_NOT_FOUND", "Teacher does not exist");
        return ApiResponses.Ok(this, r.Value);
    }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!await service.DeleteCourse(id, ct)) return ApiResponses.NotFound<object>(this, "COURSE_NOT_FOUND", "Course not found");
        return ApiResponses.Ok(this, new { message = "Course deleted" });
    }
}
