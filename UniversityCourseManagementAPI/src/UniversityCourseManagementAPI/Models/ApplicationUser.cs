using Microsoft.AspNetCore.Identity;

namespace UniversityCourseManagementAPI.Models;

public class ApplicationUser : IdentityUser
{
    // A role account is linked to the domain record it is allowed to represent.
    public int? StudentId { get; set; }
    public Student? Student { get; set; }
    public int? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
}
