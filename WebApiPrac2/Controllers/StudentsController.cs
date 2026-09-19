using Microsoft.AspNetCore.Mvc;
using WebApiPrac2.Models;

namespace WebApiPrac2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private static readonly List<Student> students = new()
    {
        new Student { Id = 1, Name = "Alex", Group = "SE-301" },
        new Student { Id = 2, Name = "Anna", Group = "SE-302" },
        new Student { Id = 3, Name = "Max", Group = "SE-301" }
    };

    [HttpGet]
    public ActionResult<List<Student>> GetAll()
    {
        return Ok(students);
    }

    [HttpGet("{id}")]
    public ActionResult<Student> GetById(int id)
    {
        var student = students.FirstOrDefault(x => x.Id == id);

        if (student == null)
        {
            return NotFound();
        }

        return Ok(student);
    }

    [HttpPost]
    public ActionResult<Student> Create(Student student)
    {
        if (string.IsNullOrWhiteSpace(student.Name) || string.IsNullOrWhiteSpace(student.Group))
        {
            return BadRequest(new { message = "Name and group are required." });
        }

        if (student.Id <= 0)
        {
            student.Id = students.Max(x => x.Id) + 1;
        }

        if (students.Any(x => x.Id == student.Id))
        {
            return BadRequest(new { message = "A student with this Id already exists." });
        }

        students.Add(student);

        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }
}
