using HospitalApi.Models;

namespace HospitalApi.Repositories;

public interface IAmbulanceRepository
{
    Task<BookAmbulanceResult> BookAmbulanceAsync(
        int? patientId,
        string bookerName,
        string bookerPhone,
        string patientName,
        int patientAge,
        string emergencyCase,
        string pickupLocation,
        string? policeComplaintProof,
        string? criticalNotes);

    Task<IEnumerable<EmergencyBookingRecord>> GetActiveEmergencyRequestsAsync();

    Task<IEnumerable<EmergencyBookingRecord>> GetAllEmergencyRequestsAsync(string? status);

    Task<IEnumerable<EmergencyBookingRecord>> GetPatientEmergencyRequestsAsync(int patientId);

    Task<EmergencyBookingRecord?> GetEmergencyRequestByCodeAsync(string code);

    Task<EmergencyBookingRecord?> GetEmergencyRequestByIdAsync(int requestId);

    Task<IEnumerable<AmbulanceRecord>> GetAmbulancesAsync();

    Task<int> AddAmbulanceAsync(string vehicleNumber, string vehicleType, string driverName, string driverPhone, string baseStation);

    Task UpdateAmbulanceAsync(int ambulanceId, string driverName, string driverPhone, bool isAvailable, string baseStation);

    Task AssignAmbulanceToRequestAsync(int requestId, int ambulanceId);

    Task ResolveEmergencyRequestAsync(int requestId, string? resolutionNotes);
}
