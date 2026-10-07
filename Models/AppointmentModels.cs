namespace HospitalApi.Models;

/// <summary>Row from sp_book_appointment: ('OK', 'APT-00021') or ('SLOT_TAKEN', null).</summary>
public class BookAppointmentResult
{
    public string Result { get; set; } = "";
    public string? BookingId { get; set; }
}


/// <summary>Row from sp_get_patient_appointments.</summary>
public class AppointmentRecord
{
    public string BookingId { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly AppointmentTime { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "";
}

/// <summary>Row from sp_get_patient_reports.</summary>
public class ReportRecord
{
    public int ReportId { get; set; }
    public int AppointmentId { get; set; }
    public string Title { get; set; } = "";
    public string? Diagnosis { get; set; }
    public string? Prescription { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    public DateOnly CreatedAt { get; set; }
    public string DoctorName { get; set; } = "";
    public string DepartmentName { get; set; } = "";
}

/// <summary>Row from sp_get_patient_billing.</summary>
public class BillRecord
{
    public int BillId { get; set; }
    public string BookingId { get; set; } = "";
    public string Title { get; set; } = "";
    public DateOnly BillDate { get; set; }
    public string DoctorName { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public decimal ConsultationFee { get; set; }
    public decimal LabCharges { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
}

/// <summary>Row from sp_get_bill_items.</summary>
public class BillItemRecord
{
    public int ItemId { get; set; }
    public string ItemType { get; set; } = "";
    public string ItemName { get; set; } = "";
    public decimal Amount { get; set; }
}

/// <summary>Row from sp_get_bill_details.</summary>
public class BillDetailRecord
{
    public int BillId { get; set; }
    public int AppointmentId { get; set; }
    public string BookingId { get; set; } = "";
    public int PatientId { get; set; }
    public string PatientCode { get; set; } = "";
    public string PatientName { get; set; } = "";
    public string PatientPhone { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public DateOnly AppointmentDate { get; set; }
    public string Description { get; set; } = "";
    public decimal ConsultationFee { get; set; }
    public decimal LabCharges { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "";
    public DateTime? PaymentDate { get; set; }
    public DateTime CreatedAt { get; set; }
}