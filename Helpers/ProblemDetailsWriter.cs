using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace HospitalApi.Helpers;

/// <summary>Writes the standard error response (RFC 7807 ProblemDetails) used everywhere in the API.</summary>
public static class ProblemDetailsWriter
{
    public static async Task WriteAsync(HttpContext context, int status, string detail)
    {
        if (context.Response.HasStarted)
        {
            return; // too late to change the response
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = ReasonPhrases.GetReasonPhrase(status),
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json");
    }
}