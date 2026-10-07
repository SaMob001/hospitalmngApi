namespace HospitalApi.Models;

// These classes mirror what each database function returns (snake_case columns
// map to PascalCase properties through the Dapper setup from M1).
// They stay inside the server: they can hold a password hash, so they are never sent to clients.

/// <summary>Row from sp_patient_login_lookup_by_phone. No password hash: patients log in with an OTP.</summary>
public class PatientLoginRecord
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
}

/// <summary>Row from sp_doctor_login_lookup.</summary>
public class DoctorLoginRecord
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Status { get; set; } = "";   // 'active' | 'on_leave' | 'inactive'
}

/// <summary>Row from sp_admin_login_lookup.</summary>
public class AdminLoginRecord
{
    public int AdminId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
}