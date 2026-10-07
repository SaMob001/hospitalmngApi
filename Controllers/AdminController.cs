using HospitalApi.DTOs.Admin;
using HospitalApi.Helpers;
using HospitalApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApi.Controllers;

[ApiController]
[Authorize(Policy = Policies.AdminOnly)]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>Get summary operational metrics for the administrative dashboard.</summary>
    [HttpGet("dashboard/stats")]
    [ProducesResponseType(typeof(AdminDashboardStatsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardStats()
        => Ok(await _adminService.GetDashboardStatsAsync());

    /// <summary>List all hospital departments for selection dropdowns.</summary>
    [HttpGet("departments")]
    [ProducesResponseType(typeof(IEnumerable<DepartmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments()
        => Ok(await _adminService.GetDepartmentsAsync());

    // ----------------------------------------------------------- doctors

    /// <summary>List all doctors with their department and employment status.</summary>
    [HttpGet("doctors")]
    [ProducesResponseType(typeof(IEnumerable<AdminDoctorResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListDoctors()
        => Ok(await _adminService.ListDoctorsAsync());

    /// <summary>Register a new doctor account with login credentials.</summary>
    [HttpPost("doctors")]
    [ProducesResponseType(typeof(AdminDoctorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddDoctor(CreateDoctorRequest request)
    {
        var result = await _adminService.AddDoctorAsync(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Update doctor profile, assigned department, and employment status.</summary>
    [HttpPut("doctors/{id}")]
    [ProducesResponseType(typeof(AdminDoctorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateDoctor(int id, UpdateDoctorRequest request)
        => Ok(await _adminService.UpdateDoctorAsync(id, request));

    // ---------------------------------------------------------- patients

    /// <summary>List all registered patients.</summary>
    [HttpGet("patients")]
    [ProducesResponseType(typeof(IEnumerable<AdminPatientResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListPatients()
        => Ok(await _adminService.ListPatientsAsync());

    /// <summary>Add a new walk-in patient from the hospital front desk.</summary>
    [HttpPost("patients")]
    [ProducesResponseType(typeof(AdminPatientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddPatient(AdminCreatePatientRequest request)
    {
        var result = await _adminService.AddPatientAsync(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Update patient demographics and address.</summary>
    [HttpPut("patients/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdatePatient(int id, AdminUpdatePatientRequest request)
    {
        await _adminService.UpdatePatientAsync(id, request);
        return NoContent();
    }

    // ----------------------------------------------------------- billing

    /// <summary>Get billing and payment reports across consultations, optionally filtered by status.</summary>
    [HttpGet("billing")]
    [ProducesResponseType(typeof(IEnumerable<AdminBillingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBillingReport([FromQuery] string? status)
        => Ok(await _adminService.GetBillingReportAsync(status));

    /// <summary>Create a billing invoice for an appointment consultation.</summary>
    [HttpPost("billing")]
    [ProducesResponseType(typeof(BillActionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateBill(CreateBillRequest request)
    {
        var result = await _adminService.CreateBillAsync(request);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Get comprehensive itemized invoice details by bill ID.</summary>
    [HttpGet("billing/{id}")]
    [ProducesResponseType(typeof(BillDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBillDetails(int id)
        => Ok(await _adminService.GetBillDetailsAsync(id));

    /// <summary>Mark an outstanding medical bill as paid.</summary>
    [HttpPut("billing/{id}/pay")]
    [ProducesResponseType(typeof(BillActionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MarkBillPaid(int id)
        => Ok(await _adminService.MarkBillPaidAsync(id));
}
