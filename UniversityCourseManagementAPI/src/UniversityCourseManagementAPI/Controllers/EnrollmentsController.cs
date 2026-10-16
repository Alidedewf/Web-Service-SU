using Microsoft.AspNetCore.Mvc;
using UniversityCourseManagementAPI.Common;
using UniversityCourseManagementAPI.DTO.Enrollments;
using UniversityCourseManagementAPI.Services;

namespace UniversityCourseManagementAPI.Controllers;

[ApiController, Route("api/[controller]")]
public class EnrollmentsController(IUniversityService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll(CancellationToken ct) => ApiResponses.Ok(this, await service.Enrollments(ct));
    [HttpGet("{id:int}")] public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var v = await service.Enrollment(id, ct); return v is null ? ApiResponses.NotFound<EnrollmentDto>(this, "ENROLLMENT_NOT_FOUND", "Enrollment not found") : ApiResponses.Ok(this, v);
    }
    [HttpPost] public async Task<IActionResult> Create(EnrollmentCreateDto dto, CancellationToken ct)
    {
        var r = await service.CreateEnrollment(dto, ct);
        if (r.Error == "STUDENT_NOT_FOUND") return ApiResponses.BadRequest<EnrollmentDto>(this, "STUDENT_NOT_FOUND", "Student does not exist");
        if (r.Error == "COURSE_NOT_FOUND") return ApiResponses.BadRequest<EnrollmentDto>(this, "COURSE_NOT_FOUND", "Course does not exist");
        if (r.Error == "DUPLICATE_ENROLLMENT") return ApiResponses.Conflict<EnrollmentDto>(this, "DUPLICATE_ENROLLMENT", "Student is already enrolled in this course");
        return StatusCode(201, ReturnResult<EnrollmentDto>.Success(201, r.Value, HttpContext.TraceIdentifier));
    }
    [HttpPut("{id:int}/grade")] public async Task<IActionResult> UpdateGrade(int id, EnrollmentGradeUpdateDto dto, CancellationToken ct)
    {
        var r = await service.UpdateGrade(id, dto, ct);
        return r.Error is not null ? ApiResponses.NotFound<EnrollmentDto>(this, "ENROLLMENT_NOT_FOUND", "Enrollment not found") : ApiResponses.Ok(this, r.Value);
    }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (!await service.DeleteEnrollment(id, ct)) return ApiResponses.NotFound<object>(this, "ENROLLMENT_NOT_FOUND", "Enrollment not found");
        return ApiResponses.Ok(this, new { message = "Enrollment deleted" });
    }
}
