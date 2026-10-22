using Microsoft.AspNetCore.Mvc;
using UniversityCourseManagementClient.Models;
using UniversityCourseManagementClient.Services;

namespace UniversityCourseManagementClient.Controllers;

public class CoursesController(CourseApiClient courses) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var (items, error) = await courses.GetCoursesAsync(ct);
        return View(new CoursePageModel { Courses = items ?? [], Error = error });
    }
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var (course, error) = await courses.GetCourseAsync(id, ct);
        if (course is null) return View("Error", error);
        return View(course);
    }
}
