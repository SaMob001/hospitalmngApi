namespace HospitalApi.Helpers;

/// <summary>Names of the authorization policies (one per role).</summary>
public static class Policies
{
    public const string PatientOnly = "PatientOnly";
    public const string DoctorOnly = "DoctorOnly";
    public const string AdminOnly = "AdminOnly";
}