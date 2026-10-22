namespace UniversityCourseManagementClient.Models;

// v1 contract: GET /api/v1/courses
public record CourseV1Dto(int Id, string Name, string? Description, int Credits, int TeacherId, DateTime CreatedAt);
// v2 contract: GET /api/v2/courses/{id} — adds teacher details and enrollment count
public record CourseTeacherV2Dto(int Id, string FirstName, string LastName, string Department);
public record CourseV2Dto(int Id, string Name, string? Description, int Credits, DateTime CreatedAt, CourseTeacherV2Dto Teacher, int EnrollmentCount);

public sealed class ApiEnvelope<T>
{
    public bool IsSuccess { get; init; }
    public T? Result { get; init; }
    public string? ErrorMessage { get; init; }
}
public sealed class CoursePageModel
{
    public IReadOnlyList<CourseV1Dto> Courses { get; init; } = [];
    public string? Error { get; init; }
}
