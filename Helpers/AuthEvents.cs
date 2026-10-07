using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace HospitalApi.Helpers;

/// <summary>Turns authentication failures into our standard ProblemDetails responses.</summary>
public static class AuthEvents
{
    public static JwtBearerEvents Create() => new()
    {
        // 401: no token, bad token, or expired token.
        OnChallenge = async context =>
        {
            context.HandleResponse(); // stop the default empty 401

            var expired = context.AuthenticateFailure is SecurityTokenExpiredException;
            var detail = expired
                ? "Your session has expired. Please log in again."
                : "Please log in to continue.";

            await ProblemDetailsWriter.WriteAsync(context.HttpContext, StatusCodes.Status401Unauthorized, detail);
        },

        // 403: valid token, but the wrong role for this endpoint.
        OnForbidden = context => ProblemDetailsWriter.WriteAsync(
            context.HttpContext,
            StatusCodes.Status403Forbidden,
            "You do not have permission to do this.")
    };
}