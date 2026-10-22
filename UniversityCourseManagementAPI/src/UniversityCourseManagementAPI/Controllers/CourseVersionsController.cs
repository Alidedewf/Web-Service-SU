using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniversityCourseManagementAPI.Common;
using UniversityCourseManagementAPI.DTO.Courses;
using UniversityCourseManagementAPI.Repositories;

namespace UniversityCourseManagementAPI.Controllers;

[ApiController, ApiVersion("1.0"), ApiExplorerSettings(GroupName = "v1"), Route("api/v{version:apiVersion}/courses")]
public class CoursesV1Controller(ICourseRepository courses) : ControllerBase
{
    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await courses.GetAllAsync(null, null, null, null, ct);
        return ApiResponses.Ok(this, result.Select(x => new CourseV1Dto(x.Id, x.Name, x.Description, x.Credits, x.TeacherId, x.CreatedAt)).ToList());
    }

    [AllowAnonymous, HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var x = await courses.GetByIdAsync(id, ct);
        return x is null ? ApiResponses.NotFound<CourseV1Dto>(this, "COURSE_NOT_FOUND", "Course not found") : ApiResponses.Ok(this, new CourseV1Dto(x.Id, x.Name, x.Description, x.Credits, x.TeacherId, x.CreatedAt));
    }
}

[ApiController, ApiVersion("2.0"), ApiExplorerSettings(GroupName = "v2"), Route("api/v{version:apiVersion}/courses")]
public class CoursesV2Controller(ICourseRepository courses) : ControllerBase
{
    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => ApiResponses.Ok(this, (await courses.GetForVersion2Async(ct)).Select(ToDto).ToList());

    [AllowAnonymous, HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var x = await courses.GetForVersion2ByIdAsync(id, ct);
        return x is null ? ApiResponses.NotFound<CourseV2Dto>(this, "COURSE_NOT_FOUND", "Course not found") : ApiResponses.Ok(this, ToDto(x));
    }

    private static CourseV2Dto ToDto(UniversityCourseManagementAPI.Models.Course x) => new(x.Id, x.Name, x.Description, x.Credits, x.CreatedAt,
        new CourseTeacherV2Dto(x.TeacherId, x.Teacher?.FirstName ?? string.Empty, x.Teacher?.LastName ?? string.Empty, x.Teacher?.Department ?? string.Empty), x.Enrollments.Count);
}
