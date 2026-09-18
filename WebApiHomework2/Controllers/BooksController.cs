using WebApiHomework2.Models;
using Microsoft.AspNetCore.Mvc;

namespace WebApiHomework2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase {
    private static readonly List<Book> Books = new() {
        new Book {
            Id = 1,
            Title = "Война и мир",
            Author = "Лев Толстой",
            Year = 1869
        }
    };

    [HttpGet]
    public ActionResult<List<Book>> GetBooks() {
        return Ok(Books);
    }

    [HttpPost]
    public ActionResult<Book> AddBook(Book book) {
        Books.Add(book);
        return StatusCode(201, book);
    }
}