using HospitalApi.Models;

namespace HospitalApi.Repositories;

public interface IPatientRepository
{
    Task<PatientProfileRecord?> GetProfileAsync(int patientId);
    Task UpdateProfileAsync(int patientId, string fullName, string phone, string? address, DateOnly? dob, string? gender);

    Task<IEnumerable<SpecialistRecord>> GetSpecialistsAsync();
    Task<IEnumerable<TimeOnly>> GetAvailableSlotsAsync(int doctorId, DateOnly date);

    Task<BookAppointmentResult> BookAppointmentAsync(int patientId, int doctorId, DateOnly date, TimeOnly time, string? reason);

    Task<IEnumerable<AppointmentRecord>> GetAppointmentsAsync(int patientId);
    Task CancelAppointmentAsync(int appointmentId, int patientId);
    Task<IEnumerable<ReportRecord>> GetReportsAsync(int patientId);
    Task<ReportRecord?> GetAppointmentReportAsync(int appointmentId);
    Task<IEnumerable<PrescriptionRecord>> GetPatientPrescriptionsAsync(int patientId);
    Task<IEnumerable<PrescriptionRecord>> GetAppointmentPrescriptionsAsync(int appointmentId);
    Task<IEnumerable<BillRecord>> GetBillingAsync(int patientId);
    Task<IEnumerable<BillItemRecord>> GetBillItemsAsync(int billId);
}