using HospitalApi.DTOs.Patient;
using HospitalApi.Exceptions;
using HospitalApi.Helpers;
using HospitalApi.Models;
using HospitalApi.Repositories;
using Npgsql;

namespace HospitalApi.Services;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _repo;
    private readonly ILogger<PatientService> _logger;

    public PatientService(IPatientRepository repo, ILogger<PatientService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // ---------------------------------------------------------------- profile

    public async Task<PatientProfileResponse> GetProfileAsync(int patientId)
    {
        var profile = await _repo.GetProfileAsync(patientId)
            ?? throw new NotFoundException("Patient profile not found.");

        return ToResponse(profile);
    }

    public async Task<PatientProfileResponse> UpdateProfileAsync(int patientId, UpdatePatientProfileRequest request)
    {
        var phone = PhoneNumber.Normalize(request.Phone)
            ?? throw new BadRequestException("Please enter a valid mobile number.");

        var fullName = request.FullName.Trim();
        var address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        var gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender;

        if (request.Dob is { } dob)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (dob > today)
            {
                throw new BadRequestException("Date of birth cannot be in the future.");
            }
            if (dob < new DateOnly(1900, 1, 1))
            {
                throw new BadRequestException("Please enter a valid date of birth.");
            }
        }

        try
        {
            await _repo.UpdateProfileAsync(patientId, fullName, phone, address, request.Dob, gender);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException("This mobile number is already used by another account.");
        }

        _logger.LogInformation("Patient {PatientId} updated their profile", patientId);

        var updated = await _repo.GetProfileAsync(patientId)
            ?? throw new NotFoundException("Patient profile not found.");

        return ToResponse(updated);
    }

    // ------------------------------------------------------- specialists & slots

    public async Task<IEnumerable<SpecialistResponse>> GetSpecialistsAsync()
    {
        var rows = await _repo.GetSpecialistsAsync();
        return rows.Select(r => new SpecialistResponse
        {
            Id = r.Id.ToString(),
            Name = r.Name,
            DoctorName = r.DoctorName,
            Availability = r.Availability
        });
    }

    public async Task<IEnumerable<SlotResponse>> GetAvailableSlotsAsync(int doctorId, DateOnly date)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (date < today)
        {
            throw new BadRequestException("Cannot check availability for a past date.");
        }

        var slots = await _repo.GetAvailableSlotsAsync(doctorId, date);
        return slots.Select(t => new SlotResponse { Time = t.ToString("HH:mm") });
    }

    // --------------------------------------------------------------- appointments

    public async Task<AppointmentResponse> BookAppointmentAsync(int patientId, BookAppointmentRequest request)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (request.Date < today)
        {
            throw new BadRequestException("Cannot book an appointment for a past date.");
        }

        // "Doctor exists and is active" — reuse the already-verified specialists lookup
        // instead of a new query, since sp_get_specialists() already filters to active doctors.
        var specialists = await _repo.GetSpecialistsAsync();
        var specialist = specialists.FirstOrDefault(s => s.Id == request.DoctorId)
            ?? throw new BadRequestException("Please select a valid, active doctor.");

        // The requested time must be one of the doctor's real open slots for that date.
        var openSlots = await _repo.GetAvailableSlotsAsync(request.DoctorId, request.Date);
        if (!openSlots.Contains(request.Time))
        {
            throw new BadRequestException("The selected time is not available. Please choose another slot.");
        }

        var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();

        var result = await _repo.BookAppointmentAsync(patientId, request.DoctorId, request.Date, request.Time, reason);

        if (result.Result == "SLOT_TAKEN")
        {
            // Rare race: someone else booked the same slot between our check above and the insert.
            throw new ConflictException("This slot was just taken by another patient. Please choose a different time.");
        }

        _logger.LogInformation("Patient {PatientId} booked appointment {BookingId} with doctor {DoctorId}",
            patientId, result.BookingId, request.DoctorId);

        return new AppointmentResponse
        {
            BookingId = result.BookingId!,
            DepartmentName = specialist.Name,
            DoctorName = specialist.DoctorName,
            Date = DateTimeFormat.DisplayDate(request.Date),
            Time = DateTimeFormat.DisplayTime(request.Time),
            Reason = reason,
            Status = "scheduled"
        };
    }

    public async Task<IEnumerable<AppointmentResponse>> GetAppointmentsAsync(int patientId)
    {
        var rows = await _repo.GetAppointmentsAsync(patientId);
        return rows.Select(ToAppointmentResponse);
    }

    public async Task<AppointmentResponse> CancelAppointmentAsync(int patientId, string bookingId)
    {
        var appointmentId = BookingId.Parse(bookingId)
            ?? throw new BadRequestException("That does not look like a valid booking id.");

        // Ownership check: fetch only THIS patient's appointments and look for a match.
        // An id that exists but belongs to someone else is indistinguishable from "not found"
        // — we never reveal whether an id belongs to another patient.
        var appointments = await _repo.GetAppointmentsAsync(patientId);
        var appointment = appointments.FirstOrDefault(a => a.BookingId == bookingId)
            ?? throw new NotFoundException("Appointment not found.");

        if (appointment.Status != "scheduled")
        {
            throw new ConflictException($"This appointment is already {appointment.Status} and cannot be cancelled.");
        }

        await _repo.CancelAppointmentAsync(appointmentId, patientId);

        _logger.LogInformation("Patient {PatientId} cancelled appointment {BookingId}", patientId, bookingId);

        var response = ToAppointmentResponse(appointment);
        response.Status = "cancelled"; // reflect the change immediately without a second DB round trip
        return response;
    }

    // ---------------------------------------------------------------- helpers

    private static PatientProfileResponse ToResponse(PatientProfileRecord record) => new()
    {
        Id = IdFormatter.Patient(record.PatientId),
        FullName = record.FullName,
        Email = record.Email,
        MobileNumber = record.Phone,
        Dob = record.Dob,
        Gender = record.Gender,
        Address = record.Address
    };

    private static AppointmentResponse ToAppointmentResponse(AppointmentRecord record) => new()
    {
        BookingId = record.BookingId,
        DepartmentName = record.DepartmentName,
        DoctorName = record.DoctorName,
        Date = DateTimeFormat.DisplayDate(record.AppointmentDate),
        Time = DateTimeFormat.DisplayTime(record.AppointmentTime),
        Reason = record.Reason,
        Status = record.Status
    };

    public async Task<IEnumerable<ReportResponse>> GetReportsAsync(int patientId)
    {
        var rows = await _repo.GetReportsAsync(patientId);
        var reports = new List<ReportResponse>();
        foreach (var r in rows)
        {
            var meds = await _repo.GetAppointmentPrescriptionsAsync(r.AppointmentId);
            reports.Add(new ReportResponse
            {
                ReportId = r.ReportId,
                BookingId = BookingId.Format(r.AppointmentId),
                Title = r.Title,
                Date = DateTimeFormat.DisplayDate(r.CreatedAt),
                DoctorName = r.DoctorName,
                DepartmentName = r.DepartmentName,
                Diagnosis = r.Diagnosis,
                Prescription = r.Prescription,
                FollowUpDate = r.FollowUpDate is { } d ? DateTimeFormat.DisplayDate(d) : null,
                Medicines = meds.Select(m => new PrescriptionResponse
                {
                    PrescriptionId = m.PrescriptionId,
                    BookingId = BookingId.Format(r.AppointmentId),
                    MedicineName = m.MedicineName,
                    Dosage = m.Dosage,
                    Duration = m.Duration,
                    Timing = m.Timing,
                    Instructions = m.Instructions,
                    DoctorName = r.DoctorName,
                    DepartmentName = r.DepartmentName,
                    Date = DateTimeFormat.DisplayDate(r.CreatedAt)
                }).ToList()
            });
        }
        return reports;
    }

    public async Task<ReportResponse?> GetAppointmentReportAsync(int patientId, string bookingId)
    {
        var appointmentId = BookingId.Parse(bookingId)
            ?? throw new BadRequestException("That does not look like a valid booking id.");

        var r = await _repo.GetAppointmentReportAsync(appointmentId);
        if (r == null) return null;

        var meds = await _repo.GetAppointmentPrescriptionsAsync(appointmentId);
        return new ReportResponse
        {
            ReportId = r.ReportId,
            BookingId = bookingId,
            Title = r.Title,
            Date = DateTimeFormat.DisplayDate(r.CreatedAt),
            DoctorName = r.DoctorName,
            DepartmentName = r.DepartmentName,
            Diagnosis = r.Diagnosis,
            Prescription = r.Prescription,
            FollowUpDate = r.FollowUpDate is { } d ? DateTimeFormat.DisplayDate(d) : null,
            Medicines = meds.Select(m => new PrescriptionResponse
            {
                PrescriptionId = m.PrescriptionId,
                BookingId = bookingId,
                MedicineName = m.MedicineName,
                Dosage = m.Dosage,
                Duration = m.Duration,
                Timing = m.Timing,
                Instructions = m.Instructions,
                DoctorName = r.DoctorName,
                DepartmentName = r.DepartmentName,
                Date = DateTimeFormat.DisplayDate(r.CreatedAt)
            }).ToList()
        };
    }

    public async Task<IEnumerable<PrescriptionResponse>> GetPrescriptionsAsync(int patientId)
    {
        var rows = await _repo.GetPatientPrescriptionsAsync(patientId);
        return rows.Select(m => new PrescriptionResponse
        {
            PrescriptionId = m.PrescriptionId,
            BookingId = BookingId.Format(m.AppointmentId),
            MedicineName = m.MedicineName,
            Dosage = m.Dosage,
            Duration = m.Duration,
            Timing = m.Timing,
            Instructions = m.Instructions,
            DoctorName = m.DoctorName,
            DepartmentName = m.DepartmentName,
            Date = DateTimeFormat.DisplayDate(m.AppointmentDate)
        });
    }

    public async Task<IEnumerable<BillResponse>> GetBillingAsync(int patientId)
    {
        var rows = (await _repo.GetBillingAsync(patientId)).ToList();
        var result = new List<BillResponse>();

        foreach (var b in rows)
        {
            var items = await _repo.GetBillItemsAsync(b.BillId);
            result.Add(new BillResponse
            {
                BillId = b.BillId,
                BookingId = b.BookingId,
                Title = b.Title,
                Date = DateTimeFormat.DisplayDate(b.BillDate),
                DoctorName = b.DoctorName,
                DepartmentName = b.DepartmentName,
                ConsultationFee = b.ConsultationFee,
                LabCharges = b.LabCharges,
                Amount = b.Amount,
                IsPaid = b.IsPaid,
                PaymentDate = b.PaymentDate?.ToString("dd MMM yyyy HH:mm"),
                Items = items.Select(i => new PatientBillItemDto
                {
                    ItemId = i.ItemId,
                    ItemType = i.ItemType,
                    ItemName = i.ItemName,
                    Amount = i.Amount
                }).ToList()
            });
        }

        return result;
    }
}