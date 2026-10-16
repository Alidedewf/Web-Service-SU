using System.ComponentModel.DataAnnotations;

namespace UniversityCourseManagementAPI.Models;

public class Teacher
{
    public int Id { get; set; }
    [Required, MaxLength(80)] public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(80)] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(254)] public string Email { get; set; } = string.Empty;
    [Required, MaxLength(120)] public string Department { get; set; } = string.Empty;
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
