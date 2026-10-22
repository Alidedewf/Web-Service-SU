using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UniversityCourseManagementAPI.Common;
using UniversityCourseManagementAPI.Data;
using UniversityCourseManagementAPI.Mapping;
using UniversityCourseManagementAPI.Middleware;
using UniversityCourseManagementAPI.Models;
using UniversityCourseManagementAPI.Repositories;
using UniversityCourseManagementAPI.Services;

var builder = WebApplication.CreateBuilder(args);
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
    throw new InvalidOperationException("JWT signing key must be supplied through Jwt__Key and contain at least 32 characters.");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Where(x => x.Value?.Errors.Count > 0).SelectMany(x => x.Value!.Errors.Select(e => $"{x.Key}: {(string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value" : e.ErrorMessage)}"));
        return new BadRequestObjectResult(ReturnResult<object>.Failure(400, "VALIDATION_ERROR", string.Join("; ", errors), context.HttpContext.TraceIdentifier));
    };
});
builder.Services.AddApiVersioning(options => { options.DefaultApiVersion = new ApiVersion(1.0); options.AssumeDefaultVersionWhenUnspecified = true; options.ReportApiVersions = true; })
    .AddApiExplorer(options => { options.GroupNameFormat = "'v'VVV"; options.SubstituteApiVersionInUrl = true; });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "University Course Management API", Version = "v1" });
    options.SwaggerDoc("v2", new OpenApiInfo { Title = "University Course Management API", Version = "v2" });
    options.DocInclusionPredicate((name, api) => api.GroupName is null || api.GroupName == name);
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Description = "JWT: Bearer {token}", Name = "Authorization", In = ParameterLocation.Header, Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
});
builder.Services.AddCors(options => options.AddPolicy("ClientApplications", policy => policy.WithOrigins("https://localhost:7101", "http://localhost:5101").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8; options.Password.RequireDigit = true; options.Password.RequireUppercase = true; options.Password.RequireLowercase = true; options.Password.RequireNonAlphanumeric = true;
    options.Lockout.AllowedForNewUsers = true; options.Lockout.MaxFailedAccessAttempts = 5; options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); options.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
// AddIdentity registers cookie schemes as defaults; the API must authenticate and challenge with JWT instead (otherwise 302 to /Account/Login).
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true, ValidIssuer = jwt.Issuer, ValidateAudience = true, ValidAudience = jwt.Audience, ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)), ValidateLifetime = true, ClockSkew = TimeSpan.Zero, NameClaimType = System.Security.Claims.ClaimTypes.Name, RoleClaimType = System.Security.Claims.ClaimTypes.Role };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context => { context.HandleResponse(); context.Response.StatusCode = 401; context.Response.ContentType = "application/json"; return context.Response.WriteAsJsonAsync(ReturnResult<object>.Failure(401, "UNAUTHORIZED", "Authentication is required", context.HttpContext.TraceIdentifier)); },
        OnForbidden = context => { context.Response.StatusCode = 403; context.Response.ContentType = "application/json"; return context.Response.WriteAsJsonAsync(ReturnResult<object>.Failure(403, "FORBIDDEN", "You do not have permission for this resource", context.HttpContext.TraceIdentifier)); }
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.AdminOnly, policy => policy.RequireAuthenticatedUser().RequireRole(Roles.Admin));
    options.AddPolicy(Policies.CanGrade, policy => policy.RequireAuthenticatedUser().RequireRole(Roles.Admin, Roles.Teacher));
    options.AddPolicy(Policies.CanViewEnrollment, policy => policy.RequireAuthenticatedUser().RequireRole(Roles.Admin, Roles.Student));
    options.AddPolicy(Policies.StudentOnly, policy => policy.RequireAuthenticatedUser().RequireRole(Roles.Student));
});
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IUniversityService, UniversityService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "University API v1"); options.SwaggerEndpoint("/swagger/v2/swagger.json", "University API v2"); });
app.UseHttpsRedirection();
app.UseCors("ClientApplications");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DatabaseMigrator.MigrateAsync(db, app.Logger);
    await DbSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(), scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(), builder.Configuration);
}
app.Run();
