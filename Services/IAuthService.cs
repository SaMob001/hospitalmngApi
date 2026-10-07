using HospitalApi.DTOs.Auth;

namespace HospitalApi.Services;

public interface IAuthService
{
    Task<PatientAuthResponse> RegisterPatientAsync(RegisterPatientRequest request);
    Task<PatientAuthResponse> LoginPatientAsync(PatientLoginRequest request);
    Task<StaffAuthResponse> LoginDoctorAsync(StaffLoginRequest request);
    Task<StaffAuthResponse> LoginAdminAsync(StaffLoginRequest request);
}