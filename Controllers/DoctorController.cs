using HospitalApi.DTOs.Doctor;
using HospitalApi.Helpers;
using HospitalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApi.Controllers;

[ApiController]
[Authorize(Policy = Policies.DoctorOnly)]
[Route("api/doctor")]
public class DoctorController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    /// <summary>Get today's scheduled consultations for the authenticated doctor.</summary>
    [HttpGet("appointments/today")]
    [ProducesResponseType(typeof(IEnumerable<DoctorConsultationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTodayConsultations([FromQuery] DateOnly? date)
    {
        var doctorId = User.GetUserId();
        return Ok(await _doctorService.GetTodayConsultationsAsync(doctorId, date));
    }

    /// <summary>Get weekly availability schedule for the authenticated doctor.</summary>
    [HttpGet("availability")]
    [ProducesResponseType(typeof(IEnumerable<DoctorAvailabilityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAvailability()
    {
        var doctorId = User.GetUserId();
        return Ok(await _doctorService.GetAvailabilityAsync(doctorId));
    }

    /// <summary>Set or update working hours for a specific day of the week.</summary>
    [HttpPut("availability")]
    [ProducesResponseType(typeof(IEnumerable<DoctorAvailabilityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetAvailability(SetDoctorAvailabilityRequest request)
    {
        var doctorId = User.GetUserId();
        return Ok(await _doctorService.SetAvailabilityAsync(doctorId, request));
    }

    /// <summary>Add a medical consultation report (diagnosis, prescription, billing) for an appointment.</summary>
    [HttpPost("reports")]
    [ProducesResponseType(typeof(ReportAddedResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddReport(AddReportRequest request)
    {
        var doctorId = User.GetUserId();
        var result = await _doctorService.AddReportAsync(doctorId, request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Get consultation report details to prefill the report editing form.</summary>
    [HttpGet("appointments/{bookingId}/report")]
    [ProducesResponseType(typeof(DoctorAppointmentReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReportForAppointment(string bookingId)
    {
        var doctorId = User.GetUserId();
        return Ok(await _doctorService.GetReportForAppointmentAsync(doctorId, bookingId));
    }

    /// <summary>Update an existing medical consultation report, prescriptions, and billing.</summary>
    [HttpPut("appointments/{bookingId}/report")]
    [HttpPut("reports")]
    [ProducesResponseType(typeof(ReportAddedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReport([FromRoute] string? bookingId, [FromBody] AddReportRequest request)
    {
        var doctorId = User.GetUserId();
        if (!string.IsNullOrWhiteSpace(bookingId))
        {
            request.BookingId = bookingId;
        }
        var result = await _doctorService.UpdateReportAsync(doctorId, request);
        return Ok(result);
    }


    /// <summary>Search past consultations and patient diagnosis history for the authenticated doctor.</summary>
    [HttpGet("patients/history")]
    [ProducesResponseType(typeof(IEnumerable<DoctorPatientHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPatientHistory([FromQuery] string? search)
    {
        var doctorId = User.GetUserId();
        return Ok(await _doctorService.GetPatientHistoryAsync(doctorId, search));
    }
}
