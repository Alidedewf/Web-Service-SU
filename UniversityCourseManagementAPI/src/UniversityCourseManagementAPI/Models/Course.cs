using System.ComponentModel.DataAnnotations;

namespace UniversityCourseManagementAPI.Models;

public class Course
{
    public int Id { get; set; }
    [Required, MaxLength(120)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }
    [Range(1, 30)] public int Credits { get; set; }
    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
