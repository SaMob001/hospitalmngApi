using HospitalApi.Models;

namespace HospitalApi.Repositories;

public interface IAuthRepository
{
    /// <summary>Inserts a patient and returns the new patient_id.</summary>
    Task<int> RegisterPatientAsync(string fullName, string email, string phone,
        string passwordHash, DateOnly? dob, string? gender);

    /// <summary>Returns null when no patient has this phone number.</summary>
    Task<PatientLoginRecord?> GetPatientByPhoneAsync(string phone);

    /// <summary>Returns null when no doctor has this email.</summary>
    Task<DoctorLoginRecord?> GetDoctorByEmailAsync(string email);

    /// <summary>Returns null when no admin has this email.</summary>
    Task<AdminLoginRecord?> GetAdminByEmailAsync(string email);
}