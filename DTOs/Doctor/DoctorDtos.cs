using System.ComponentModel.DataAnnotations;

namespace HospitalApi.DTOs.Doctor;

/// <summary>Consultation item for today's doctor dashboard.</summary>
public class DoctorConsultationResponse
{
    public string BookingId { get; set; } = "";
    public string Time { get; set; } = "";
    public string Status { get; set; } = "";
    public string? Reason { get; set; }
    public string PatientId { get; set; } = "";
    public string PatientName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? BillingStatus { get; set; }
    public bool IsSettled { get; set; }
}


/// <summary>Doctor's availability entry for a day of the week.</summary>
public class DoctorAvailabilityResponse
{
    public int DayOfWeek { get; set; }
    public string DayName { get; set; } = "";
    public string StartTime { get; set; } = "";
    public string EndTime { get; set; } = "";
    public int SlotMinutes { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>Request to set or update working hours for a specific day.</summary>
public class SetDoctorAvailabilityRequest
{
    [Range(1, 7, ErrorMessage = "Day of week must be between 1 (Monday) and 7 (Sunday).")]
    public int DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    [Range(5, 120, ErrorMessage = "Slot duration must be between 5 and 120 minutes.")]
    public int SlotMinutes { get; set; } = 30;

    public bool IsActive { get; set; } = true;
}

/// <summary>Item for structured prescription medicines table.</summary>
public class PrescriptionItemRequest
{
    [Required(ErrorMessage = "Medicine name is required.")]
    public string MedicineName { get; set; } = "";

    public string Dosage { get; set; } = "";
    public string Duration { get; set; } = "";
    public string Timing { get; set; } = "";
    public string? Instructions { get; set; }
}

/// <summary>Item for lab test fee added during consultation.</summary>
public class LabTestItemRequest
{
    [Required(ErrorMessage = "Lab test name is required.")]
    public string TestName { get; set; } = "";

    [Range(0, 100000, ErrorMessage = "Test amount must be non-negative.")]
    public decimal Amount { get; set; }
}

/// <summary>Request to submit a diagnosis, prescription, and billing fees for an appointment.</summary>
public class AddReportRequest
{
    [Required(ErrorMessage = "Booking ID is required.")]
    public string BookingId { get; set; } = "";

    [Required(ErrorMessage = "Report title is required.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 120 characters.")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Diagnosis is required.")]
    public string Diagnosis { get; set; } = "";

    public string? Prescription { get; set; }

    public DateOnly? FollowUpDate { get; set; }

    public List<PrescriptionItemRequest>? Medicines { get; set; }

    // Billing & Diagnostic Charges
    public string? ConsultationType { get; set; } = "General Consultancy";

    [Range(0, 100000, ErrorMessage = "Consultation fee must be non-negative.")]
    public decimal ConsultationFee { get; set; } = 400.00m;

    public List<LabTestItemRequest>? LabTests { get; set; }
}

/// <summary>Response returned when a consultation report is submitted.</summary>
public class ReportAddedResponse
{
    public int ReportId { get; set; }
    public string BookingId { get; set; } = "";
    public string Status { get; set; } = "completed";
    public string Title { get; set; } = "";
    public string? FollowUpDate { get; set; }
    public int? BillId { get; set; }
    public decimal ConsultationFee { get; set; }
    public decimal LabCharges { get; set; }
    public decimal TotalAmount { get; set; }
    public string Message { get; set; } = "Report recorded, appointment completed, and billing invoice generated.";
}

/// <summary>Patient history entry for a doctor.</summary>
public class DoctorPatientHistoryResponse
{
    public string BookingId { get; set; } = "";
    public string Date { get; set; } = "";
    public string PatientName { get; set; } = "";
    public string? Diagnosis { get; set; }
    public string? Prescription { get; set; }
    public string? BillingStatus { get; set; }
    public bool IsSettled { get; set; }
}

/// <summary>Detailed report data for prefilling the doctor consultation report form.</summary>
public class DoctorAppointmentReportResponse
{
    public bool HasReport { get; set; }
    public int? ReportId { get; set; }
    public string BookingId { get; set; } = "";
    public string PatientId { get; set; } = "";
    public string PatientName { get; set; } = "";
    public string Title { get; set; } = "";
    public string Diagnosis { get; set; } = "";
    public string? Prescription { get; set; }
    public string? FollowUpDate { get; set; }
    public string ConsultationType { get; set; } = "General Consultancy";
    public decimal ConsultationFee { get; set; } = 400.00m;
    public decimal LabCharges { get; set; } = 0.00m;
    public decimal TotalAmount { get; set; } = 400.00m;
    public string? BillingStatus { get; set; }
    public bool IsSettled { get; set; }
    public int? BillId { get; set; }
    public List<PrescriptionItemRequest> Medicines { get; set; } = new();
    public List<LabTestItemRequest> LabTests { get; set; } = new();
}

