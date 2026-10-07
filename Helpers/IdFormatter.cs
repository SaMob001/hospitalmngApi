namespace HospitalApi.Helpers;

/// <summary>
/// Converts database ids to the display ids the Flutter app expects.
/// Changing a format later means changing only this file.
/// </summary>
public static class IdFormatter
{
    /// <summary>1001 becomes "PAT-1001"; 7 becomes "PAT-0007".</summary>
    public static string Patient(int patientId) => $"PAT-{patientId:D4}";
}