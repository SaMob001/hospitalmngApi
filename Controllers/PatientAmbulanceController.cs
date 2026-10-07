using HospitalApi.DTOs.Emergency;
using HospitalApi.Helpers;
using HospitalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApi.Controllers;

[ApiController]
[Route("api/patient/ambulance")]
public class PatientAmbulanceController : ControllerBase
{
    private readonly IAmbulanceService _ambulanceService;

    public PatientAmbulanceController(IAmbulanceService ambulanceService)
    {
        _ambulanceService = ambulanceService;
    }

    /// <summary>Emergency SOS booking for ambulance dispatch.</summary>
    [HttpPost("book")]
    [ProducesResponseType(typeof(EmergencyBookingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BookAmbulance([FromBody] BookAmbulanceRequest request)
    {
        int? patientId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            try { patientId = User.GetUserId(); } catch { }
        }

        var response = await _ambulanceService.BookAmbulanceAsync(patientId, request);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>List my emergency ambulance bookings.</summary>
    [Authorize(Policy = Policies.PatientOnly)]
    [HttpGet("my-requests")]
    [ProducesResponseType(typeof(IEnumerable<EmergencyBookingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyRequests()
    {
        var patientId = User.GetUserId();
        var list = await _ambulanceService.GetPatientEmergencyRequestsAsync(patientId);
        return Ok(list);
    }

    /// <summary>Track live emergency status by SOS booking code (e.g. SOS-10001).</summary>
    [HttpGet("{code}")]
    [ProducesResponseType(typeof(EmergencyBookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusByCode(string code)
    {
        var record = await _ambulanceService.GetEmergencyRequestByCodeAsync(code);
        if (record == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = $"Emergency request code '{code}' was not found."
            });
        }
        return Ok(record);
    }
}
