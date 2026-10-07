using HospitalApi.DTOs.Emergency;

namespace HospitalApi.Services;

public interface IAmbulanceService
{
    Task<EmergencyBookingResponse> BookAmbulanceAsync(int? patientId, BookAmbulanceRequest request);

    Task<IEnumerable<EmergencyBookingResponse>> GetActiveEmergencyRequestsAsync();

    Task<IEnumerable<EmergencyBookingResponse>> GetAllEmergencyRequestsAsync(string? status);

    Task<IEnumerable<EmergencyBookingResponse>> GetPatientEmergencyRequestsAsync(int patientId);

    Task<EmergencyBookingResponse?> GetEmergencyRequestByCodeAsync(string code);

    Task<IEnumerable<AmbulanceDto>> GetAmbulancesAsync();

    Task<AmbulanceDto> AddAmbulanceAsync(CreateAmbulanceRequest request);

    Task<AmbulanceDto> UpdateAmbulanceAsync(int ambulanceId, UpdateAmbulanceRequest request);

    Task<EmergencyBookingResponse> AssignAmbulanceAsync(int requestId, AssignAmbulanceRequest request);

    Task<EmergencyBookingResponse> ResolveEmergencyRequestAsync(int requestId, ResolveEmergencyRequest request);
}
