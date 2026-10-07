using HospitalApi.DTOs.Doctor;

namespace HospitalApi.Services;

public interface IDoctorService
{
    Task<IEnumerable<DoctorConsultationResponse>> GetTodayConsultationsAsync(int doctorId, DateOnly? date);
    Task<IEnumerable<DoctorAvailabilityResponse>> GetAvailabilityAsync(int doctorId);
    Task<IEnumerable<DoctorAvailabilityResponse>> SetAvailabilityAsync(int doctorId, SetDoctorAvailabilityRequest request);
    Task<ReportAddedResponse> AddReportAsync(int doctorId, AddReportRequest request);
    Task<ReportAddedResponse> UpdateReportAsync(int doctorId, AddReportRequest request);
    Task<DoctorAppointmentReportResponse> GetReportForAppointmentAsync(int doctorId, string bookingId);
    Task<IEnumerable<DoctorPatientHistoryResponse>> GetPatientHistoryAsync(int doctorId, string? search);
}
