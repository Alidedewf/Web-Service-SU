using System.ComponentModel.DataAnnotations;

namespace UniversityCourseManagementAPI.DTO.Enrollments;

public record EnrollmentDto(int Id, int StudentId, string StudentName, int CourseId, string CourseName, DateTime EnrollmentDate, decimal? Grade);
public class EnrollmentCreateDto
{
    [Range(1, int.MaxValue)] public int StudentId { get; set; }
    [Range(1, int.MaxValue)] public int CourseId { get; set; }
}
public class EnrollmentGradeUpdateDto
{
    [Required, Range(typeof(decimal), "0", "100")] public decimal Grade { get; set; }
}
