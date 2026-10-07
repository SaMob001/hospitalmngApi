using HospitalApi.DTOs.Admin;

namespace HospitalApi.Services;

public interface IAdminService
{
    Task<AdminDashboardStatsResponse> GetDashboardStatsAsync();
    Task<IEnumerable<DepartmentResponse>> GetDepartmentsAsync();
    Task<IEnumerable<AdminDoctorResponse>> ListDoctorsAsync();
    Task<AdminDoctorResponse> AddDoctorAsync(CreateDoctorRequest request);
    Task<AdminDoctorResponse> UpdateDoctorAsync(int doctorId, UpdateDoctorRequest request);
    Task<IEnumerable<AdminPatientResponse>> ListPatientsAsync();
    Task<AdminPatientResponse> AddPatientAsync(AdminCreatePatientRequest request);
    Task UpdatePatientAsync(int patientId, AdminUpdatePatientRequest request);
    Task<IEnumerable<AdminBillingResponse>> GetBillingReportAsync(string? status);
    Task<BillActionResponse> CreateBillAsync(CreateBillRequest request);
    Task<BillActionResponse> MarkBillPaidAsync(int billId);
    Task<BillDetailResponse> GetBillDetailsAsync(int billId);
}
