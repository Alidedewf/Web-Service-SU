using Microsoft.AspNetCore.Mvc;

namespace UniversityCourseManagementAPI.Common;

public static class ApiResponses
{
    public static IActionResult Ok<T>(ControllerBase c, T? data) =>
        c.StatusCode(StatusCodes.Status200OK, ReturnResult<T>.Success(200, data, c.HttpContext.TraceIdentifier));
    public static IActionResult Created<T>(ControllerBase c, string action, object routeValues, T data) =>
        c.StatusCode(StatusCodes.Status201Created, ReturnResult<T>.Success(201, data, c.HttpContext.TraceIdentifier));
    public static IActionResult NotFound<T>(ControllerBase c, string code, string message) =>
        c.NotFound(ReturnResult<T>.Failure(404, code, message, c.HttpContext.TraceIdentifier));
    public static IActionResult BadRequest<T>(ControllerBase c, string code, string message) =>
        c.BadRequest(ReturnResult<T>.Failure(400, code, message, c.HttpContext.TraceIdentifier));
    public static IActionResult Conflict<T>(ControllerBase c, string code, string message) =>
        c.Conflict(ReturnResult<T>.Failure(409, code, message, c.HttpContext.TraceIdentifier));
}
