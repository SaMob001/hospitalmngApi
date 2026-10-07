namespace HospitalApi.Helpers;

/// <summary>
/// Strongly-typed view of the "Jwt" configuration section.
/// Key comes from user-secrets; the rest from appsettings.json.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string Key { get; set; } = "";
    public int ExpiryMinutes { get; set; } = 60;
}