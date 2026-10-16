using System.ComponentModel.DataAnnotations;

namespace UniversityCourseManagementAPI.Models;

public class Student
{
    public int Id { get; set; }
    [Required, MaxLength(80)] public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(80)] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(254)] public string Email { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
