using HospitalApi.DTOs.Emergency;
using HospitalApi.Helpers;
using HospitalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApi.Controllers;

[ApiController]
[Authorize(Policy = Policies.AdminOnly)]
[Route("api/admin/ambulance")]
public class AdminAmbulanceController : ControllerBase
{
    private readonly IAmbulanceService _ambulanceService;

    public AdminAmbulanceController(IAmbulanceService ambulanceService)
    {
        _ambulanceService = ambulanceService;
    }

    /// <summary>List all ambulances with driver contacts and availability status.</summary>
    [HttpGet("fleet")]
    [ProducesResponseType(typeof(IEnumerable<AmbulanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFleet()
        => Ok(await _ambulanceService.GetAmbulancesAsync());

    /// <summary>Register a new ambulance van and driver.</summary>
    [HttpPost("fleet")]
    [ProducesResponseType(typeof(AmbulanceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAmbulance([FromBody] CreateAmbulanceRequest request)
    {
        var result = await _ambulanceService.AddAmbulanceAsync(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Update driver details or toggle availability.</summary>
    [HttpPut("fleet/{id}")]
    [ProducesResponseType(typeof(AmbulanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAmbulance(int id, [FromBody] UpdateAmbulanceRequest request)
        => Ok(await _ambulanceService.UpdateAmbulanceAsync(id, request));

    /// <summary>List all SOS ambulance emergency requests across the hospital.</summary>
    [HttpGet("requests")]
    [ProducesResponseType(typeof(IEnumerable<EmergencyBookingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRequests([FromQuery] string? status)
        => Ok(await _ambulanceService.GetAllEmergencyRequestsAsync(status));

    /// <summary>Get currently active/unresolved emergency requests (Polled for the 1-minute alert pop-up!).</summary>
    [HttpGet("requests/active")]
    [ProducesResponseType(typeof(IEnumerable<EmergencyBookingResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveRequests()
        => Ok(await _ambulanceService.GetActiveEmergencyRequestsAsync());

    /// <summary>Assign an ambulance and driver to an active emergency request.</summary>
    [HttpPut("requests/{id}/assign")]
    [ProducesResponseType(typeof(EmergencyBookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignAmbulance(int id, [FromBody] AssignAmbulanceRequest request)
        => Ok(await _ambulanceService.AssignAmbulanceAsync(id, request));

    /// <summary>Resolve an emergency request once patient is safely admitted / handled.</summary>
    [HttpPut("requests/{id}/resolve")]
    [ProducesResponseType(typeof(EmergencyBookingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveEmergency(int id, [FromBody] ResolveEmergencyRequest request)
        => Ok(await _ambulanceService.ResolveEmergencyRequestAsync(id, request));
}
