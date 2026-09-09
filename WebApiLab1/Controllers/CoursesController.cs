using Microsoft.AspNetCore.Mvc;
using WebApiLab1.Models;

namespace WebApiLab1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private static readonly List<Course> courses = new()
    {
        new Course
        {
            Id = 1,
            Name = "Web Services",
            Teacher = "Teacher 1",
            Credits = 5
        },
        new Course
        {
            Id = 2,
            Name = "Databases",
            Teacher = "Teacher 2",
            Credits = 4
        }
    };

    [HttpGet]
    public ActionResult<List<Course>> GetAll()
    {
        return Ok(courses);
    }

    [HttpGet("{id}")]
    public ActionResult<Course> GetById(int id)
    {
        var course = courses.FirstOrDefault(x => x.Id == id);

        if (course == null)
        {
            return NotFound();
        }

        return Ok(course);
    }
}