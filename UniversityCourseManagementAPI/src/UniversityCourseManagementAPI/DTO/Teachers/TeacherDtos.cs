using System.ComponentModel.DataAnnotations;

namespace UniversityCourseManagementAPI.DTO.Teachers;

public record TeacherDto(int Id, string FirstName, string LastName, string Email, string Department);
public class TeacherCreateDto
{
    [Required, MaxLength(80)] public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(80)] public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(254)] public string Email { get; set; } = string.Empty;
    [Required, MaxLength(120)] public string Department { get; set; } = string.Empty;
}
public class TeacherUpdateDto : TeacherCreateDto { }
