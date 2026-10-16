using Microsoft.EntityFrameworkCore;
using UniversityCourseManagementAPI.Models;

namespace UniversityCourseManagementAPI.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.Teachers.AnyAsync()) return;
        var teacher = new Teacher { FirstName = "Amina", LastName = "Sadykova", Email = "amina.sadykova@example.com", Department = "Software Engineering" };
        var student = new Student { FirstName = "Alex", LastName = "Smith", Email = "alex.smith@example.com", BirthDate = new DateOnly(2003, 4, 12) };
        db.Teachers.Add(teacher);
        db.Students.Add(student);
        await db.SaveChangesAsync();
        db.Courses.Add(new Course { Name = "Web Services", Description = "REST APIs and ASP.NET Core", Credits = 5, TeacherId = teacher.Id });
        await db.SaveChangesAsync();
    }
}
