using HospitalApi.DTOs.Patient;
using HospitalApi.Helpers;
using HospitalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApi.Controllers;

[ApiController]
[Authorize(Policy = Policies.PatientOnly)]
[Route("api/patient")]
public class PatientController : ControllerBase
{
    private readonly IPatientService _patients;

    public PatientController(IPatientService patients)
    {
        _patients = patients;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var patientId = User.GetUserId();
        return Ok(await _patients.GetProfileAsync(patientId));
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdatePatientProfileRequest request)
    {
        var patientId = User.GetUserId();
        return Ok(await _patients.UpdateProfileAsync(patientId, request));
    }

        [HttpGet("specialists")]
    public async Task<IActionResult> GetSpecialists()
        => Ok(await _patients.GetSpecialistsAsync());

    [HttpGet("doctors/{doctorId}/slots")]
    public async Task<IActionResult> GetAvailableSlots(int doctorId, [FromQuery] DateOnly date)
        => Ok(await _patients.GetAvailableSlotsAsync(doctorId, date));



        [HttpPost("appointments")]
    public async Task<IActionResult> BookAppointment(BookAppointmentRequest request)
    {
        var patientId = User.GetUserId();
        var result = await _patients.BookAppointmentAsync(patientId, request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

        [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointments()
    {
        var patientId = User.GetUserId();
        return Ok(await _patients.GetAppointmentsAsync(patientId));
    }

    [HttpPut("appointments/{id}/cancel")]
    public async Task<IActionResult> CancelAppointment(string id)
    {
        var patientId = User.GetUserId();
        return Ok(await _patients.CancelAppointmentAsync(patientId, id));
    }

    [HttpGet("reports")]
    public async Task<IActionResult> GetReports()
    {
        var patientId = User.GetUserId();
        return Ok(await _patients.GetReportsAsync(patientId));
    }

    [HttpGet("appointments/{id}/report")]
    public async Task<IActionResult> GetAppointmentReport(string id)
    {
        var patientId = User.GetUserId();
        var report = await _patients.GetAppointmentReportAsync(patientId, id);
        if (report == null) return NotFound(new ProblemDetails { Title = "Not Found", Detail = "No consultation report found for this appointment." });
        return Ok(report);
    }

    [HttpGet("prescriptions")]
    public async Task<IActionResult> GetPrescriptions()
    {
        var patientId = User.GetUserId();
        return Ok(await _patients.GetPrescriptionsAsync(patientId));
    }

    [HttpGet("billing")]
    public async Task<IActionResult> GetBilling()
    {
        var patientId = User.GetUserId();
        return Ok(await _patients.GetBillingAsync(patientId));
    }
}