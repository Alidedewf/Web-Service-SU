using System.ComponentModel.DataAnnotations;

namespace UniversityCourseManagementAPI.DTO.Courses;

public record CourseDto(int Id, string Name, string? Description, int Credits, int TeacherId, string TeacherName, DateTime CreatedAt);
public class CourseCreateDto
{
    [Required, MaxLength(120)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }
    [Range(1, 30)] public int Credits { get; set; }
    [Range(1, int.MaxValue)] public int TeacherId { get; set; }
}
public class CourseUpdateDto : CourseCreateDto { }
