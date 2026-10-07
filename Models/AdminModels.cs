namespace HospitalApi.Models;

/// <summary>Row returned by sp_admin_dashboard_stats.</summary>
public class AdminDashboardStatsRecord
{
    public long AppointmentsToday { get; set; }
    public long ActiveDoctors { get; set; }
    public decimal? BilledThisMonth { get; set; }
    public long PendingReports { get; set; }
}

/// <summary>Row returned by sp_admin_departments.</summary>
public class DepartmentRecord
{
    public int DepartmentId { get; set; }
    public string Name { get; set; } = "";
}

/// <summary>Row returned by sp_admin_list_doctors.</summary>
public class AdminDoctorRecord
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Status { get; set; } = "";
    public string DepartmentName { get; set; } = "";
}

/// <summary>Row returned by sp_admin_list_patients.</summary>
public class AdminPatientRecord
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateOnly? Dob { get; set; }
    public string? Gender { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Row returned by sp_admin_billing_report.</summary>
public class AdminBillingRecord
{
    public int BillId { get; set; }
    public string PatientName { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public DateOnly AppointmentDate { get; set; }
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "";
    public DateTime? PaymentDate { get; set; }
}

/// <summary>Helper row for bill creation validation.</summary>
public class AppointmentForBillRecord
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string Status { get; set; } = "";
    public bool HasBill { get; set; }
}
