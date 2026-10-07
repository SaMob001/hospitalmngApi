using Dapper;
using HospitalApi.Data;
using HospitalApi.Models;

namespace HospitalApi.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly IDbConnectionFactory _db;

    public AdminRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<AdminDashboardStatsRecord> GetDashboardStatsAsync()
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryFirstAsync<AdminDashboardStatsRecord>(
            "SELECT * FROM sp_admin_dashboard_stats()");
    }

    public async Task<IEnumerable<DepartmentRecord>> GetDepartmentsAsync()
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<DepartmentRecord>(
            "SELECT department_id, name FROM sp_admin_departments()");
    }

    public async Task<IEnumerable<AdminDoctorRecord>> ListDoctorsAsync()
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<AdminDoctorRecord>(
            "SELECT * FROM sp_admin_list_doctors()");
    }

    public async Task<int> AddDoctorAsync(int departmentId, string name, string email, string phone, string passwordHash)
    {
        await using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT sp_admin_add_doctor(@DepartmentId, @Name, @Email, @Phone, @PasswordHash)",
            new { DepartmentId = departmentId, Name = name, Email = email, Phone = phone, PasswordHash = passwordHash });
    }

    public async Task UpdateDoctorAsync(int doctorId, int departmentId, string name, string phone, string status)
    {
        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT sp_admin_update_doctor(@DoctorId, @DepartmentId, @Name, @Phone, @Status::doctor_status_type)",
            new { DoctorId = doctorId, DepartmentId = departmentId, Name = name, Phone = phone, Status = status });
    }

    public async Task<IEnumerable<AdminPatientRecord>> ListPatientsAsync()
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<AdminPatientRecord>(
            "SELECT * FROM sp_admin_list_patients()");
    }

    public async Task<int> AddPatientAsync(string name, string email, string phone, string passwordHash, DateOnly? dob, string? gender)
    {
        await using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT sp_admin_add_patient(@Name, @Email, @Phone, @PasswordHash, @Dob::date, @Gender::gender_type)",
            new { Name = name, Email = email, Phone = phone, PasswordHash = passwordHash, Dob = dob, Gender = gender });
    }

    public async Task UpdatePatientAsync(int patientId, string name, string phone, string? address)
    {
        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT sp_admin_update_patient(@PatientId, @Name, @Phone, @Address)",
            new { PatientId = patientId, Name = name, Phone = phone, Address = address });
    }

    public async Task<IEnumerable<AdminBillingRecord>> GetBillingReportAsync(string? status)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<AdminBillingRecord>(
            "SELECT * FROM sp_admin_billing_report(@Status::billing_status_type)",
            new { Status = status });
    }

    public async Task<AppointmentForBillRecord?> GetAppointmentForBillAsync(int appointmentId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<AppointmentForBillRecord>(
            @"SELECT ap.appointment_id, ap.patient_id, ap.status::text,
                     EXISTS(SELECT 1 FROM billing b WHERE b.appointment_id = ap.appointment_id) AS has_bill
              FROM appointments ap
              WHERE ap.appointment_id = @AppointmentId",
            new { AppointmentId = appointmentId });
    }

    public async Task<int> CreateBillAsync(int appointmentId, int patientId, string description, decimal amount)
    {
        await using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT sp_create_bill(@AppointmentId, @PatientId, @Description, @Amount)",
            new { AppointmentId = appointmentId, PatientId = patientId, Description = description, Amount = amount });
    }

    public async Task<string?> GetBillStatusAsync(int billId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<string>(
            "SELECT status::text FROM billing WHERE bill_id = @BillId",
            new { BillId = billId });
    }

    public async Task MarkBillPaidAsync(int billId)
    {
        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT sp_mark_bill_paid(@BillId)",
            new { BillId = billId });
    }

    public async Task<BillDetailRecord?> GetBillDetailsAsync(int billId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<BillDetailRecord>(
            "SELECT * FROM sp_get_bill_details(@BillId)",
            new { BillId = billId });
    }

    public async Task<IEnumerable<BillItemRecord>> GetBillItemsAsync(int billId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<BillItemRecord>(
            "SELECT * FROM sp_get_bill_items(@BillId)",
            new { BillId = billId });
    }
}
