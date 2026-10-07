namespace HospitalApi.Models;

/// <summary>Raw row returned by sp_doctor_today_consultations.</summary>
public class DoctorConsultationRecord
{
    public int AppointmentId { get; set; }
    public TimeOnly AppointmentTime { get; set; }
    public string Status { get; set; } = "";
    public string? Reason { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? BillingStatus { get; set; }
    public bool IsSettled { get; set; }
}

/// <summary>Raw row returned by sp_get_doctor_availability.</summary>
public class DoctorAvailabilityRecord
{
    public int AvailabilityId { get; set; }
    public int DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotMinutes { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>Raw row returned by sp_doctor_patient_history.</summary>
public class DoctorPatientHistoryRecord
{
    public int AppointmentId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public string PatientName { get; set; } = "";
    public string? Diagnosis { get; set; }
    public string? Prescription { get; set; }
    public string? BillingStatus { get; set; }
    public bool IsSettled { get; set; }
}


/// <summary>Helper record to verify appointment details before creating a report.</summary>
public class AppointmentForReportRecord
{
    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public string Status { get; set; } = "";
    public bool HasReport { get; set; }
}

/// <summary>Raw row from prescriptions table or functions.</summary>
public class PrescriptionRecord
{
    public int PrescriptionId { get; set; }
    public int AppointmentId { get; set; }
    public string MedicineName { get; set; } = "";
    public string Dosage { get; set; } = "";
    public string Duration { get; set; } = "";
    public string Timing { get; set; } = "";
    public string? Instructions { get; set; }
    public DateTime CreatedAt { get; set; }
    public string DoctorName { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public DateOnly AppointmentDate { get; set; }
}

/// <summary>Raw row for existing consultation report details.</summary>
public class DoctorAppointmentReportRecord
{
    public int ReportId { get; set; }
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Diagnosis { get; set; }
    public string? Prescription { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    public DateOnly CreatedAt { get; set; }
}
