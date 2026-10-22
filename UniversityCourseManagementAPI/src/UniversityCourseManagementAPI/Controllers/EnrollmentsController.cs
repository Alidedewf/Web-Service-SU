using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using UniversityCourseManagementAPI.Common;
using UniversityCourseManagementAPI.DTO.Enrollments;
using UniversityCourseManagementAPI.Models;
using UniversityCourseManagementAPI.Repositories;
using UniversityCourseManagementAPI.Services;

namespace UniversityCourseManagementAPI.Controllers;

[ApiController, ApiVersionNeutral, Route("api/[controller]")]
public class EnrollmentsController(IUniversityService service, IEnrollmentRepository enrollments,
    UserManager<ApplicationUser> users) : ControllerBase
{
    [Authorize(Policy = Policies.AdminOnly), HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => ApiResponses.Ok(this, await service.Enrollments(ct));

    [Authorize(Policy = Policies.StudentOnly), HttpGet("my")]
    public async Task<IActionResult> GetMy(CancellationToken ct)
    {
        var studentId = (await users.GetUserAsync(User))?.StudentId;
        if (!studentId.HasValue) return ApiResponses.Forbidden<IReadOnlyList<EnrollmentDto>>(this, "STUDENT_LINK_REQUIRED", "This account is not linked to a student");
        var result = (await service.Enrollments(ct)).Where(x => x.StudentId == studentId.Value).ToList();
        return ApiResponses.Ok(this, result);
    }

    [Authorize(Policy = Policies.CanViewEnrollment), HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var value = await service.Enrollment(id, ct);
        if (value is null) return ApiResponses.NotFound<EnrollmentDto>(this, "ENROLLMENT_NOT_FOUND", "Enrollment not found");
        if (User.IsInRole(Roles.Student) && (await users.GetUserAsync(User))?.StudentId != value.StudentId)
            return ApiResponses.Forbidden<EnrollmentDto>(this, "ENROLLMENT_ACCESS_DENIED", "You can view only your own enrollment");
        return ApiResponses.Ok(this, value);
    }

    [Authorize(Policy = Policies.AdminOnly), HttpPost]
    public async Task<IActionResult> Create(EnrollmentCreateDto dto, CancellationToken ct)
    {
        var r = await service.CreateEnrollment(dto, ct);
        if (r.Error == "STUDENT_NOT_FOUND") return ApiResponses.NotFound<EnrollmentDto>(this, r.Error, "Student does not exist");
        if (r.Error == "COURSE_NOT_FOUND") return ApiResponses.NotFound<EnrollmentDto>(this, r.Error, "Course does not exist");
        return r.Error == "DUPLICATE_ENROLLMENT" ? ApiResponses.Conflict<EnrollmentDto>(this, r.Error, "Student is already enrolled in this course") : StatusCode(201, ReturnResult<EnrollmentDto>.Success(201, r.Value, HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = Policies.CanGrade), HttpPut("{id:int}/grade")]
    public async Task<IActionResult> UpdateGrade(int id, EnrollmentGradeUpdateDto dto, CancellationToken ct)
    {
        var enrollment = await enrollments.GetByIdAsync(id, ct);
        if (enrollment is null) return ApiResponses.NotFound<EnrollmentDto>(this, "ENROLLMENT_NOT_FOUND", "Enrollment not found");
        if (User.IsInRole(Roles.Teacher) && (await users.GetUserAsync(User))?.TeacherId != enrollment.Course?.TeacherId)
            return ApiResponses.Forbidden<EnrollmentDto>(this, "COURSE_ACCESS_DENIED", "You can update grades only for your own courses");
        var r = await service.UpdateGrade(id, dto, ct);
        return ApiResponses.Ok(this, r.Value);
    }

    [Authorize(Policy = Policies.AdminOnly), HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        !await service.DeleteEnrollment(id, ct) ? ApiResponses.NotFound<object>(this, "ENROLLMENT_NOT_FOUND", "Enrollment not found") : ApiResponses.Ok(this, new { message = "Enrollment deleted" });
}
