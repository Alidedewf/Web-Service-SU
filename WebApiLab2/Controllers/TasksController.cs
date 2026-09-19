using Microsoft.AspNetCore.Mvc;
using WebApiLab2.Models;

namespace WebApiLab2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private static readonly List<TaskItem> tasks = new();

    [HttpGet]
    public ActionResult<List<TaskItem>> GetAll()
    {
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public ActionResult<TaskItem> GetById(int id)
    {
        var task = tasks.FirstOrDefault(x => x.Id == id);

        if (task == null)
        {
            return NotFound();
        }

        return Ok(task);
    }

    [HttpPost]
    public ActionResult<TaskItem> Create(TaskItem task)
    {
        if (string.IsNullOrWhiteSpace(task.Title))
        {
            return BadRequest(new { message = "Title is required." });
        }

        task.Id = tasks.Count == 0 ? 1 : tasks.Max(x => x.Id) + 1;
        tasks.Add(task);

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    public ActionResult<TaskItem> Update(int id, TaskItem updatedTask)
    {
        if (string.IsNullOrWhiteSpace(updatedTask.Title))
        {
            return BadRequest(new { message = "Title is required." });
        }

        var task = tasks.FirstOrDefault(x => x.Id == id);

        if (task == null)
        {
            return NotFound();
        }

        task.Title = updatedTask.Title;
        task.Description = updatedTask.Description;
        task.IsCompleted = updatedTask.IsCompleted;

        return Ok(task);
    }

    [HttpDelete("{id}")]
    public ActionResult<TaskItem> Delete(int id)
    {
        var task = tasks.FirstOrDefault(x => x.Id == id);

        if (task == null)
        {
            return NotFound();
        }

        tasks.Remove(task);

        return Ok(task);
    }
}
