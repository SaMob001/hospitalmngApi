namespace HospitalApi.Models;

public record AmbulanceRecord
{
    public int AmbulanceId { get; init; }
    public string VehicleNumber { get; init; } = string.Empty;
    public string VehicleType { get; init; } = string.Empty;
    public string DriverName { get; init; } = string.Empty;
    public string DriverPhone { get; init; } = string.Empty;
    public bool IsAvailable { get; init; }
    public string BaseStation { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record EmergencyBookingRecord
{
    public int RequestId { get; init; }
    public string RequestCode { get; init; } = string.Empty;
    public int? PatientId { get; init; }
    public string BookerName { get; init; } = string.Empty;
    public string BookerPhone { get; init; } = string.Empty;
    public string PatientName { get; init; } = string.Empty;
    public int PatientAge { get; init; }
    public string EmergencyCase { get; init; } = string.Empty;
    public string PickupLocation { get; init; } = string.Empty;
    public string? PoliceComplaintProof { get; init; }
    public string? CriticalNotes { get; init; }
    public int? AmbulanceId { get; init; }
    public string? VehicleNumber { get; init; }
    public string? DriverName { get; init; }
    public string? DriverPhone { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public string? ResolutionNotes { get; init; }
}

public record BookAmbulanceResult
{
    public int RequestId { get; init; }
    public string RequestCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
