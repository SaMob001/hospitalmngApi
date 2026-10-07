using HospitalApi.Models;

namespace HospitalApi.Repositories;

public interface IAdminRepository
{
    Task<AdminDashboardStatsRecord> GetDashboardStatsAsync();
    Task<IEnumerable<DepartmentRecord>> GetDepartmentsAsync();
    Task<IEnumerable<AdminDoctorRecord>> ListDoctorsAsync();
    Task<int> AddDoctorAsync(int departmentId, string name, string email, string phone, string passwordHash);
    Task UpdateDoctorAsync(int doctorId, int departmentId, string name, string phone, string status);
    Task<IEnumerable<AdminPatientRecord>> ListPatientsAsync();
    Task<int> AddPatientAsync(string name, string email, string phone, string passwordHash, DateOnly? dob, string? gender);
    Task UpdatePatientAsync(int patientId, string name, string phone, string? address);
    Task<IEnumerable<AdminBillingRecord>> GetBillingReportAsync(string? status);
    Task<AppointmentForBillRecord?> GetAppointmentForBillAsync(int appointmentId);
    Task<int> CreateBillAsync(int appointmentId, int patientId, string description, decimal amount);
    Task<string?> GetBillStatusAsync(int billId);
    Task MarkBillPaidAsync(int billId);
    Task<BillDetailRecord?> GetBillDetailsAsync(int billId);
    Task<IEnumerable<BillItemRecord>> GetBillItemsAsync(int billId);
}
