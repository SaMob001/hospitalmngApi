namespace HospitalApi.Models;

/// <summary>Row from sp_get_patient_profile.</summary>
public class PatientProfileRecord
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateOnly? Dob { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
}