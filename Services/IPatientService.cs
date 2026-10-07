using HospitalApi.DTOs.Patient;

namespace HospitalApi.Services;

public interface IPatientService
{
    Task<PatientProfileResponse> GetProfileAsync(int patientId);
    Task<PatientProfileResponse> UpdateProfileAsync(int patientId, UpdatePatientProfileRequest request);

    Task<IEnumerable<SpecialistResponse>> GetSpecialistsAsync();
    Task<IEnumerable<SlotResponse>> GetAvailableSlotsAsync(int doctorId, DateOnly date);

    Task<AppointmentResponse> BookAppointmentAsync(int patientId, BookAppointmentRequest request);
    Task<IEnumerable<AppointmentResponse>> GetAppointmentsAsync(int patientId);
    Task<AppointmentResponse> CancelAppointmentAsync(int patientId, string bookingId);
    Task<IEnumerable<ReportResponse>> GetReportsAsync(int patientId);
    Task<ReportResponse?> GetAppointmentReportAsync(int patientId, string bookingId);
    Task<IEnumerable<PrescriptionResponse>> GetPrescriptionsAsync(int patientId);
    Task<IEnumerable<BillResponse>> GetBillingAsync(int patientId);
}