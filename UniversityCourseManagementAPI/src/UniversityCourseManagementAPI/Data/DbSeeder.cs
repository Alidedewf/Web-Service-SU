using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniversityCourseManagementAPI.Models;
using UniversityCourseManagementAPI.Services;

namespace UniversityCourseManagementAPI.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, RoleManager<IdentityRole> roles,
        UserManager<ApplicationUser> users, IConfiguration configuration)
    {
        foreach (var role in Roles.All)
            if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));

        if (!await db.Teachers.AnyAsync())
        {
            var teacher = new Teacher { FirstName = "Amina", LastName = "Sadykova", Email = "amina.sadykova@example.com", Department = "Software Engineering" };
            var otherTeacher = new Teacher { FirstName = "Daniyar", LastName = "Akhmetov", Email = "daniyar.akhmetov@example.com", Department = "Information Systems" };
            var student = new Student { FirstName = "Alikhan", LastName = "Mustafayev", Email = "alikhan.mustafayev@example.com", BirthDate = new DateOnly(2003, 4, 12) };
            var otherStudent = new Student { FirstName = "Aruzhan", LastName = "Bekova", Email = "aruzhan.bekova@example.com", BirthDate = new DateOnly(2004, 9, 3) };
            db.AddRange(teacher, otherTeacher, student, otherStudent);
            await db.SaveChangesAsync();
            var webServices = new Course { Name = "Web Services", Description = "REST APIs and ASP.NET Core", Credits = 5, TeacherId = teacher.Id };
            var databases = new Course { Name = "Databases", Description = "Relational modelling and SQL", Credits = 4, TeacherId = otherTeacher.Id };
            db.Courses.AddRange(webServices, databases);
            await db.SaveChangesAsync();
            // Enrollments for security scenarios: own course of teacher1 vs foreign course, own vs foreign grades of student1.
            db.Enrollments.AddRange(
                new Enrollment { StudentId = student.Id, CourseId = webServices.Id },
                new Enrollment { StudentId = student.Id, CourseId = databases.Id },
                new Enrollment { StudentId = otherStudent.Id, CourseId = webServices.Id, Grade = 75 });
            await db.SaveChangesAsync();
        }

        // Never place the seeded admin password in source control. Supply Seed__AdminPassword locally.
        var password = configuration["Seed:AdminPassword"];
        if (!string.IsNullOrWhiteSpace(password) && await users.FindByNameAsync("admin") is null)
        {
            var admin = new ApplicationUser { UserName = "admin", Email = "admin@university.local", EmailConfirmed = true };
            var result = await users.CreateAsync(admin, password);
            if (result.Succeeded) await users.AddToRoleAsync(admin, Roles.Admin);
        }

        // Demo Teacher/Student accounts linked to existing domain records. Password comes from Seed__TestUserPassword.
        var testPassword = configuration["Seed:TestUserPassword"];
        if (string.IsNullOrWhiteSpace(testPassword)) return;
        var seededTeacher = await db.Teachers.FirstOrDefaultAsync(x => x.Email == "amina.sadykova@example.com");
        var seededStudent = await db.Students.FirstOrDefaultAsync(x => x.Email == "alikhan.mustafayev@example.com");
        if (seededTeacher is not null)
            await EnsureUserAsync(users, "teacher1", "teacher1@university.local", testPassword, Roles.Teacher, u => u.TeacherId = seededTeacher.Id);
        if (seededStudent is not null)
            await EnsureUserAsync(users, "student1", "student1@university.local", testPassword, Roles.Student, u => u.StudentId = seededStudent.Id);
    }

    private static async Task EnsureUserAsync(UserManager<ApplicationUser> users, string userName, string email,
        string password, string role, Action<ApplicationUser> link)
    {
        if (await users.FindByNameAsync(userName) is not null) return;
        var user = new ApplicationUser { UserName = userName, Email = email, EmailConfirmed = true };
        link(user);
        var result = await users.CreateAsync(user, password);
        if (result.Succeeded) await users.AddToRoleAsync(user, role);
    }
}
