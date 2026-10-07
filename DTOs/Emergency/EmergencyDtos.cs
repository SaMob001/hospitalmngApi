using System.ComponentModel.DataAnnotations;

namespace HospitalApi.DTOs.Emergency;

public class BookAmbulanceRequest
{
    [Required(ErrorMessage = "Booker name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Booker name must be between 2 and 100 characters.")]
    public string BookerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact phone number is required.")]
    [Phone(ErrorMessage = "Please provide a valid phone number.")]
    public string BookerPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Patient name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Patient name must be between 2 and 100 characters.")]
    public string PatientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Patient age is required.")]
    [Range(0, 130, ErrorMessage = "Please enter a valid age between 0 and 130.")]
    public int PatientAge { get; set; }

    [Required(ErrorMessage = "Emergency case type is required.")]
    public string EmergencyCase { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pickup location is required.")]
    [StringLength(300, MinimumLength = 5, ErrorMessage = "Pickup location must be at least 5 characters.")]
    public string PickupLocation { get; set; } = string.Empty;

    /// <summary>Optional: Police complaint / FIR number or attachment reference.</summary>
    public string? PoliceComplaintProof { get; set; }

    /// <summary>Optional: Critical symptoms or triage notes.</summary>
    public string? CriticalNotes { get; set; }
}

public class EmergencyBookingResponse
{
    public int RequestId { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public int? PatientId { get; set; }
    public string BookerName { get; set; } = string.Empty;
    public string BookerPhone { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public int PatientAge { get; set; }
    public string EmergencyCase { get; set; } = string.Empty;
    public string PickupLocation { get; set; } = string.Empty;
    public string? PoliceComplaintProof { get; set; }
    public string? CriticalNotes { get; set; }
    public int? AmbulanceId { get; set; }
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public string Status { get; set; } = "pending";
    public string CreatedAt { get; set; } = string.Empty;
    public string? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
}

public class AmbulanceDto
{
    public int AmbulanceId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string DriverPhone { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string BaseStation { get; set; } = string.Empty;
}

public class CreateAmbulanceRequest
{
    [Required(ErrorMessage = "Vehicle number is required.")]
    public string VehicleNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vehicle type is required.")]
    public string VehicleType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Driver name is required.")]
    public string DriverName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Driver phone is required.")]
    public string DriverPhone { get; set; } = string.Empty;

    public string? BaseStation { get; set; }
}

public class UpdateAmbulanceRequest
{
    [Required(ErrorMessage = "Driver name is required.")]
    public string DriverName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Driver phone is required.")]
    public string DriverPhone { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;

    [Required(ErrorMessage = "Base station is required.")]
    public string BaseStation { get; set; } = string.Empty;
}

public class AssignAmbulanceRequest
{
    [Required(ErrorMessage = "Ambulance ID is required.")]
    public int AmbulanceId { get; set; }
}

public class ResolveEmergencyRequest
{
    public string? ResolutionNotes { get; set; }
}
