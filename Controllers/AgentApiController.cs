using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediCareMS.Helpers.AI;
using MediCareMS.Helpers;
using MediCareMS.Data;
using MediCareMS.Models.Entities.Appointment;
using MediCareMS.Models.Entities.Billing;
using MediCareMS.Models.Enums;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MediCareMS.Controllers;

/// <summary>
/// REST API for direct agent actions triggered by UI button clicks.
/// All endpoints require authentication.
/// </summary>
[Authorize]
[Route("api/agent")]
[ApiController]
public class AgentApiController : ControllerBase
{
    private readonly IAgentActionService _agent;
    private readonly AppDbContext _db;
    private readonly ISslCommerzService _ssl;
    private readonly ILogger<AgentApiController> _logger;

    public AgentApiController(IAgentActionService agent, AppDbContext db, ISslCommerzService ssl, ILogger<AgentApiController> logger)
    {
        _agent = agent;
        _db = db;
        _ssl = ssl;
        _logger = logger;
    }

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

    // POST /api/agent/appointments  (simple — no payment, kept for backward compat)
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

    // POST /api/agent/book-with-payment  ← chatbot uses this to book + initiate payment
    [HttpPost("book-with-payment")]
    public async Task<IActionResult> BookWithPayment([FromBody] BookRequest req)
    {
        if (req.DoctorId <= 0 || string.IsNullOrWhiteSpace(req.Date) || string.IsNullOrWhiteSpace(req.Time))
            return BadRequest(new { success = false, error = "doctorId, date, and time are required." });

        var userId = UserId();

        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
        if (patient == null)
            return BadRequest(new { success = false, error = "No patient profile found." });

        var doctor = await _db.Doctors
            .Include(d => d.Specialization)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.Id == req.DoctorId && !d.IsDeleted);
        if (doctor == null)
            return BadRequest(new { success = false, error = "Doctor not found." });

        if (!DateOnly.TryParse(req.Date, out var apptDate))
            return BadRequest(new { success = false, error = "Invalid date." });

        if (!TimeOnly.TryParse(req.Time, out var apptTime))
            return BadRequest(new { success = false, error = "Invalid time." });

        // Conflict check
        var conflict = await _db.Appointments.AnyAsync(a =>
            a.DoctorId == req.DoctorId &&
            a.AppointmentDate == apptDate &&
            a.AppointmentTime == apptTime &&
            !a.IsDeleted &&
            a.Status != AppointmentStatus.Cancelled);
        if (conflict)
            return BadRequest(new { success = false, error = "This slot is already booked. Please choose another time." });

        // 1. Create appointment (PendingPayment)
        var count = await _db.Appointments.CountAsync() + 1;
        var tokenCount = await _db.Appointments
            .CountAsync(a => a.DoctorId == req.DoctorId && a.AppointmentDate == apptDate && !a.IsDeleted);

        var appointment = new Appointment
        {
            AppointmentNo   = $"APT-{DateTime.Today.Year}-{count:D4}",
            PatientId       = patient.Id,
            DoctorId        = req.DoctorId,
            AppointmentDate = apptDate,
            AppointmentTime = apptTime,
            TokenNumber     = tokenCount + 1,
            Status          = AppointmentStatus.PendingPayment,
            ChiefComplaint  = req.ChiefComplaint,
            CreatedAt       = DateTime.UtcNow,
            CreatedBy       = userId
        };
        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        // 2. Create invoice
        var invoiceCount = await _db.Invoices.CountAsync() + 1;
        var invoice = new Invoice
        {
            InvoiceNo       = $"INV-{DateTime.Today.Year}-{invoiceCount:D4}",
            AppointmentId   = appointment.Id,
            PatientId       = patient.Id,
            DoctorId        = doctor.Id,
            ConsultationFee = doctor.ConsultationFee,
            TestFee         = 0,
            OtherCharges    = 0,
            Discount        = 0,
            TotalAmount     = doctor.ConsultationFee,
            PaidAmount      = 0,
            Status          = PaymentStatus.Unpaid,
            DueDate         = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            Notes           = $"Consultation fee for {appointment.AppointmentNo}",
            CreatedAt       = DateTime.UtcNow,
            CreatedBy       = userId
        };
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        // 3. Create pending payment record
        var txnId = $"MCR-{appointment.Id}-{DateTime.UtcNow.Ticks}";
        var pending = new Payment
        {
            InvoiceId            = invoice.Id,
            Amount               = doctor.ConsultationFee,
            Method               = PaymentMethod.Online,
            TransactionReference = txnId,
            SslStatus            = SSLCommerzStatus.Pending,
            PaidAt               = DateTime.UtcNow,
            CreatedAt            = DateTime.UtcNow,
            CreatedBy            = userId
        };
        _db.Payments.Add(pending);
        await _db.SaveChangesAsync();

        // 4. Initiate SSLCommerz payment
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var initResponse = await _ssl.InitiatePaymentAsync(new SslCommerzInitRequest
        {
            TransactionId   = txnId,
            Amount          = doctor.ConsultationFee,
            CustomerName    = patient.FullName,
            CustomerEmail   = User.FindFirstValue(ClaimTypes.Email) ?? "patient@medicare.com",
            CustomerPhone   = patient.MobileNumber ?? "01700000000",
            ProductName     = $"Appointment {appointment.AppointmentNo} — Dr. {doctor.FullName}",
            ProductCategory = "Medical Consultation",
            SuccessUrl      = $"{baseUrl}/Payment/Success",
            FailUrl         = $"{baseUrl}/Payment/Fail",
            CancelUrl       = $"{baseUrl}/Payment/Cancel",
            IpnUrl          = $"{baseUrl}/Payment/Ipn"
        });

        if (!initResponse.IsSuccess)
        {
            pending.SslStatus     = SSLCommerzStatus.Failed;
            pending.FailureReason = initResponse.ErrorMessage;
            await _db.SaveChangesAsync();
            _logger.LogError("SSLCommerz init failed for chatbot booking {AptNo}: {Reason}",
                appointment.AppointmentNo, initResponse.ErrorMessage);
            return BadRequest(new { success = false, error = $"Payment gateway error: {initResponse.ErrorMessage}. Appointment saved — retry from My Appointments." });
        }

        pending.SslSessionKey = initResponse.SessionKey;
        await _db.SaveChangesAsync();

        var card = new AppointmentCardDto
        {
            Id             = appointment.Id,
            AppointmentNo  = appointment.AppointmentNo,
            DoctorName     = doctor.FullName,
            Specialization = doctor.Specialization?.Name ?? "",
            Date           = apptDate.ToString("dddd, MMMM d, yyyy"),
            Time           = apptTime.ToString("h:mm tt"),
            Status         = "PendingPayment",
            ChiefComplaint = req.ChiefComplaint,
            Fee            = doctor.ConsultationFee,
            PaymentUrl     = initResponse.GatewayUrl
        };

        return Ok(new { success = true, appointment = card });
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
