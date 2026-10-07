namespace HospitalApi.Helpers;

/// <summary>
/// The three role names used in JWT role claims and [Authorize(Roles = ...)].
/// One place to change them; no "magic strings" scattered around.
/// </summary>
public static class Roles
{
    public const string Patient = "patient";
    public const string Doctor = "doctor";
    public const string Admin = "admin";
}