using Microsoft.EntityFrameworkCore;
using UniversityCourseManagementAPI.Data;
using UniversityCourseManagementAPI.Models;

namespace UniversityCourseManagementAPI.Repositories;

public interface IStudentRepository
{
    Task<List<Student>> GetAllAsync(CancellationToken ct);
    Task<Student?> GetByIdAsync(int id, CancellationToken ct);
    Task<Student?> GetByEmailAsync(string email, CancellationToken ct);
    Task<Student?> GetWithCoursesAsync(int id, CancellationToken ct);
    Task AddAsync(Student entity, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
    void Delete(Student entity);
}
public interface ITeacherRepository
{
    Task<List<Teacher>> GetAllAsync(CancellationToken ct);
    Task<Teacher?> GetByIdAsync(int id, CancellationToken ct);
    Task<Teacher?> GetByEmailAsync(string email, CancellationToken ct);
    Task AddAsync(Teacher entity, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
    void Delete(Teacher entity);
}
public interface ICourseRepository
{
    Task<List<Course>> GetAllAsync(string? search, int? teacherId, int? minCredits, int? maxCredits, CancellationToken ct);
    Task<Course?> GetByIdAsync(int id, CancellationToken ct);
    Task AddAsync(Course entity, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
    void Delete(Course entity);
    Task<bool> TeacherExistsAsync(int teacherId, CancellationToken ct);
}
public interface IEnrollmentRepository
{
    Task<List<Enrollment>> GetAllAsync(CancellationToken ct);
    Task<Enrollment?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> ExistsAsync(int studentId, int courseId, CancellationToken ct);
    Task<bool> StudentExistsAsync(int studentId, CancellationToken ct);
    Task<bool> CourseExistsAsync(int courseId, CancellationToken ct);
    Task AddAsync(Enrollment entity, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
    void Delete(Enrollment entity);
}

public sealed class StudentRepository(ApplicationDbContext db) : IStudentRepository
{
    public Task<List<Student>> GetAllAsync(CancellationToken ct) => db.Students.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);
    public Task<Student?> GetByIdAsync(int id, CancellationToken ct) => db.Students.FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task<Student?> GetByEmailAsync(string email, CancellationToken ct) => db.Students.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower(), ct);
    public Task<Student?> GetWithCoursesAsync(int id, CancellationToken ct) => db.Students.AsNoTracking().Include(x => x.Enrollments).ThenInclude(x => x.Course).ThenInclude(x => x!.Teacher).FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task AddAsync(Student entity, CancellationToken ct) => db.Students.AddAsync(entity, ct).AsTask();
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
    public void Delete(Student entity) => db.Students.Remove(entity);
}
public sealed class TeacherRepository(ApplicationDbContext db) : ITeacherRepository
{
    public Task<List<Teacher>> GetAllAsync(CancellationToken ct) => db.Teachers.AsNoTracking().OrderBy(x => x.Id).ToListAsync(ct);
    public Task<Teacher?> GetByIdAsync(int id, CancellationToken ct) => db.Teachers.Include(x => x.Courses).FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task<Teacher?> GetByEmailAsync(string email, CancellationToken ct) => db.Teachers.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower(), ct);
    public Task AddAsync(Teacher entity, CancellationToken ct) => db.Teachers.AddAsync(entity, ct).AsTask();
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
    public void Delete(Teacher entity) => db.Teachers.Remove(entity);
}
public sealed class CourseRepository(ApplicationDbContext db) : ICourseRepository
{
    public Task<List<Course>> GetAllAsync(string? search, int? teacherId, int? minCredits, int? maxCredits, CancellationToken ct)
    {
        var q = db.Courses.AsNoTracking().Include(x => x.Teacher).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(x => x.Name.ToLower().Contains(search.ToLower()));
        if (teacherId.HasValue) q = q.Where(x => x.TeacherId == teacherId.Value);
        if (minCredits.HasValue) q = q.Where(x => x.Credits >= minCredits.Value);
        if (maxCredits.HasValue) q = q.Where(x => x.Credits <= maxCredits.Value);
        return q.OrderBy(x => x.Id).ToListAsync(ct);
    }
    public Task<Course?> GetByIdAsync(int id, CancellationToken ct) => db.Courses.Include(x => x.Teacher).FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task AddAsync(Course entity, CancellationToken ct) => db.Courses.AddAsync(entity, ct).AsTask();
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
    public void Delete(Course entity) => db.Courses.Remove(entity);
    public Task<bool> TeacherExistsAsync(int teacherId, CancellationToken ct) => db.Teachers.AnyAsync(x => x.Id == teacherId, ct);
}
public sealed class EnrollmentRepository(ApplicationDbContext db) : IEnrollmentRepository
{
    public Task<List<Enrollment>> GetAllAsync(CancellationToken ct) => db.Enrollments.AsNoTracking().Include(x => x.Student).Include(x => x.Course).OrderBy(x => x.Id).ToListAsync(ct);
    public Task<Enrollment?> GetByIdAsync(int id, CancellationToken ct) => db.Enrollments.Include(x => x.Student).Include(x => x.Course).FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task<bool> ExistsAsync(int studentId, int courseId, CancellationToken ct) => db.Enrollments.AnyAsync(x => x.StudentId == studentId && x.CourseId == courseId, ct);
    public Task<bool> StudentExistsAsync(int studentId, CancellationToken ct) => db.Students.AnyAsync(x => x.Id == studentId, ct);
    public Task<bool> CourseExistsAsync(int courseId, CancellationToken ct) => db.Courses.AnyAsync(x => x.Id == courseId, ct);
    public Task AddAsync(Enrollment entity, CancellationToken ct) => db.Enrollments.AddAsync(entity, ct).AsTask();
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
    public void Delete(Enrollment entity) => db.Enrollments.Remove(entity);
}
