namespace HospitalApi.DTOs.Patient;

/// <summary>Single prescribed medication item.</summary>
public class PrescriptionResponse
{
    public int PrescriptionId { get; set; }
    public string BookingId { get; set; } = "";
    public string MedicineName { get; set; } = "";
    public string Dosage { get; set; } = "";
    public string Duration { get; set; } = "";
    public string Timing { get; set; } = "";
    public string? Instructions { get; set; }
    public string DoctorName { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public string Date { get; set; } = "";
}

/// <summary>Detailed consultation and lab report item.</summary>
public class ReportResponse
{
    public int ReportId { get; set; }
    public string BookingId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Date { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public string? Diagnosis { get; set; }
    public string? Prescription { get; set; }
    public string? FollowUpDate { get; set; }
    public List<PrescriptionResponse> Medicines { get; set; } = [];
}

/// <summary>Itemized charge line for a bill.</summary>
public class PatientBillItemDto
{
    public int ItemId { get; set; }
    public string ItemType { get; set; } = "";
    public string ItemName { get; set; } = "";
    public decimal Amount { get; set; }
}

/// <summary>Matches and extends the Flutter BillItem model.</summary>
public class BillResponse
{
    public int BillId { get; set; }
    public string BookingId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Date { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public decimal ConsultationFee { get; set; }
    public decimal LabCharges { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
    public string? PaymentDate { get; set; }
    public List<PatientBillItemDto> Items { get; set; } = [];
}