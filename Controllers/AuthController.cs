using HospitalApi.DTOs.Auth;
using HospitalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApi.Controllers;

/// <summary>Public endpoints: register and log in. All return a JWT on success.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>Register a new patient. Returns a token so the app can log the patient in straight away.</summary>
    [HttpPost("patient/register")]
    [ProducesResponseType(typeof(PatientAuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterPatient(RegisterPatientRequest request)
    {
        var result = await _auth.RegisterPatientAsync(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Patient login with mobile number and OTP (fixed dev OTP for now).</summary>
    [HttpPost("patient/login")]
    [ProducesResponseType(typeof(PatientAuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginPatient(PatientLoginRequest request)
        => Ok(await _auth.LoginPatientAsync(request));

    /// <summary>Doctor login with email and password. Blocked if the doctor is not active.</summary>
    [HttpPost("doctor/login")]
    [ProducesResponseType(typeof(StaffAuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> LoginDoctor(StaffLoginRequest request)
        => Ok(await _auth.LoginDoctorAsync(request));

    /// <summary>Admin login with email and password.</summary>
    [HttpPost("admin/login")]
    [ProducesResponseType(typeof(StaffAuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAdmin(StaffLoginRequest request)
        => Ok(await _auth.LoginAdminAsync(request));
}