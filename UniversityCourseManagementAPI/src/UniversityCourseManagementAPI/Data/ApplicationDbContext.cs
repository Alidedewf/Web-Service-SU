using Microsoft.EntityFrameworkCore;
using UniversityCourseManagementAPI.Models;

namespace UniversityCourseManagementAPI.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Student>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Teacher>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Enrollment>().HasIndex(x => new { x.StudentId, x.CourseId }).IsUnique();
        modelBuilder.Entity<Course>().HasOne(x => x.Teacher).WithMany(x => x.Courses)
            .HasForeignKey(x => x.TeacherId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Enrollment>().HasOne(x => x.Student).WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Enrollment>().HasOne(x => x.Course).WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Enrollment>().Property(x => x.Grade).HasPrecision(5, 2);
    }
}
