using System.Text.Json;
using UniversityCourseManagementAPI.Common;

namespace UniversityCourseManagementAPI.Middleware;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            var response = ReturnResult<object>.Failure(500, "INTERNAL_SERVER_ERROR",
                "An unexpected error occurred", context.TraceIdentifier);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        }
    }
}
