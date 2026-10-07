using System.Security.Cryptography;
using HospitalApi.DTOs.Admin;
using HospitalApi.Exceptions;
using HospitalApi.Helpers;
using HospitalApi.Models;
using HospitalApi.Repositories;
using Npgsql;

namespace HospitalApi.Services;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _repo;
    private readonly ILogger<AdminService> _logger;

    public AdminService(IAdminRepository repo, ILogger<AdminService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<AdminDashboardStatsResponse> GetDashboardStatsAsync()
    {
        var record = await _repo.GetDashboardStatsAsync();
        return new AdminDashboardStatsResponse
        {
            AppointmentsToday = record.AppointmentsToday,
            ActiveDoctors = record.ActiveDoctors,
            BilledThisMonth = record.BilledThisMonth ?? 0m,
            PendingReports = record.PendingReports
        };
    }

    public async Task<IEnumerable<DepartmentResponse>> GetDepartmentsAsync()
    {
        var rows = await _repo.GetDepartmentsAsync();
        return rows.Select(d => new DepartmentResponse
        {
            DepartmentId = d.DepartmentId,
            Name = d.Name
        });
    }

    public async Task<IEnumerable<AdminDoctorResponse>> ListDoctorsAsync()
    {
        var rows = await _repo.ListDoctorsAsync();
        return rows.Select(d => new AdminDoctorResponse
        {
            DoctorId = d.DoctorId,
            FullName = d.FullName,
            Email = d.Email,
            Phone = d.Phone,
            Status = d.Status,
            DepartmentName = d.DepartmentName
        });
    }

    public async Task<AdminDoctorResponse> AddDoctorAsync(CreateDoctorRequest request)
    {
        var phone = PhoneNumber.Normalize(request.Phone)
            ?? throw new BadRequestException("Please enter a valid mobile number.");

        var fullName = request.FullName.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        int doctorId;
        try
        {
            doctorId = await _repo.AddDoctorAsync(request.DepartmentId, fullName, email, phone, passwordHash);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException("A doctor with this email address or phone number is already registered.");
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new BadRequestException("Selected department does not exist.");
        }

        _logger.LogInformation("Admin registered new doctor {DoctorId} ({FullName})", doctorId, fullName);

        var doctors = await _repo.ListDoctorsAsync();
        var created = doctors.FirstOrDefault(d => d.DoctorId == doctorId)
            ?? throw new NotFoundException("Doctor record could not be retrieved.");

        return new AdminDoctorResponse
        {
            DoctorId = created.DoctorId,
            FullName = created.FullName,
            Email = created.Email,
            Phone = created.Phone,
            Status = created.Status,
            DepartmentName = created.DepartmentName
        };
    }

    public async Task<AdminDoctorResponse> UpdateDoctorAsync(int doctorId, UpdateDoctorRequest request)
    {
        var phone = PhoneNumber.Normalize(request.Phone)
            ?? throw new BadRequestException("Please enter a valid mobile number.");

        var fullName = request.FullName.Trim();
        var status = request.Status.Trim().ToLowerInvariant();

        if (status is not ("active" or "on_leave" or "inactive"))
        {
            throw new BadRequestException("Status must be 'active', 'on_leave', or 'inactive'.");
        }

        try
        {
            await _repo.UpdateDoctorAsync(doctorId, request.DepartmentId, fullName, phone, status);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException("This phone number is already used by another doctor.");
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new BadRequestException("Selected department does not exist.");
        }

        _logger.LogInformation("Admin updated doctor {DoctorId}", doctorId);

        var doctors = await _repo.ListDoctorsAsync();
        var updated = doctors.FirstOrDefault(d => d.DoctorId == doctorId)
            ?? throw new NotFoundException("Doctor not found.");

        return new AdminDoctorResponse
        {
            DoctorId = updated.DoctorId,
            FullName = updated.FullName,
            Email = updated.Email,
            Phone = updated.Phone,
            Status = updated.Status,
            DepartmentName = updated.DepartmentName
        };
    }

    public async Task<IEnumerable<AdminPatientResponse>> ListPatientsAsync()
    {
        var rows = await _repo.ListPatientsAsync();
        return rows.Select(p => new AdminPatientResponse
        {
            PatientId = IdFormatter.Patient(p.PatientId),
            FullName = p.FullName,
            Email = p.Email,
            Phone = p.Phone,
            Dob = p.Dob,
            Gender = p.Gender,
            CreatedAt = p.CreatedAt.ToString("dd MMM yyyy HH:mm")
        });
    }

    public async Task<AdminPatientResponse> AddPatientAsync(AdminCreatePatientRequest request)
    {
        var phone = PhoneNumber.Normalize(request.Phone)
            ?? throw new BadRequestException("Please enter a valid mobile number.");

        var fullName = request.FullName.Trim();
        var email = request.Email.Trim().ToLowerInvariant();
        var gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim().ToLowerInvariant();

        if (request.Dob is { } dob)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (dob > today)
            {
                throw new BadRequestException("Date of birth cannot be in the future.");
            }
        }

        var placeholderHash = BCrypt.Net.BCrypt.HashPassword(
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));

        int patientId;
        try
        {
            patientId = await _repo.AddPatientAsync(fullName, email, phone, placeholderHash, request.Dob, gender);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException("A patient with this email or mobile number already exists.");
        }

        _logger.LogInformation("Admin added walk-in patient {PatientId}", patientId);

        return new AdminPatientResponse
        {
            PatientId = IdFormatter.Patient(patientId),
            FullName = fullName,
            Email = email,
            Phone = phone,
            Dob = request.Dob,
            Gender = gender,
            CreatedAt = DateTime.UtcNow.ToString("dd MMM yyyy HH:mm")
        };
    }

    public async Task UpdatePatientAsync(int patientId, AdminUpdatePatientRequest request)
    {
        var phone = PhoneNumber.Normalize(request.Phone)
            ?? throw new BadRequestException("Please enter a valid mobile number.");

        var fullName = request.FullName.Trim();
        var address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();

        try
        {
            await _repo.UpdatePatientAsync(patientId, fullName, phone, address);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException("This mobile number is already used by another patient.");
        }

        _logger.LogInformation("Admin updated patient {PatientId}", patientId);
    }

    public async Task<IEnumerable<AdminBillingResponse>> GetBillingReportAsync(string? status)
    {
        var cleanStatus = string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToLowerInvariant();
        if (cleanStatus is not (null or "due" or "paid"))
        {
            throw new BadRequestException("Billing status filter must be 'due', 'paid', or omitted.");
        }

        var rows = await _repo.GetBillingReportAsync(cleanStatus);
        return rows.Select(b => new AdminBillingResponse
        {
            BillId = b.BillId,
            PatientName = b.PatientName,
            DoctorName = b.DoctorName,
            AppointmentDate = DateTimeFormat.DisplayDate(b.AppointmentDate),
            Description = b.Description,
            Amount = b.Amount,
            Status = b.Status,
            PaymentDate = b.PaymentDate?.ToString("dd MMM yyyy HH:mm")
        });
    }

    public async Task<BillActionResponse> CreateBillAsync(CreateBillRequest request)
    {
        var appointmentId = BookingId.Parse(request.BookingId)
            ?? throw new BadRequestException("That does not look like a valid booking id.");

        var appointment = await _repo.GetAppointmentForBillAsync(appointmentId)
            ?? throw new NotFoundException("Appointment not found.");

        if (appointment.Status == "cancelled")
        {
            throw new ConflictException("Cannot generate a bill for a cancelled appointment.");
        }

        if (appointment.HasBill)
        {
            throw new ConflictException("A bill has already been created for this appointment.");
        }

        if (request.Amount <= 0)
        {
            throw new BadRequestException("Bill amount must be strictly greater than zero.");
        }

        int billId;
        try
        {
            billId = await _repo.CreateBillAsync(appointmentId, appointment.PatientId, request.Description.Trim(), request.Amount);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException("A bill already exists for this appointment.");
        }

        _logger.LogInformation("Admin created bill {BillId} for appointment {AppointmentId}", billId, appointmentId);

        return new BillActionResponse
        {
            BillId = billId,
            Status = "due",
            Message = $"Bill {billId} created successfully."
        };
    }

    public async Task<BillActionResponse> MarkBillPaidAsync(int billId)
    {
        var status = await _repo.GetBillStatusAsync(billId)
            ?? throw new NotFoundException("Bill not found.");

        if (status == "paid")
        {
            throw new ConflictException("This bill is already marked as paid.");
        }

        await _repo.MarkBillPaidAsync(billId);
        _logger.LogInformation("Admin marked bill {BillId} as paid", billId);

        return new BillActionResponse
        {
            BillId = billId,
            Status = "paid",
            Message = $"Bill {billId} marked as paid."
        };
    }

    public async Task<BillDetailResponse> GetBillDetailsAsync(int billId)
    {
        var record = await _repo.GetBillDetailsAsync(billId)
            ?? throw new NotFoundException($"Bill #{billId} not found.");

        var items = await _repo.GetBillItemsAsync(billId);

        return new BillDetailResponse
        {
            BillId = record.BillId,
            BookingId = record.BookingId,
            PatientCode = record.PatientCode,
            PatientName = record.PatientName,
            PatientPhone = record.PatientPhone,
            DoctorName = record.DoctorName,
            DepartmentName = record.DepartmentName,
            AppointmentDate = DateTimeFormat.DisplayDate(record.AppointmentDate),
            Description = record.Description,
            ConsultationFee = record.ConsultationFee,
            LabCharges = record.LabCharges,
            TotalAmount = record.TotalAmount,
            Status = record.Status,
            PaymentDate = record.PaymentDate?.ToString("dd MMM yyyy HH:mm"),
            CreatedAt = record.CreatedAt.ToString("dd MMM yyyy HH:mm"),
            Items = items.Select(i => new BillItemDto
            {
                ItemId = i.ItemId,
                ItemType = i.ItemType,
                ItemName = i.ItemName,
                Amount = i.Amount
            }).ToList()
        };
    }
}
