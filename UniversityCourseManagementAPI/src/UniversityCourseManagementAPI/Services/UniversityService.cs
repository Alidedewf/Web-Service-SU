using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UniversityCourseManagementAPI.DTO.Courses;
using UniversityCourseManagementAPI.DTO.Enrollments;
using UniversityCourseManagementAPI.DTO.Students;
using UniversityCourseManagementAPI.DTO.Teachers;
using UniversityCourseManagementAPI.Models;
using UniversityCourseManagementAPI.Repositories;

namespace UniversityCourseManagementAPI.Services;

public interface IUniversityService
{
    Task<IReadOnlyList<StudentDto>> Students(CancellationToken ct);
    Task<StudentDto?> Student(int id, CancellationToken ct);
    Task<StudentDto?> CreateStudent(StudentCreateDto dto, CancellationToken ct);
    Task<(string? Error, StudentDto? Value)> UpdateStudent(int id, StudentUpdateDto dto, CancellationToken ct);
    Task<bool> DeleteStudent(int id, CancellationToken ct);
    Task<StudentCourseDto?> StudentCourses(int id, CancellationToken ct);
    Task<IReadOnlyList<TeacherDto>> Teachers(CancellationToken ct);
    Task<TeacherDto?> Teacher(int id, CancellationToken ct);
    Task<(string? Error, TeacherDto? Value)> CreateTeacher(TeacherCreateDto dto, CancellationToken ct);
    Task<(string? Error, TeacherDto? Value)> UpdateTeacher(int id, TeacherUpdateDto dto, CancellationToken ct);
    Task<string?> DeleteTeacher(int id, CancellationToken ct);
    Task<IReadOnlyList<CourseDto>> Courses(string? search, int? teacherId, int? minCredits, int? maxCredits, CancellationToken ct);
    Task<CourseDto?> Course(int id, CancellationToken ct);
    Task<(string? Error, CourseDto? Value)> CreateCourse(CourseCreateDto dto, CancellationToken ct);
    Task<(string? Error, CourseDto? Value)> UpdateCourse(int id, CourseUpdateDto dto, CancellationToken ct);
    Task<bool> DeleteCourse(int id, CancellationToken ct);
    Task<IReadOnlyList<EnrollmentDto>> Enrollments(CancellationToken ct);
    Task<EnrollmentDto?> Enrollment(int id, CancellationToken ct);
    Task<(string? Error, EnrollmentDto? Value)> CreateEnrollment(EnrollmentCreateDto dto, CancellationToken ct);
    Task<(string? Error, EnrollmentDto? Value)> UpdateGrade(int id, EnrollmentGradeUpdateDto dto, CancellationToken ct);
    Task<bool> DeleteEnrollment(int id, CancellationToken ct);
}

public sealed class UniversityService(IStudentRepository students, ITeacherRepository teachers,
    ICourseRepository courses, IEnrollmentRepository enrollments, IMapper mapper,
    ILogger<UniversityService> logger) : IUniversityService
{
    public async Task<IReadOnlyList<StudentDto>> Students(CancellationToken ct) => mapper.Map<List<StudentDto>>(await students.GetAllAsync(ct));
    public async Task<StudentDto?> Student(int id, CancellationToken ct) => mapper.Map<StudentDto?>(await students.GetByIdAsync(id, ct));
    public async Task<StudentDto?> CreateStudent(StudentCreateDto dto, CancellationToken ct)
    {
        if (await students.GetByEmailAsync(dto.Email, ct) is not null) return null;
        var entity = mapper.Map<Student>(dto); entity.CreatedAt = DateTime.UtcNow;
        await students.AddAsync(entity, ct); await students.SaveAsync(ct);
        logger.LogInformation("Created student {StudentId}", entity.Id);
        return mapper.Map<StudentDto>(entity);
    }
    public async Task<(string? Error, StudentDto? Value)> UpdateStudent(int id, StudentUpdateDto dto, CancellationToken ct)
    {
        var entity = await students.GetByIdAsync(id, ct); if (entity is null) return ("NOT_FOUND", null);
        var duplicate = await students.GetByEmailAsync(dto.Email, ct);
        if (duplicate is not null && duplicate.Id != id) return ("EMAIL_CONFLICT", null);
        mapper.Map(dto, entity); await students.SaveAsync(ct); logger.LogInformation("Updated student {StudentId}", id);
        return (null, mapper.Map<StudentDto>(entity));
    }
    public async Task<bool> DeleteStudent(int id, CancellationToken ct)
    {
        var entity = await students.GetByIdAsync(id, ct); if (entity is null) return false;
        students.Delete(entity); await students.SaveAsync(ct); logger.LogInformation("Deleted student {StudentId}", id); return true;
    }
    public async Task<StudentCourseDto?> StudentCourses(int id, CancellationToken ct)
    {
        var s = await students.GetWithCoursesAsync(id, ct); if (s is null) return null;
        var list = s.Enrollments.Select(e => new StudentEnrollmentDto(e.Id, e.CourseId, e.Course?.Name ?? "",
            e.Course?.Teacher is null ? "" : e.Course.Teacher.FirstName + " " + e.Course.Teacher.LastName, e.EnrollmentDate, e.Grade)).ToList();
        return new StudentCourseDto(s.Id, s.FirstName, s.LastName, list);
    }
    public async Task<IReadOnlyList<TeacherDto>> Teachers(CancellationToken ct) => mapper.Map<List<TeacherDto>>(await teachers.GetAllAsync(ct));
    public async Task<TeacherDto?> Teacher(int id, CancellationToken ct) => mapper.Map<TeacherDto?>(await teachers.GetByIdAsync(id, ct));
    public async Task<(string? Error, TeacherDto? Value)> CreateTeacher(TeacherCreateDto dto, CancellationToken ct)
    {
        if (await teachers.GetByEmailAsync(dto.Email, ct) is not null) return ("EMAIL_CONFLICT", null);
        var e = mapper.Map<Teacher>(dto); await teachers.AddAsync(e, ct); await teachers.SaveAsync(ct);
        logger.LogInformation("Created teacher {TeacherId}", e.Id); return (null, mapper.Map<TeacherDto>(e));
    }
    public async Task<(string? Error, TeacherDto? Value)> UpdateTeacher(int id, TeacherUpdateDto dto, CancellationToken ct)
    {
        var e = await teachers.GetByIdAsync(id, ct); if (e is null) return ("NOT_FOUND", null);
        var d = await teachers.GetByEmailAsync(dto.Email, ct); if (d is not null && d.Id != id) return ("EMAIL_CONFLICT", null);
        mapper.Map(dto, e); await teachers.SaveAsync(ct); logger.LogInformation("Updated teacher {TeacherId}", id);
        return (null, mapper.Map<TeacherDto>(e));
    }
    public async Task<string?> DeleteTeacher(int id, CancellationToken ct)
    {
        var e = await teachers.GetByIdAsync(id, ct); if (e is null) return "NOT_FOUND";
        if (e.Courses.Count > 0) return "HAS_COURSES";
        teachers.Delete(e); await teachers.SaveAsync(ct); logger.LogInformation("Deleted teacher {TeacherId}", id); return null;
    }
    public async Task<IReadOnlyList<CourseDto>> Courses(string? search, int? teacherId, int? minCredits, int? maxCredits, CancellationToken ct) =>
        mapper.Map<List<CourseDto>>(await courses.GetAllAsync(search, teacherId, minCredits, maxCredits, ct));
    public async Task<CourseDto?> Course(int id, CancellationToken ct) => mapper.Map<CourseDto?>(await courses.GetByIdAsync(id, ct));
    public async Task<(string? Error, CourseDto? Value)> CreateCourse(CourseCreateDto dto, CancellationToken ct)
    {
        if (!await courses.TeacherExistsAsync(dto.TeacherId, ct)) return ("TEACHER_NOT_FOUND", null);
        var e = mapper.Map<Course>(dto); e.CreatedAt = DateTime.UtcNow;
        await courses.AddAsync(e, ct); await courses.SaveAsync(ct); logger.LogInformation("Created course {CourseId}", e.Id);
        return (null, mapper.Map<CourseDto>(await courses.GetByIdAsync(e.Id, ct)));
    }
    public async Task<(string? Error, CourseDto? Value)> UpdateCourse(int id, CourseUpdateDto dto, CancellationToken ct)
    {
        var e = await courses.GetByIdAsync(id, ct); if (e is null) return ("NOT_FOUND", null);
        if (!await courses.TeacherExistsAsync(dto.TeacherId, ct)) return ("TEACHER_NOT_FOUND", null);
        mapper.Map(dto, e); await courses.SaveAsync(ct); logger.LogInformation("Updated course {CourseId}", id);
        return (null, mapper.Map<CourseDto>(await courses.GetByIdAsync(id, ct)));
    }
    public async Task<bool> DeleteCourse(int id, CancellationToken ct)
    {
        var e = await courses.GetByIdAsync(id, ct); if (e is null) return false;
        courses.Delete(e); await courses.SaveAsync(ct); logger.LogInformation("Deleted course {CourseId}", id); return true;
    }
    public async Task<IReadOnlyList<EnrollmentDto>> Enrollments(CancellationToken ct) => mapper.Map<List<EnrollmentDto>>(await enrollments.GetAllAsync(ct));
    public async Task<EnrollmentDto?> Enrollment(int id, CancellationToken ct) => mapper.Map<EnrollmentDto?>(await enrollments.GetByIdAsync(id, ct));
    public async Task<(string? Error, EnrollmentDto? Value)> CreateEnrollment(EnrollmentCreateDto dto, CancellationToken ct)
    {
        if (!await enrollments.StudentExistsAsync(dto.StudentId, ct)) return ("STUDENT_NOT_FOUND", null);
        if (!await enrollments.CourseExistsAsync(dto.CourseId, ct)) return ("COURSE_NOT_FOUND", null);
        if (await enrollments.ExistsAsync(dto.StudentId, dto.CourseId, ct)) return ("DUPLICATE_ENROLLMENT", null);
        var e = new Enrollment { StudentId = dto.StudentId, CourseId = dto.CourseId, EnrollmentDate = DateTime.UtcNow };
        await enrollments.AddAsync(e, ct); await enrollments.SaveAsync(ct); logger.LogInformation("Created enrollment {EnrollmentId}", e.Id);
        return (null, mapper.Map<EnrollmentDto>(await enrollments.GetByIdAsync(e.Id, ct)));
    }
    public async Task<(string? Error, EnrollmentDto? Value)> UpdateGrade(int id, EnrollmentGradeUpdateDto dto, CancellationToken ct)
    {
        var e = await enrollments.GetByIdAsync(id, ct); if (e is null) return ("NOT_FOUND", null);
        e.Grade = dto.Grade; await enrollments.SaveAsync(ct); logger.LogInformation("Updated grade for enrollment {EnrollmentId}", id);
        return (null, mapper.Map<EnrollmentDto>(await enrollments.GetByIdAsync(id, ct)));
    }
    public async Task<bool> DeleteEnrollment(int id, CancellationToken ct)
    {
        var e = await enrollments.GetByIdAsync(id, ct); if (e is null) return false;
        enrollments.Delete(e); await enrollments.SaveAsync(ct); logger.LogInformation("Deleted enrollment {EnrollmentId}", id); return true;
    }
}
