using Microsoft.AspNetCore.Mvc;
using WebApiHomework1.Models;

namespace WebApiHomework1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private static readonly List<Student> students = new()
    {
        new Student
        {
            Id = 1,
            Name = "Ayan",
            Age = 20
        },
        new Student
        {
            Id = 2,
            Name = "Dana",
            Age = 21
        }
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
    public ActionResult<Student> AddStudent(Student student)
    {
        student.Id = students.Count + 1;
        students.Add(student);

        return Ok(student);
    }
}