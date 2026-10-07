using HospitalApi.DTOs.Doctor;
using HospitalApi.Exceptions;
using HospitalApi.Helpers;
using HospitalApi.Models;
using HospitalApi.Repositories;

namespace HospitalApi.Services;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _repo;
    private readonly ILogger<DoctorService> _logger;

    private static readonly string[] DayNames =
    [
        "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
    ];

    public DoctorService(IDoctorRepository repo, ILogger<DoctorService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<DoctorConsultationResponse>> GetTodayConsultationsAsync(int doctorId, DateOnly? date)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var rows = await _repo.GetTodayConsultationsAsync(doctorId, targetDate);

        return rows.Select(r => new DoctorConsultationResponse
        {
            BookingId = $"APT-{r.AppointmentId:D5}",
            Time = DateTimeFormat.DisplayTime(r.AppointmentTime),
            Status = r.Status,
            Reason = r.Reason,
            PatientId = IdFormatter.Patient(r.PatientId),
            PatientName = r.PatientName,
            Phone = r.Phone,
            BillingStatus = r.BillingStatus,
            IsSettled = r.IsSettled
        });
    }


    public async Task<IEnumerable<DoctorAvailabilityResponse>> GetAvailabilityAsync(int doctorId)
    {
        var rows = await _repo.GetAvailabilityAsync(doctorId);
        return rows.Select(ToAvailabilityResponse);
    }

    public async Task<IEnumerable<DoctorAvailabilityResponse>> SetAvailabilityAsync(int doctorId, SetDoctorAvailabilityRequest request)
    {
        if (request.DayOfWeek < 1 || request.DayOfWeek > 7)
        {
            throw new BadRequestException("Day of week must be between 1 (Monday) and 7 (Sunday).");
        }

        if (request.StartTime >= request.EndTime)
        {
            throw new BadRequestException("Start time must be strictly before end time.");
        }

        if (request.SlotMinutes < 5 || request.SlotMinutes > 120)
        {
            throw new BadRequestException("Slot duration must be between 5 and 120 minutes.");
        }

        await _repo.SetAvailabilityAsync(
            doctorId,
            request.DayOfWeek,
            request.StartTime,
            request.EndTime,
            request.SlotMinutes,
            request.IsActive);

        _logger.LogInformation("Doctor {DoctorId} updated schedule for day {DayOfWeek}", doctorId, request.DayOfWeek);

        return await GetAvailabilityAsync(doctorId);
    }

    public async Task<ReportAddedResponse> AddReportAsync(int doctorId, AddReportRequest request)
    {
        var appointmentId = BookingId.Parse(request.BookingId)
            ?? throw new BadRequestException("That does not look like a valid booking id.");

        var appointment = await _repo.GetAppointmentForReportAsync(appointmentId);

        // Security rule: if not found OR belongs to another doctor, return 404 (no enumeration)
        if (appointment is null || appointment.DoctorId != doctorId)
        {
            throw new NotFoundException("Appointment not found.");
        }

        if (appointment.Status == "cancelled")
        {
            throw new ConflictException("Cannot add a consultation report for a cancelled appointment.");
        }

        if (appointment.HasReport)
        {
            // If report already exists, seamlessly update it instead of rejecting with conflict
            return await UpdateReportAsync(doctorId, request);
        }

        if (request.FollowUpDate is { } followUp)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (followUp < today)
            {
                throw new BadRequestException("Follow-up date cannot be in the past.");
            }
        }

        var title = request.Title.Trim();
        var diagnosis = request.Diagnosis.Trim();

        // Format legacy prescription text if not explicitly provided but medicines exist
        var prescription = string.IsNullOrWhiteSpace(request.Prescription) ? null : request.Prescription.Trim();
        if (string.IsNullOrWhiteSpace(prescription) && request.Medicines is { Count: > 0 })
        {
            prescription = string.Join("\n", request.Medicines.Select(m =>
                $"• {m.MedicineName} | {m.Dosage} | {m.Duration} | {m.Timing}"));
        }

        var reportId = await _repo.AddReportAsync(
            appointmentId,
            appointment.PatientId,
            doctorId,
            title,
            diagnosis,
            prescription,
            request.FollowUpDate);

        // Save structured medicines into prescriptions table
        if (request.Medicines is { Count: > 0 })
        {
            foreach (var med in request.Medicines)
            {
                if (!string.IsNullOrWhiteSpace(med.MedicineName))
                {
                    await _repo.AddPrescriptionAsync(
                        appointmentId,
                        appointment.PatientId,
                        med.MedicineName.Trim(),
                        string.IsNullOrWhiteSpace(med.Dosage) ? null : med.Dosage.Trim(),
                        string.IsNullOrWhiteSpace(med.Duration) ? null : med.Duration.Trim(),
                        string.IsNullOrWhiteSpace(med.Timing) ? null : med.Timing.Trim(),
                        string.IsNullOrWhiteSpace(med.Instructions) ? null : med.Instructions.Trim());
                }
            }
        }

        // Generate billing invoice and itemized charges (Consultation fee + Lab tests)
        var consultType = string.IsNullOrWhiteSpace(request.ConsultationType) ? "General Consultancy" : request.ConsultationType.Trim();
        var consultFee = request.ConsultationFee >= 0 ? request.ConsultationFee : 400.00m;
        decimal labTotal = 0;
        if (request.LabTests is { Count: > 0 })
        {
            labTotal = request.LabTests.Where(t => !string.IsNullOrWhiteSpace(t.TestName)).Sum(t => Math.Max(0, t.Amount));
        }
        var totalBillAmount = consultFee + labTotal;
        var billDesc = labTotal > 0 ? $"{consultType} & Lab Diagnostics" : consultType;

        var billId = await _repo.RecordConsultationBillAsync(
            appointmentId,
            appointment.PatientId,
            billDesc,
            consultFee,
            labTotal,
            totalBillAmount);

        // Record consultation fee item
        if (consultFee > 0)
        {
            await _repo.AddBillItemAsync(billId, "consultation", consultType, consultFee);
        }

        // Record individual lab test items
        if (request.LabTests is { Count: > 0 })
        {
            foreach (var lab in request.LabTests)
            {
                if (!string.IsNullOrWhiteSpace(lab.TestName))
                {
                    await _repo.AddBillItemAsync(billId, "lab_test", lab.TestName.Trim(), Math.Max(0, lab.Amount));
                }
            }
        }

        _logger.LogInformation("Doctor {DoctorId} added report {ReportId} with {MedCount} medicines and created Bill {BillId} (Amount: {Amount}) for appointment {AppointmentId}",
            doctorId, reportId, request.Medicines?.Count ?? 0, billId, totalBillAmount, appointmentId);

        return new ReportAddedResponse
        {
            ReportId = reportId,
            BookingId = request.BookingId,
            Status = "completed",
            Title = title,
            FollowUpDate = request.FollowUpDate is { } d ? DateTimeFormat.DisplayDate(d) : null,
            BillId = billId,
            ConsultationFee = consultFee,
            LabCharges = labTotal,
            TotalAmount = totalBillAmount,
            Message = "Report recorded, appointment completed, and billing invoice generated."
        };
    }

    public async Task<ReportAddedResponse> UpdateReportAsync(int doctorId, AddReportRequest request)
    {
        var appointmentId = BookingId.Parse(request.BookingId)
            ?? throw new BadRequestException("That does not look like a valid booking id.");

        var appointment = await _repo.GetAppointmentForReportAsync(appointmentId);

        if (appointment is null || appointment.DoctorId != doctorId)
        {
            throw new NotFoundException("Appointment not found.");
        }

        if (appointment.Status == "cancelled")
        {
            throw new ConflictException("Cannot update a consultation report for a cancelled appointment.");
        }

        if (!appointment.HasReport)
        {
            return await AddReportAsync(doctorId, request);
        }

        // Check if the bill has already been settled / paid by patient or admin
        var existingBillCheck = await _repo.GetAppointmentBillAsync(appointmentId);
        if (existingBillCheck != null && existingBillCheck.Status.Equals("paid", StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException("This consultation bill has already been settled and paid. The report and billing charges cannot be modified.");
        }

        if (request.FollowUpDate is { } followUp)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (followUp < today)
            {
                throw new BadRequestException("Follow-up date cannot be in the past.");
            }
        }

        var title = request.Title.Trim();
        var diagnosis = request.Diagnosis.Trim();

        var prescription = string.IsNullOrWhiteSpace(request.Prescription) ? null : request.Prescription.Trim();
        if (string.IsNullOrWhiteSpace(prescription) && request.Medicines is { Count: > 0 })
        {
            prescription = string.Join("\n", request.Medicines.Select(m =>
                $"• {m.MedicineName} | {m.Dosage} | {m.Duration} | {m.Timing}"));
        }

        var reportId = await _repo.UpdateReportAsync(
            appointmentId,
            doctorId,
            title,
            diagnosis,
            prescription,
            request.FollowUpDate);

        // Update structured medicines: clear old and insert updated
        await _repo.DeletePrescriptionsAsync(appointmentId);
        if (request.Medicines is { Count: > 0 })
        {
            foreach (var med in request.Medicines)
            {
                if (!string.IsNullOrWhiteSpace(med.MedicineName))
                {
                    await _repo.AddPrescriptionAsync(
                        appointmentId,
                        appointment.PatientId,
                        med.MedicineName.Trim(),
                        string.IsNullOrWhiteSpace(med.Dosage) ? null : med.Dosage.Trim(),
                        string.IsNullOrWhiteSpace(med.Duration) ? null : med.Duration.Trim(),
                        string.IsNullOrWhiteSpace(med.Timing) ? null : med.Timing.Trim(),
                        string.IsNullOrWhiteSpace(med.Instructions) ? null : med.Instructions.Trim());
                }
            }
        }

        // Recalculate billing & update
        var consultType = string.IsNullOrWhiteSpace(request.ConsultationType) ? "General Consultancy" : request.ConsultationType.Trim();
        var consultFee = request.ConsultationFee >= 0 ? request.ConsultationFee : 400.00m;
        decimal labTotal = 0;
        if (request.LabTests is { Count: > 0 })
        {
            labTotal = request.LabTests.Where(t => !string.IsNullOrWhiteSpace(t.TestName)).Sum(t => Math.Max(0, t.Amount));
        }
        var totalBillAmount = consultFee + labTotal;
        var billDesc = labTotal > 0 ? $"{consultType} & Lab Diagnostics" : consultType;

        var existingBillId = await _repo.UpdateConsultationBillAsync(
            appointmentId,
            billDesc,
            consultFee,
            labTotal,
            totalBillAmount);

        int billId;
        if (existingBillId is { } vBillId && vBillId > 0)
        {
            billId = vBillId;
        }
        else
        {
            billId = await _repo.RecordConsultationBillAsync(
                appointmentId,
                appointment.PatientId,
                billDesc,
                consultFee,
                labTotal,
                totalBillAmount);
        }

        if (consultFee > 0)
        {
            await _repo.AddBillItemAsync(billId, "consultation", consultType, consultFee);
        }

        if (request.LabTests is { Count: > 0 })
        {
            foreach (var lab in request.LabTests)
            {
                if (!string.IsNullOrWhiteSpace(lab.TestName))
                {
                    await _repo.AddBillItemAsync(billId, "lab_test", lab.TestName.Trim(), Math.Max(0, lab.Amount));
                }
            }
        }

        _logger.LogInformation("Doctor {DoctorId} updated report {ReportId} and recalculated Bill {BillId} (Amount: {Amount}) for appointment {AppointmentId}",
            doctorId, reportId, billId, totalBillAmount, appointmentId);

        return new ReportAddedResponse
        {
            ReportId = reportId,
            BookingId = request.BookingId,
            Status = "completed",
            Title = title,
            FollowUpDate = request.FollowUpDate is { } d ? DateTimeFormat.DisplayDate(d) : null,
            BillId = billId,
            ConsultationFee = consultFee,
            LabCharges = labTotal,
            TotalAmount = totalBillAmount,
            Message = "Consultation report updated successfully and billing charges recalculated."
        };
    }

    public async Task<DoctorAppointmentReportResponse> GetReportForAppointmentAsync(int doctorId, string bookingId)
    {
        var appointmentId = BookingId.Parse(bookingId)
            ?? throw new BadRequestException("That does not look like a valid booking id.");

        var appointment = await _repo.GetAppointmentForReportAsync(appointmentId);

        if (appointment is null || appointment.DoctorId != doctorId)
        {
            throw new NotFoundException("Appointment not found.");
        }

        if (!appointment.HasReport)
        {
            return new DoctorAppointmentReportResponse
            {
                HasReport = false,
                BookingId = bookingId,
                PatientId = IdFormatter.Patient(appointment.PatientId)
            };
        }

        var report = await _repo.GetAppointmentReportDetailsAsync(appointmentId, doctorId);
        if (report is null)
        {
            return new DoctorAppointmentReportResponse
            {
                HasReport = false,
                BookingId = bookingId,
                PatientId = IdFormatter.Patient(appointment.PatientId)
            };
        }

        var prescriptions = await _repo.GetAppointmentPrescriptionsAsync(appointmentId);
        var bill = await _repo.GetAppointmentBillAsync(appointmentId);

        var billItems = new List<BillItemRecord>();
        if (bill != null)
        {
            billItems = (await _repo.GetBillItemsAsync(bill.BillId)).ToList();
        }

        var consultItem = billItems.FirstOrDefault(i => i.ItemType.Equals("consultation", StringComparison.OrdinalIgnoreCase));
        var consultType = consultItem?.ItemName ?? "General Consultancy";
        var consultFee = bill?.ConsultationFee ?? (consultItem?.Amount ?? 400.00m);
        var labCharges = bill?.LabCharges ?? 0.00m;
        var totalAmount = bill?.TotalAmount ?? (consultFee + labCharges);

        var labTests = billItems
            .Where(i => i.ItemType.Equals("lab_test", StringComparison.OrdinalIgnoreCase))
            .Select(i => new LabTestItemRequest
            {
                TestName = i.ItemName,
                Amount = i.Amount
            })
            .ToList();

        var medicines = prescriptions.Select(p => new PrescriptionItemRequest
        {
            MedicineName = p.MedicineName,
            Dosage = p.Dosage,
            Duration = p.Duration,
            Timing = p.Timing,
            Instructions = p.Instructions
        }).ToList();

        return new DoctorAppointmentReportResponse
        {
            HasReport = true,
            ReportId = report.ReportId,
            BookingId = bookingId,
            PatientId = IdFormatter.Patient(report.PatientId),
            PatientName = report.PatientName,
            Title = report.Title,
            Diagnosis = report.Diagnosis ?? "",
            Prescription = report.Prescription,
            FollowUpDate = report.FollowUpDate?.ToString("yyyy-MM-dd"),
            ConsultationType = consultType,
            ConsultationFee = consultFee,
            LabCharges = labCharges,
            TotalAmount = totalAmount,
            BillingStatus = bill?.Status,
            IsSettled = bill != null && bill.Status.Equals("paid", StringComparison.OrdinalIgnoreCase),
            BillId = bill?.BillId,
            Medicines = medicines,
            LabTests = labTests
        };
    }


    public async Task<IEnumerable<DoctorPatientHistoryResponse>> GetPatientHistoryAsync(int doctorId, string? search)
    {
        var rows = await _repo.GetPatientHistoryAsync(doctorId, search);
        return rows.Select(r => new DoctorPatientHistoryResponse
        {
            BookingId = $"APT-{r.AppointmentId:D5}",
            Date = DateTimeFormat.DisplayDate(r.AppointmentDate),
            PatientName = r.PatientName,
            Diagnosis = r.Diagnosis,
            Prescription = r.Prescription,
            BillingStatus = r.BillingStatus,
            IsSettled = r.IsSettled
        });
    }


    private static DoctorAvailabilityResponse ToAvailabilityResponse(DoctorAvailabilityRecord r) => new()
    {
        DayOfWeek = r.DayOfWeek,
        DayName = (r.DayOfWeek >= 1 && r.DayOfWeek <= 7) ? DayNames[r.DayOfWeek - 1] : $"Day {r.DayOfWeek}",
        StartTime = r.StartTime.ToString("HH:mm"),
        EndTime = r.EndTime.ToString("HH:mm"),
        SlotMinutes = r.SlotMinutes,
        IsActive = r.IsActive
    };
}
