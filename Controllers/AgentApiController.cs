using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediCareMS.Helpers.AI;
using System.Security.Claims;

namespace MediCareMS.Controllers;

/// <summary>
/// REST API for direct agent actions triggered by UI button clicks
/// (e.g. clicking a slot button, clicking "Book" on a doctor card).
/// All endpoints require authentication.
/// </summary>
[Authorize]
[Route("api/agent")]
[ApiController]
public class AgentApiController : ControllerBase
{
    private readonly IAgentActionService _agent;

    public AgentApiController(IAgentActionService agent) => _agent = agent;

    private int UserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    // GET /api/agent/doctors?spec=Cardiologist&name=&date=2026-06-19
    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors(
        [FromQuery] string? spec,
        [FromQuery] string? name,
        [FromQuery] string? date)
    {
        var result = await _agent.SearchDoctorsAsync(spec, name, date);
        return Ok(result);
    }

    // GET /api/agent/slots?doctorId=5&date=2026-06-19
    [HttpGet("slots")]
    public async Task<IActionResult> GetSlots(
        [FromQuery] int    doctorId,
        [FromQuery] string date)
    {
        if (doctorId <= 0 || string.IsNullOrWhiteSpace(date))
            return BadRequest("doctorId and date are required.");

        var result = await _agent.GetAvailableSlotsAsync(doctorId, date);
        return Ok(result);
    }

    // GET /api/agent/appointments
    [HttpGet("appointments")]
    public async Task<IActionResult> GetMyAppointments()
    {
        var result = await _agent.GetMyAppointmentsAsync(UserId());
        return Ok(result);
    }

    // POST /api/agent/appointments
    [HttpPost("appointments")]
    public async Task<IActionResult> Book([FromBody] BookRequest req)
    {
        if (req.DoctorId <= 0 || string.IsNullOrWhiteSpace(req.Date) || string.IsNullOrWhiteSpace(req.Time))
            return BadRequest("doctorId, date, and time are required.");

        var (ok, card, err) = await _agent.CreateAppointmentAsync(
            req.DoctorId, req.Date, req.Time, req.ChiefComplaint, UserId());

        return ok ? Ok(new { success = true, appointment = card })
                  : BadRequest(new { success = false, error = err });
    }

    // DELETE /api/agent/appointments/{id}
    [HttpDelete("appointments/{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var (ok, msg) = await _agent.CancelAppointmentAsync(id, UserId());
        return ok ? Ok(new { success = true, message = msg })
                  : BadRequest(new { success = false, error = msg });
    }

    // PUT /api/agent/appointments/{id}/reschedule
    [HttpPut("appointments/{id:int}/reschedule")]
    public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.NewDate) || string.IsNullOrWhiteSpace(req.NewTime))
            return BadRequest("newDate and newTime are required.");

        var (ok, card, err) = await _agent.RescheduleAppointmentAsync(id, req.NewDate, req.NewTime, UserId());
        return ok ? Ok(new { success = true, appointment = card })
                  : BadRequest(new { success = false, error = err });
    }

    // GET /api/agent/profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var profile = await _agent.GetPatientProfileAsync(UserId());
        return profile != null ? Ok(profile) : NotFound("No patient profile found.");
    }

    public record BookRequest(int DoctorId, string Date, string Time, string? ChiefComplaint);
    public record RescheduleRequest(string NewDate, string NewTime);
}
