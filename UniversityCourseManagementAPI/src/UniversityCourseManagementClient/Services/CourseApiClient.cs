using System.Net;
using System.Net.Http.Json;
using UniversityCourseManagementClient.Models;

namespace UniversityCourseManagementClient.Services;

public sealed class CourseApiClient(HttpClient http, ILogger<CourseApiClient> logger)
{
    public Task<(IReadOnlyList<CourseV1Dto>? Value, string? Error)> GetCoursesAsync(CancellationToken ct) =>
        GetAsync<IReadOnlyList<CourseV1Dto>>("api/v1/courses", ct);

    public Task<(CourseV2Dto? Value, string? Error)> GetCourseAsync(int id, CancellationToken ct) =>
        GetAsync<CourseV2Dto>($"api/v2/courses/{id}", ct);

    private async Task<(T? Value, string? Error)> GetAsync<T>(string url, CancellationToken ct) where T : class
    {
        try
        {
            using var response = await http.GetAsync(url, ct);
            if (response.StatusCode == HttpStatusCode.NotFound) return (null, "Course not found.");
            if (!response.IsSuccessStatusCode) return (null, $"API returned {(int)response.StatusCode} ({response.ReasonPhrase}).");
            var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<T>>(cancellationToken: ct);
            return body?.IsSuccess == true && body.Result is not null ? (body.Result, null) : (null, body?.ErrorMessage ?? "The API returned an invalid response.");
        }
        catch (HttpRequestException ex) { logger.LogWarning(ex, "University API is unavailable"); return (null, "The University API is unavailable. Check that it is running."); }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested) { return (null, "The request to the University API timed out."); }
        catch (System.Text.Json.JsonException ex) { logger.LogWarning(ex, "Invalid JSON from University API"); return (null, "The API returned an invalid response."); }
    }
}
