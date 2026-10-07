using HospitalApi.Models;

namespace HospitalApi.Repositories;

public interface IDoctorRepository
{
    Task<IEnumerable<DoctorConsultationRecord>> GetTodayConsultationsAsync(int doctorId, DateOnly date);
    Task<IEnumerable<DoctorAvailabilityRecord>> GetAvailabilityAsync(int doctorId);
    Task SetAvailabilityAsync(int doctorId, int dayOfWeek, TimeOnly startTime, TimeOnly endTime, int slotMinutes, bool isActive);
    Task<AppointmentForReportRecord?> GetAppointmentForReportAsync(int appointmentId);
    Task<int> AddReportAsync(int appointmentId, int patientId, int doctorId, string title, string diagnosis, string? prescription, DateOnly? followUpDate);
    Task AddPrescriptionAsync(int appointmentId, int patientId, string medicineName, string? dosage, string? duration, string? timing, string? instructions);
    Task<int> RecordConsultationBillAsync(int appointmentId, int patientId, string description, decimal consultationFee, decimal labCharges, decimal totalAmount);
    Task AddBillItemAsync(int billId, string itemType, string itemName, decimal amount);
    Task<IEnumerable<DoctorPatientHistoryRecord>> GetPatientHistoryAsync(int doctorId, string? search);

    Task<DoctorAppointmentReportRecord?> GetAppointmentReportDetailsAsync(int appointmentId, int doctorId);
    Task<IEnumerable<PrescriptionRecord>> GetAppointmentPrescriptionsAsync(int appointmentId);
    Task<BillDetailRecord?> GetAppointmentBillAsync(int appointmentId);
    Task<IEnumerable<BillItemRecord>> GetBillItemsAsync(int billId);
    Task<int> UpdateReportAsync(int appointmentId, int doctorId, string title, string diagnosis, string? prescription, DateOnly? followUpDate);
    Task DeletePrescriptionsAsync(int appointmentId);
    Task<int?> UpdateConsultationBillAsync(int appointmentId, string description, decimal consultationFee, decimal labCharges, decimal totalAmount);
}
