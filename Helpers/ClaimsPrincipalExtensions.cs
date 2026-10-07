using System.Security.Claims;
using HospitalApi.Exceptions;

namespace HospitalApi.Helpers;

/// <summary>Reads the caller's id out of their JWT. Used by every protected controller.</summary>
public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("sub")?.Value;
        if (!int.TryParse(value, out var id))
        {
            // Should not happen for a validated token; guards against a malformed one.
            throw new UnauthorizedException("Please log in to continue.");
        }
        return id;
    }
}