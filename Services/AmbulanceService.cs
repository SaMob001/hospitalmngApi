using HospitalApi.DTOs.Emergency;
using HospitalApi.Exceptions;
using HospitalApi.Helpers;
using HospitalApi.Models;
using HospitalApi.Repositories;
using Npgsql;

namespace HospitalApi.Services;

public class AmbulanceService : IAmbulanceService
{
    private readonly IAmbulanceRepository _repo;
    private readonly ILogger<AmbulanceService> _logger;

    public AmbulanceService(IAmbulanceRepository repo, ILogger<AmbulanceService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<EmergencyBookingResponse> BookAmbulanceAsync(int? patientId, BookAmbulanceRequest request)
    {
        var bookerPhone = PhoneNumber.Normalize(request.BookerPhone)
            ?? throw new BadRequestException("Please enter a valid 10-digit mobile number for the contact person.");

        var bookerName = request.BookerName.Trim();
        var patientName = request.PatientName.Trim();
        var pickupLocation = request.PickupLocation.Trim();
        var emergencyCase = request.EmergencyCase.Trim();

        if (request.PatientAge < 0 || request.PatientAge > 130)
        {
            throw new BadRequestException("Please enter a valid patient age.");
        }

        var result = await _repo.BookAmbulanceAsync(
            patientId,
            bookerName,
            bookerPhone,
            patientName,
            request.PatientAge,
            emergencyCase,
            pickupLocation,
            request.PoliceComplaintProof?.Trim(),
            request.CriticalNotes?.Trim());

        _logger.LogWarning("🚨 EMERGENCY SOS DISPATCH TRIGGERED: {RequestCode} for {PatientName} ({EmergencyCase}) at {Location}",
            result.RequestCode, patientName, emergencyCase, pickupLocation);

        return new EmergencyBookingResponse
        {
            RequestId = result.RequestId,
            RequestCode = result.RequestCode,
            PatientId = patientId,
            BookerName = bookerName,
            BookerPhone = bookerPhone,
            PatientName = patientName,
            PatientAge = request.PatientAge,
            EmergencyCase = emergencyCase,
            PickupLocation = pickupLocation,
            PoliceComplaintProof = request.PoliceComplaintProof,
            CriticalNotes = request.CriticalNotes,
            Status = "pending",
            CreatedAt = result.CreatedAt.ToString("dd MMM yyyy HH:mm")
        };
    }

    public async Task<IEnumerable<EmergencyBookingResponse>> GetActiveEmergencyRequestsAsync()
    {
        var records = await _repo.GetActiveEmergencyRequestsAsync();
        return records.Select(ToResponse);
    }

    public async Task<IEnumerable<EmergencyBookingResponse>> GetAllEmergencyRequestsAsync(string? status)
    {
        var cleanStatus = string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToLowerInvariant();
        var records = await _repo.GetAllEmergencyRequestsAsync(cleanStatus);
        return records.Select(ToResponse);
    }

    public async Task<IEnumerable<EmergencyBookingResponse>> GetPatientEmergencyRequestsAsync(int patientId)
    {
        var records = await _repo.GetPatientEmergencyRequestsAsync(patientId);
        return records.Select(ToResponse);
    }

    public async Task<EmergencyBookingResponse?> GetEmergencyRequestByCodeAsync(string code)
    {
        var cleanCode = code.Trim().ToUpperInvariant();
        var record = await _repo.GetEmergencyRequestByCodeAsync(cleanCode);
        return record == null ? null : ToResponse(record);
    }

    public async Task<IEnumerable<AmbulanceDto>> GetAmbulancesAsync()
    {
        var records = await _repo.GetAmbulancesAsync();
        return records.Select(a => new AmbulanceDto
        {
            AmbulanceId = a.AmbulanceId,
            VehicleNumber = a.VehicleNumber,
            VehicleType = a.VehicleType,
            DriverName = a.DriverName,
            DriverPhone = a.DriverPhone,
            IsAvailable = a.IsAvailable,
            BaseStation = a.BaseStation
        });
    }

    public async Task<AmbulanceDto> AddAmbulanceAsync(CreateAmbulanceRequest request)
    {
        var phone = PhoneNumber.Normalize(request.DriverPhone)
            ?? throw new BadRequestException("Please enter a valid 10-digit mobile number for the driver.");

        var vehicleNumber = request.VehicleNumber.Trim().ToUpperInvariant();
        var vehicleType = request.VehicleType.Trim();
        var driverName = request.DriverName.Trim();
        var baseStation = string.IsNullOrWhiteSpace(request.BaseStation) ? "Alden Emergency Trauma Bay" : request.BaseStation.Trim();

        int id;
        try
        {
            id = await _repo.AddAmbulanceAsync(vehicleNumber, vehicleType, driverName, phone, baseStation);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException("An ambulance with this vehicle number is already registered.");
        }

        _logger.LogInformation("Added ambulance {VehicleNumber} (ID {Id})", vehicleNumber, id);

        return new AmbulanceDto
        {
            AmbulanceId = id,
            VehicleNumber = vehicleNumber,
            VehicleType = vehicleType,
            DriverName = driverName,
            DriverPhone = phone,
            IsAvailable = true,
            BaseStation = baseStation
        };
    }

    public async Task<AmbulanceDto> UpdateAmbulanceAsync(int ambulanceId, UpdateAmbulanceRequest request)
    {
        var phone = PhoneNumber.Normalize(request.DriverPhone)
            ?? throw new BadRequestException("Please enter a valid 10-digit mobile number for the driver.");

        var driverName = request.DriverName.Trim();
        var baseStation = request.BaseStation.Trim();

        await _repo.UpdateAmbulanceAsync(ambulanceId, driverName, phone, request.IsAvailable, baseStation);
        _logger.LogInformation("Updated ambulance {AmbulanceId}", ambulanceId);

        var ambulances = await _repo.GetAmbulancesAsync();
        var updated = ambulances.FirstOrDefault(a => a.AmbulanceId == ambulanceId)
            ?? throw new NotFoundException("Ambulance record not found.");

        return new AmbulanceDto
        {
            AmbulanceId = updated.AmbulanceId,
            VehicleNumber = updated.VehicleNumber,
            VehicleType = updated.VehicleType,
            DriverName = updated.DriverName,
            DriverPhone = updated.DriverPhone,
            IsAvailable = updated.IsAvailable,
            BaseStation = updated.BaseStation
        };
    }

    public async Task<EmergencyBookingResponse> AssignAmbulanceAsync(int requestId, AssignAmbulanceRequest request)
    {
        var current = await _repo.GetEmergencyRequestByIdAsync(requestId)
            ?? throw new NotFoundException("Emergency request not found.");

        if (current.Status == "resolved")
        {
            throw new ConflictException("This emergency case has already been resolved.");
        }

        var ambulances = await _repo.GetAmbulancesAsync();
        var ambulance = ambulances.FirstOrDefault(a => a.AmbulanceId == request.AmbulanceId)
            ?? throw new NotFoundException("Ambulance van not found.");

        await _repo.AssignAmbulanceToRequestAsync(requestId, request.AmbulanceId);
        _logger.LogInformation("Emergency request {RequestId} assigned to ambulance {VehicleNumber}", requestId, ambulance.VehicleNumber);

        var updated = await _repo.GetEmergencyRequestByIdAsync(requestId)
            ?? throw new NotFoundException("Emergency request not found.");

        return ToResponse(updated);
    }

    public async Task<EmergencyBookingResponse> ResolveEmergencyRequestAsync(int requestId, ResolveEmergencyRequest request)
    {
        var current = await _repo.GetEmergencyRequestByIdAsync(requestId)
            ?? throw new NotFoundException("Emergency request not found.");

        if (current.Status == "resolved")
        {
            throw new ConflictException("This emergency request has already been marked as resolved.");
        }

        var notes = string.IsNullOrWhiteSpace(request.ResolutionNotes) ? "Emergency case attended and closed by administration." : request.ResolutionNotes.Trim();
        await _repo.ResolveEmergencyRequestAsync(requestId, notes);

        _logger.LogInformation("Emergency request {RequestId} marked as RESOLVED", requestId);

        var updated = await _repo.GetEmergencyRequestByIdAsync(requestId)
            ?? throw new NotFoundException("Emergency request not found.");

        return ToResponse(updated);
    }

    private static EmergencyBookingResponse ToResponse(EmergencyBookingRecord r) => new()
    {
        RequestId = r.RequestId,
        RequestCode = r.RequestCode,
        PatientId = r.PatientId,
        BookerName = r.BookerName,
        BookerPhone = r.BookerPhone,
        PatientName = r.PatientName,
        PatientAge = r.PatientAge,
        EmergencyCase = r.EmergencyCase,
        PickupLocation = r.PickupLocation,
        PoliceComplaintProof = r.PoliceComplaintProof,
        CriticalNotes = r.CriticalNotes,
        AmbulanceId = r.AmbulanceId,
        VehicleNumber = r.VehicleNumber,
        DriverName = r.DriverName,
        DriverPhone = r.DriverPhone,
        Status = r.Status,
        CreatedAt = r.CreatedAt.ToString("dd MMM yyyy HH:mm"),
        ResolvedAt = r.ResolvedAt?.ToString("dd MMM yyyy HH:mm"),
        ResolutionNotes = r.ResolutionNotes
    };
}
