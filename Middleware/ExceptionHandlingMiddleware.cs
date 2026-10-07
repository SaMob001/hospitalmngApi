using HospitalApi.Exceptions;
using HospitalApi.Helpers;
using Npgsql;

namespace HospitalApi.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected mid-request. Nothing to send back.
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        int status;
        string detail;

        switch (ex)
        {
            // Expected business errors thrown by our services.
            case AppException appEx:
                status = appEx.StatusCode;
                detail = appEx.Message;
                _logger.LogWarning("Handled {ExceptionType} on {Path}: {Message}",
                    appEx.GetType().Name, context.Request.Path, appEx.Message);
                break;

            // Database constraint violations (must come before the general Postgres cases).
            case PostgresException pg when pg.SqlState == PostgresErrorCodes.UniqueViolation:
                status = StatusCodes.Status409Conflict;
                detail = "This record already exists.";
                _logger.LogWarning("Unique violation ({Constraint}) on {Path}",
                    pg.ConstraintName, context.Request.Path);
                break;

            case PostgresException pg when pg.SqlState == PostgresErrorCodes.ForeignKeyViolation:
                status = StatusCodes.Status400BadRequest;
                detail = "The request refers to something that does not exist.";
                _logger.LogWarning("Foreign key violation ({Constraint}) on {Path}",
                    pg.ConstraintName, context.Request.Path);
                break;

            // The database understood us but the query failed (bad SQL, wrong parameter type...).
            // That is OUR bug, so it is a 500, not "unavailable".
            case PostgresException:
                status = StatusCodes.Status500InternalServerError;
                detail = "Something went wrong on our side. Please try again.";
                _logger.LogError(ex, "Database query error on {Path}", context.Request.Path);
                break;

            // We could not reach the database at all (server down, network, timeout).
            case NpgsqlException:
                status = StatusCodes.Status503ServiceUnavailable;
                detail = "The service is temporarily unavailable. Please try again shortly.";
                _logger.LogError(ex, "Database connection error on {Path}", context.Request.Path);
                break;

            // Anything unexpected: log everything, tell the client nothing technical.
            default:
                status = StatusCodes.Status500InternalServerError;
                detail = "Something went wrong on our side. Please try again.";
                _logger.LogError(ex, "Unhandled exception on {Path}", context.Request.Path);
                break;
        }

        await ProblemDetailsWriter.WriteAsync(context, status, detail);
    }
}