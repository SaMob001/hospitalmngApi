using System.ComponentModel.DataAnnotations;
using HospitalApi.Helpers;

namespace HospitalApi.DTOs.Patient;

public class PatientProfileResponse
{
    public string Id { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string MobileNumber { get; set; } = "";
    public DateOnly? Dob { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
}

public class UpdatePatientProfileRequest
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 120 characters.")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Please enter your mobile number.")]
    [RegularExpression(ValidationPatterns.Phone, ErrorMessage = "Please enter a valid mobile number.")]
    public string Phone { get; set; } = "";

    [StringLength(255, ErrorMessage = "Address is too long.")]
    public string? Address { get; set; }

    /// <summary>Optional. Format: yyyy-MM-dd.</summary>
    public DateOnly? Dob { get; set; }

    /// <summary>Optional. One of: male, female, other.</summary>
    [RegularExpression(ValidationPatterns.Gender, ErrorMessage = "Gender must be male, female or other.")]
    public string? Gender { get; set; }
}