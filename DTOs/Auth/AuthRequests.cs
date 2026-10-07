using System.ComponentModel.DataAnnotations;
using HospitalApi.Helpers;

namespace HospitalApi.DTOs.Auth;

public class RegisterPatientRequest
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 120 characters.")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(150, ErrorMessage = "Email address is too long.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Please enter your mobile number.")]
    [RegularExpression(ValidationPatterns.Phone, ErrorMessage = "Please enter a valid mobile number.")]
    public string Phone { get; set; } = "";

    /// <summary>Optional. Format: yyyy-MM-dd.</summary>
    public DateOnly? Dob { get; set; }

    /// <summary>Optional. One of: male, female, other.</summary>
    [RegularExpression(ValidationPatterns.Gender, ErrorMessage = "Gender must be male, female or other.")]
    public string? Gender { get; set; }
}

public class PatientLoginRequest
{
    [Required(ErrorMessage = "Please enter your mobile number.")]
    [RegularExpression(ValidationPatterns.Phone, ErrorMessage = "Please enter a valid mobile number.")]
    public string Phone { get; set; } = "";

    [Required(ErrorMessage = "Please enter the OTP.")]
    [StringLength(10, ErrorMessage = "The OTP is not valid.")]
    public string Otp { get; set; } = "";
}

/// <summary>Used by both doctor and admin login.</summary>
public class StaffLoginRequest
{
    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(150, ErrorMessage = "Email address is too long.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Please enter your password.")]
    [StringLength(128, ErrorMessage = "The password is not valid.")]
    public string Password { get; set; } = "";
}