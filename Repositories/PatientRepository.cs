using Dapper;
using HospitalApi.Data;
using HospitalApi.Models;

namespace HospitalApi.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly IDbConnectionFactory _db;

    public PatientRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<PatientProfileRecord?> GetProfileAsync(int patientId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<PatientProfileRecord>(
            "SELECT * FROM sp_get_patient_profile(@PatientId)",
            new { PatientId = patientId });
    }

    public async Task UpdateProfileAsync(int patientId, string fullName, string phone, string? address,
        DateOnly? dob, string? gender)
    {
        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT sp_update_patient_profile(@PatientId, @FullName, @Phone, @Address, @Dob::date, @Gender::gender_type)",
            new { PatientId = patientId, FullName = fullName, Phone = phone, Address = address, Dob = dob, Gender = gender });
    }

    public async Task<IEnumerable<SpecialistRecord>> GetSpecialistsAsync()
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<SpecialistRecord>(
            "SELECT id, name, doctor_name, availability FROM sp_get_specialists()");
    }

    public async Task<IEnumerable<TimeOnly>> GetAvailableSlotsAsync(int doctorId, DateOnly date)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<TimeOnly>(
            "SELECT slot_time FROM sp_get_available_slots(@DoctorId, @Date::date)",
            new { DoctorId = doctorId, Date = date });
    }

        public async Task<BookAppointmentResult> BookAppointmentAsync(int patientId, int doctorId, DateOnly date, TimeOnly time, string? reason)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryFirstAsync<BookAppointmentResult>(
            "SELECT result, booking_id FROM sp_book_appointment(@PatientId, @DoctorId, @Date::date, @Time::time, @Reason)",
            new { PatientId = patientId, DoctorId = doctorId, Date = date, Time = time, Reason = reason });
    }

        public async Task<IEnumerable<AppointmentRecord>> GetAppointmentsAsync(int patientId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<AppointmentRecord>(
            "SELECT * FROM sp_get_patient_appointments(@PatientId)",
            new { PatientId = patientId });
    }

    public async Task CancelAppointmentAsync(int appointmentId, int patientId)
    {
        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT sp_cancel_appointment(@AppointmentId, @PatientId)",
            new { AppointmentId = appointmentId, PatientId = patientId });
    }
    public async Task<IEnumerable<ReportRecord>> GetReportsAsync(int patientId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<ReportRecord>(
            "SELECT * FROM sp_get_patient_reports(@PatientId)",
            new { PatientId = patientId });
    }

    public async Task<ReportRecord?> GetAppointmentReportAsync(int appointmentId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<ReportRecord>(
            "SELECT * FROM sp_get_appointment_report(@AppointmentId)",
            new { AppointmentId = appointmentId });
    }

    public async Task<IEnumerable<PrescriptionRecord>> GetPatientPrescriptionsAsync(int patientId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<PrescriptionRecord>(
            "SELECT * FROM sp_get_patient_prescriptions(@PatientId)",
            new { PatientId = patientId });
    }

    public async Task<IEnumerable<PrescriptionRecord>> GetAppointmentPrescriptionsAsync(int appointmentId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<PrescriptionRecord>(
            "SELECT * FROM sp_get_appointment_prescriptions(@AppointmentId)",
            new { AppointmentId = appointmentId });
    }

    public async Task<IEnumerable<BillRecord>> GetBillingAsync(int patientId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<BillRecord>(
            "SELECT * FROM sp_get_patient_billing(@PatientId)",
            new { PatientId = patientId });
    }

    public async Task<IEnumerable<BillItemRecord>> GetBillItemsAsync(int billId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<BillItemRecord>(
            "SELECT * FROM sp_get_bill_items(@BillId)",
            new { BillId = billId });
    }
}