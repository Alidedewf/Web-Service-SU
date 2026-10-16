using AutoMapper;
using UniversityCourseManagementAPI.DTO.Courses;
using UniversityCourseManagementAPI.DTO.Enrollments;
using UniversityCourseManagementAPI.DTO.Students;
using UniversityCourseManagementAPI.DTO.Teachers;
using UniversityCourseManagementAPI.Models;

namespace UniversityCourseManagementAPI.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Student, StudentDto>();
        CreateMap<StudentCreateDto, Student>();
        CreateMap<StudentUpdateDto, Student>();
        CreateMap<Teacher, TeacherDto>();
        CreateMap<TeacherCreateDto, Teacher>();
        CreateMap<TeacherUpdateDto, Teacher>();
        CreateMap<Course, CourseDto>().ForCtorParam("TeacherName", o => o.MapFrom(s => s.Teacher == null ? "" : s.Teacher.FirstName + " " + s.Teacher.LastName));
        CreateMap<CourseCreateDto, Course>();
        CreateMap<CourseUpdateDto, Course>();
        CreateMap<Enrollment, EnrollmentDto>()
            .ForCtorParam("StudentName", o => o.MapFrom(s => s.Student == null ? "" : s.Student.FirstName + " " + s.Student.LastName))
            .ForCtorParam("CourseName", o => o.MapFrom(s => s.Course == null ? "" : s.Course.Name));
    }
}
