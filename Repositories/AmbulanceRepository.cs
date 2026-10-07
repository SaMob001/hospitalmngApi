using Dapper;
using HospitalApi.Data;
using HospitalApi.Models;

namespace HospitalApi.Repositories;

public class AmbulanceRepository : IAmbulanceRepository
{
    private readonly IDbConnectionFactory _db;

    public AmbulanceRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<BookAmbulanceResult> BookAmbulanceAsync(
        int? patientId,
        string bookerName,
        string bookerPhone,
        string patientName,
        int patientAge,
        string emergencyCase,
        string pickupLocation,
        string? policeComplaintProof,
        string? criticalNotes)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryFirstAsync<BookAmbulanceResult>(
            "SELECT * FROM sp_book_ambulance(@PatientId, @BookerName, @BookerPhone, @PatientName, @PatientAge, @EmergencyCase, @PickupLocation, @PoliceComplaintProof, @CriticalNotes)",
            new
            {
                PatientId = patientId,
                BookerName = bookerName,
                BookerPhone = bookerPhone,
                PatientName = patientName,
                PatientAge = patientAge,
                EmergencyCase = emergencyCase,
                PickupLocation = pickupLocation,
                PoliceComplaintProof = policeComplaintProof,
                CriticalNotes = criticalNotes
            });
    }

    public async Task<IEnumerable<EmergencyBookingRecord>> GetActiveEmergencyRequestsAsync()
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<EmergencyBookingRecord>("SELECT * FROM sp_get_active_emergency_requests()");
    }

    public async Task<IEnumerable<EmergencyBookingRecord>> GetAllEmergencyRequestsAsync(string? status)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<EmergencyBookingRecord>(
            "SELECT * FROM sp_get_all_emergency_requests(@Status)",
            new { Status = status });
    }

    public async Task<IEnumerable<EmergencyBookingRecord>> GetPatientEmergencyRequestsAsync(int patientId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<EmergencyBookingRecord>(
            "SELECT * FROM sp_get_patient_emergency_requests(@PatientId)",
            new { PatientId = patientId });
    }

    public async Task<EmergencyBookingRecord?> GetEmergencyRequestByCodeAsync(string code)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<EmergencyBookingRecord>(
            "SELECT * FROM sp_get_emergency_request_by_code(@Code)",
            new { Code = code });
    }

    public async Task<EmergencyBookingRecord?> GetEmergencyRequestByIdAsync(int requestId)
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<EmergencyBookingRecord>(
            @"SELECT b.request_id, b.request_code::TEXT, b.patient_id, b.booker_name::TEXT, b.booker_phone::TEXT,
                     b.patient_name::TEXT, b.patient_age, b.emergency_case::TEXT, b.pickup_location::TEXT,
                     b.police_complaint_proof::TEXT, b.critical_notes::TEXT,
                     b.ambulance_id, a.vehicle_number::TEXT, a.driver_name::TEXT, a.driver_phone::TEXT,
                     b.status::TEXT, b.created_at, b.resolved_at, b.resolution_notes::TEXT
              FROM ambulance_bookings b
              LEFT JOIN ambulances a ON a.ambulance_id = b.ambulance_id
              WHERE b.request_id = @RequestId",
            new { RequestId = requestId });
    }

    public async Task<IEnumerable<AmbulanceRecord>> GetAmbulancesAsync()
    {
        await using var conn = _db.CreateConnection();
        return await conn.QueryAsync<AmbulanceRecord>("SELECT * FROM sp_get_ambulances()");
    }

    public async Task<int> AddAmbulanceAsync(string vehicleNumber, string vehicleType, string driverName, string driverPhone, string baseStation)
    {
        await using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT sp_add_ambulance(@VehicleNumber, @VehicleType, @DriverName, @DriverPhone, @BaseStation)",
            new
            {
                VehicleNumber = vehicleNumber,
                VehicleType = vehicleType,
                DriverName = driverName,
                DriverPhone = driverPhone,
                BaseStation = baseStation
            });
    }

    public async Task UpdateAmbulanceAsync(int ambulanceId, string driverName, string driverPhone, bool isAvailable, string baseStation)
    {
        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT sp_update_ambulance(@AmbulanceId, @DriverName, @DriverPhone, @IsAvailable, @BaseStation)",
            new
            {
                AmbulanceId = ambulanceId,
                DriverName = driverName,
                DriverPhone = driverPhone,
                IsAvailable = isAvailable,
                BaseStation = baseStation
            });
    }

    public async Task AssignAmbulanceToRequestAsync(int requestId, int ambulanceId)
    {
        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT sp_assign_ambulance_to_request(@RequestId, @AmbulanceId)",
            new { RequestId = requestId, AmbulanceId = ambulanceId });
    }

    public async Task ResolveEmergencyRequestAsync(int requestId, string? resolutionNotes)
    {
        await using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "SELECT sp_resolve_emergency_request(@RequestId, @ResolutionNotes)",
            new { RequestId = requestId, ResolutionNotes = resolutionNotes });
    }
}
