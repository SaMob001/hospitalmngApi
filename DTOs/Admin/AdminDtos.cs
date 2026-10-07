using System.ComponentModel.DataAnnotations;

namespace HospitalApi.DTOs.Admin;

/// <summary>Summary metrics for the administrative dashboard.</summary>
public class AdminDashboardStatsResponse
{
    public long AppointmentsToday { get; set; }
    public long ActiveDoctors { get; set; }
    public decimal BilledThisMonth { get; set; }
    public long PendingReports { get; set; }
}

/// <summary>Department item for dropdowns and lists.</summary>
public class DepartmentResponse
{
    public int DepartmentId { get; set; }
    public string Name { get; set; } = "";
}

/// <summary>Doctor record in admin management table.</summary>
public class AdminDoctorResponse
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Status { get; set; } = "";
    public string DepartmentName { get; set; } = "";
}

/// <summary>Request to register a new doctor.</summary>
public class CreateDoctorRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid department.")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 120 characters.")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Phone number is required.")]
    public string Phone { get; set; } = "";

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = "";
}

/// <summary>Request to update an existing doctor's profile and status.</summary>
public class UpdateDoctorRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid department.")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 120 characters.")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Phone number is required.")]
    public string Phone { get; set; } = "";

    [Required(ErrorMessage = "Status is required.")]
    [RegularExpression("^(active|on_leave|inactive)$", ErrorMessage = "Status must be 'active', 'on_leave', or 'inactive'.")]
    public string Status { get; set; } = "active";
}

/// <summary>Patient record in admin management table.</summary>
public class AdminPatientResponse
{
    public string PatientId { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public DateOnly? Dob { get; set; }
    public string? Gender { get; set; }
    public string CreatedAt { get; set; } = "";
}

/// <summary>Request to register a walk-in patient from the front desk.</summary>
public class AdminCreatePatientRequest
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 120 characters.")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Phone number is required.")]
    public string Phone { get; set; } = "";

    public DateOnly? Dob { get; set; }

    [RegularExpression("^(male|female|other)$", ErrorMessage = "Gender must be 'male', 'female', or 'other'.")]
    public string? Gender { get; set; }
}

/// <summary>Request to edit patient details by staff.</summary>
public class AdminUpdatePatientRequest
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 120 characters.")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Phone number is required.")]
    public string Phone { get; set; } = "";

    public string? Address { get; set; }
}

/// <summary>Detailed billing row for admin billing report.</summary>
public class AdminBillingResponse
{
    public int BillId { get; set; }
    public string PatientName { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public string AppointmentDate { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public string Status { get; set; } = "";
    public string? PaymentDate { get; set; }
}

/// <summary>Request to generate a bill for an appointment.</summary>
public class CreateBillRequest
{
    [Required(ErrorMessage = "Booking ID is required.")]
    public string BookingId { get; set; } = "";

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Description must be between 2 and 200 characters.")]
    public string Description { get; set; } = "";

    [Range(0.01, 1000000.0, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }
}

/// <summary>Standard action response for billing mutations.</summary>
public class BillActionResponse
{
    public int BillId { get; set; }
    public string Status { get; set; } = "";
    public string Message { get; set; } = "";
}

/// <summary>Itemized line charge within a bill.</summary>
public class BillItemDto
{
    public int ItemId { get; set; }
    public string ItemType { get; set; } = "";
    public string ItemName { get; set; } = "";
    public decimal Amount { get; set; }
}

/// <summary>Comprehensive invoice details for admin inspection and patient billing receipt.</summary>
public class BillDetailResponse
{
    public int BillId { get; set; }
    public string BookingId { get; set; } = "";
    public string PatientCode { get; set; } = "";
    public string PatientName { get; set; } = "";
    public string PatientPhone { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public string AppointmentDate { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal ConsultationFee { get; set; }
    public decimal LabCharges { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "";
    public string? PaymentDate { get; set; }
    public string CreatedAt { get; set; } = "";
    public List<BillItemDto> Items { get; set; } = [];
}
