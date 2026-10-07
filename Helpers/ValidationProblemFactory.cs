using Microsoft.AspNetCore.Mvc;

namespace HospitalApi.Helpers;

/// <summary>
/// Builds the 400 response for DTO validation failures, in our standard ProblemDetails shape:
/// "detail" is ONE short friendly message; "errors" lists every field problem.
/// </summary>
public static class ValidationProblemFactory
{
    private const string GenericMessage =
        "Some of the information you entered is not valid. Please check it and try again.";

    public static IActionResult Create(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .ToDictionary(
                entry => ToCamelCase(entry.Key),
                entry => entry.Value!.Errors.Select(e => Friendly(e.ErrorMessage)).ToArray());

        var detail = errors.Values.SelectMany(messages => messages).FirstOrDefault() ?? GenericMessage;

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = detail,
            Instance = context.HttpContext.Request.Path
        };
        problem.Extensions["errors"] = errors;
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        return new ObjectResult(problem)
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentTypes = { "application/problem+json" }
        };
    }

    // Technical parser messages (bad JSON, wrong date format...) must never reach the user.
    private static string Friendly(string message) =>
        string.IsNullOrWhiteSpace(message)
        || message.Contains("JSON", StringComparison.OrdinalIgnoreCase)
        || message.Contains("Path:", StringComparison.Ordinal)
            ? GenericMessage
            : message;

    private static string ToCamelCase(string key) =>
        string.IsNullOrEmpty(key) ? key : char.ToLowerInvariant(key[0]) + key[1..];
}