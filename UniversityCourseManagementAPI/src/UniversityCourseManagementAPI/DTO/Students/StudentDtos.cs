using System.ComponentModel.DataAnnotations;

namespace UniversityCourseManagementAPI.DTO.Students;

public record StudentDto(int Id, string FirstName, string LastName, string Email, DateOnly BirthDate, DateTime CreatedAt);
public class StudentCreateDto
{
    [Required, MaxLength(80)] public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(80)] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(254)] public string Email { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
}
public class StudentUpdateDto
{
    [Required, MaxLength(80)] public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(80)] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(254)] public string Email { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
}
public record StudentCourseDto(int StudentId, string FirstName, string LastName, IReadOnlyList<StudentEnrollmentDto> Courses);
public record StudentEnrollmentDto(int EnrollmentId, int CourseId, string CourseName, string TeacherName, DateTime EnrollmentDate, decimal? Grade);
