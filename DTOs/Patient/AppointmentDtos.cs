using System.ComponentModel.DataAnnotations;

namespace HospitalApi.DTOs.Patient;

public class BookAppointmentRequest
{
    [Required(ErrorMessage = "Please select a doctor.")]
    public int DoctorId { get; set; }

    /// <summary>Format: yyyy-MM-dd.</summary>
    [Required(ErrorMessage = "Please select a date.")]
    public DateOnly Date { get; set; }

    /// <summary>Format: HH:mm (24-hour).</summary>
    [Required(ErrorMessage = "Please select a time.")]
    public TimeOnly Time { get; set; }

    [StringLength(255, ErrorMessage = "Reason is too long.")]
    public string? Reason { get; set; }
}

/// <summary>Matches the Flutter Appointment model exactly.</summary>
public class AppointmentResponse
{
    public string BookingId { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public string Date { get; set; } = "";
    public string Time { get; set; } = "";
    public string? Reason { get; set; }="";
        public string Status { get; set; } = "";
}