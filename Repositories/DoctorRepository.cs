using Dapper;
using HospitalApi.Data;
using HospitalApi.Models;

namespace HospitalApi.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly IDbConnectionFactory _db;

    public DoctorRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<IEnumerable<DoctorConsultationRecord>> GetTodayConsultationsAsync(int doctorId, DateOnly date)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<DoctorConsultationRecord>(
            "SELECT * FROM sp_doctor_today_consultations(@DoctorId, @Date::date)",
            new { DoctorId = doctorId, Date = date });
    }

    public async Task<IEnumerable<DoctorAvailabilityRecord>> GetAvailabilityAsync(int doctorId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<DoctorAvailabilityRecord>(
            "SELECT * FROM sp_get_doctor_availability(@DoctorId)",
            new { DoctorId = doctorId });
    }

    public async Task SetAvailabilityAsync(int doctorId, int dayOfWeek, TimeOnly startTime, TimeOnly endTime, int slotMinutes, bool isActive)
    {
        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT sp_set_doctor_availability(@DoctorId, @Day, @Start::time, @End::time, @SlotMinutes, @IsActive)",
            new { DoctorId = doctorId, Day = dayOfWeek, Start = startTime, End = endTime, SlotMinutes = slotMinutes, IsActive = isActive });
    }

    public async Task<AppointmentForReportRecord?> GetAppointmentForReportAsync(int appointmentId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<AppointmentForReportRecord>(
            @"SELECT ap.appointment_id, ap.doctor_id, ap.patient_id, ap.status::text,
                     EXISTS(SELECT 1 FROM reports r WHERE r.appointment_id = ap.appointment_id) AS has_report
              FROM appointments ap
              WHERE ap.appointment_id = @AppointmentId",
            new { AppointmentId = appointmentId });
    }

    public async Task<int> AddReportAsync(int appointmentId, int patientId, int doctorId, string title, string diagnosis, string? prescription, DateOnly? followUpDate)
    {
        await using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT sp_add_report(@AppointmentId, @PatientId, @DoctorId, @Title, @Diagnosis, @Prescription, @FollowUpDate::date)",
            new
            {
                AppointmentId = appointmentId,
                PatientId = patientId,
                DoctorId = doctorId,
                Title = title,
                Diagnosis = diagnosis,
                Prescription = prescription,
                FollowUpDate = followUpDate
            });
    }

    public async Task AddPrescriptionAsync(int appointmentId, int patientId, string medicineName, string? dosage, string? duration, string? timing, string? instructions)
    {
        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT sp_add_prescription(@AppointmentId, @PatientId, @MedicineName, @Dosage, @Duration, @Timing, @Instructions)",
            new
            {
                AppointmentId = appointmentId,
                PatientId = patientId,
                MedicineName = medicineName,
                Dosage = dosage,
                Duration = duration,
                Timing = timing,
                Instructions = instructions
            });
    }

    public async Task<int> RecordConsultationBillAsync(int appointmentId, int patientId, string description, decimal consultationFee, decimal labCharges, decimal totalAmount)
    {
        await using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT sp_record_consultation_bill(@AppointmentId, @PatientId, @Description, @ConsultationFee, @LabCharges, @TotalAmount)",
            new
            {
                AppointmentId = appointmentId,
                PatientId = patientId,
                Description = description,
                ConsultationFee = consultationFee,
                LabCharges = labCharges,
                TotalAmount = totalAmount
            });
    }

    public async Task AddBillItemAsync(int billId, string itemType, string itemName, decimal amount)
    {
        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT sp_add_bill_item(@BillId, @ItemType, @ItemName, @Amount)",
            new
            {
                BillId = billId,
                ItemType = itemType,
                ItemName = itemName,
                Amount = amount
            });
    }

    public async Task<IEnumerable<DoctorPatientHistoryRecord>> GetPatientHistoryAsync(int doctorId, string? search)
    {
        await using var conn = _db.CreateConnection();
        var cleanSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        return await conn.QueryAsync<DoctorPatientHistoryRecord>(
            "SELECT * FROM sp_doctor_patient_history(@DoctorId, @Search)",
            new { DoctorId = doctorId, Search = cleanSearch });
    }

    public async Task<DoctorAppointmentReportRecord?> GetAppointmentReportDetailsAsync(int appointmentId, int doctorId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<DoctorAppointmentReportRecord>(
            @"SELECT r.report_id, r.appointment_id, r.patient_id, p.full_name AS patient_name,
                     r.title, r.diagnosis, r.prescription, r.follow_up_date, r.created_at::date AS created_at
              FROM reports r
              JOIN patients p ON p.patient_id = r.patient_id
              WHERE r.appointment_id = @AppointmentId AND r.doctor_id = @DoctorId",
            new { AppointmentId = appointmentId, DoctorId = doctorId });
    }

    public async Task<IEnumerable<PrescriptionRecord>> GetAppointmentPrescriptionsAsync(int appointmentId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<PrescriptionRecord>(
            @"SELECT prescription_id, appointment_id, medicine_name,
                     COALESCE(dosage, '') AS dosage, COALESCE(duration, '') AS duration,
                     COALESCE(timing, '') AS timing, instructions
              FROM prescriptions
              WHERE appointment_id = @AppointmentId
              ORDER BY prescription_id ASC",
            new { AppointmentId = appointmentId });
    }

    public async Task<BillDetailRecord?> GetAppointmentBillAsync(int appointmentId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<BillDetailRecord>(
            @"SELECT bill_id, appointment_id, patient_id, description,
                     consultation_fee, lab_charges, amount as total_amount, status::text,
                     payment_date, created_at
              FROM billing
              WHERE appointment_id = @AppointmentId",
            new { AppointmentId = appointmentId });
    }

    public async Task<IEnumerable<BillItemRecord>> GetBillItemsAsync(int billId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<BillItemRecord>(
            "SELECT * FROM sp_get_bill_items(@BillId)",
            new { BillId = billId });
    }

    public async Task<int> UpdateReportAsync(int appointmentId, int doctorId, string title, string diagnosis, string? prescription, DateOnly? followUpDate)
    {
        await using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT sp_update_report(@AppointmentId, @DoctorId, @Title, @Diagnosis, @Prescription, @FollowUpDate::date)",
            new
            {
                AppointmentId = appointmentId,
                DoctorId = doctorId,
                Title = title,
                Diagnosis = diagnosis,
                Prescription = prescription,
                FollowUpDate = followUpDate
            });
    }

    public async Task DeletePrescriptionsAsync(int appointmentId)
    {
        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT sp_delete_appointment_prescriptions(@AppointmentId)",
            new { AppointmentId = appointmentId });
    }

    public async Task<int?> UpdateConsultationBillAsync(int appointmentId, string description, decimal consultationFee, decimal labCharges, decimal totalAmount)
    {
        await using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int?>(
            "SELECT sp_update_consultation_bill(@AppointmentId, @Description, @ConsultationFee, @LabCharges, @TotalAmount)",
            new
            {
                AppointmentId = appointmentId,
                Description = description,
                ConsultationFee = consultationFee,
                LabCharges = labCharges,
                TotalAmount = totalAmount
            });
    }
}

