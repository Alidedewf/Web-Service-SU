namespace UniversityCourseManagementAPI.Common;

public sealed class ReturnResult<T>
{
    public int StatusCode { get; init; }
    public bool IsSuccess { get; init; }
    public T? Result { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public string? TraceId { get; init; }

    public static ReturnResult<T> Success(int statusCode, T? result, string? traceId) =>
        new() { StatusCode = statusCode, IsSuccess = true, Result = result, TraceId = traceId };

    public static ReturnResult<T> Failure(int statusCode, string errorCode, string errorMessage, string? traceId) =>
        new() { StatusCode = statusCode, IsSuccess = false, ErrorCode = errorCode, ErrorMessage = errorMessage, TraceId = traceId };
}
