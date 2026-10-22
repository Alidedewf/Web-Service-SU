namespace UniversityCourseManagementAPI.DTO.Courses;

public record CourseV1Dto(int Id, string Name, string? Description, int Credits, int TeacherId, DateTime CreatedAt);
public record CourseTeacherV2Dto(int Id, string FirstName, string LastName, string Department);
public record CourseV2Dto(int Id, string Name, string? Description, int Credits, DateTime CreatedAt,
    CourseTeacherV2Dto Teacher, int EnrollmentCount);
