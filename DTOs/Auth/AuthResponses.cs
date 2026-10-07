namespace HospitalApi.DTOs.Auth;

// ---- Patient: matches the JSON the Flutter app expects exactly ----
// { token, user: { id: "PAT-1001", name, mobileNumber, hospitalName } }

public class PatientUserDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string MobileNumber { get; set; } = "";
    public string HospitalName { get; set; } = "";
}

public class PatientAuthResponse
{
    public string Token { get; set; } = "";
    public PatientUserDto User { get; set; } = new();
}

// ---- Doctor and admin (web apps) ----

public class StaffUserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
}

public class StaffAuthResponse
{
    public string Token { get; set; } = "";
    public StaffUserDto User { get; set; } = new();
}