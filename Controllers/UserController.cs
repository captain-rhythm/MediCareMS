using System.Security.Claims;
using MediCareMS.Data;
using MediCareMS.Helpers;
using MediCareMS.Models.Entities.Appointment;
using MediCareMS.Models.Entities.Billing;
using MediCareMS.Models.Enums;
using MediCareMS.Models.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCareMS.Controllers;

[Authorize(Roles = "Patient")]
public class UserController : Controller
{
    private readonly AppDbContext _db;
    private readonly ISslCommerzService _ssl;
    private readonly ILogger<UserController> _logger;

    public UserController(AppDbContext db, ISslCommerzService ssl, ILogger<UserController> logger)
    {
        _db = db; _ssl = ssl; _logger = logger;
    }

    /// <summary>
    /// Gets the current user's ID from the NameIdentifier claim.
    /// </summary>
    private int GetCurrentUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdStr, out var uid) ? uid : 0;
    }

    /// <summary>
    /// Finds the Patient record linked to the currently logged-in user.
    /// </summary>
    private async Task<MediCareMS.Models.Entities.Patient.Patient?> GetCurrentPatientAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return null;
        return await _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
    }

    public IActionResult Portal()
    {
        return View();
    }

    public IActionResult Dashboard()
    {
        return RedirectToAction(nameof(MyAppointments));
    }

    [HttpGet]
    public async Task<IActionResult> MyProfile()
    {
        var patient = await GetCurrentPatientAsync();
        if (patient == null) return RedirectToAction("Login", "Auth");

        var model = new MyProfileViewModel
        {
            FullName              = patient.FullName,
            MobileNumber          = patient.MobileNumber ?? "",
            BloodGroup            = patient.BloodGroup ?? BloodGroup.Unknown,
            DateOfBirth           = patient.DateOfBirth,
            Gender                = patient.Gender,
            Address               = patient.Address,
            EmergencyContactName  = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MyProfile(MyProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var patient = await GetCurrentPatientAsync();
        if (patient == null) return RedirectToAction("Login", "Auth");

        patient.FullName              = model.FullName;
        patient.MobileNumber          = model.MobileNumber;
        patient.BloodGroup            = model.BloodGroup;
        patient.DateOfBirth           = model.DateOfBirth;
        patient.Gender                = model.Gender;
        patient.Address               = model.Address;
        patient.EmergencyContactName  = model.EmergencyContactName;
        patient.EmergencyContactPhone = model.EmergencyContactPhone;
        patient.UpdatedAt             = DateTime.UtcNow;

        // Also sync phone to ApplicationUser
        if (patient.UserId.HasValue)
        {
            var user = await _db.Users.FindAsync(patient.UserId.Value);
            if (user != null)
            {
                user.PhoneNumber = model.MobileNumber;
                user.UpdatedAt   = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Profile updated successfully!";
        return RedirectToAction(nameof(MyProfile));
    }

    public async Task<IActionResult> MyAppointments()
    {
        var patient = await GetCurrentPatientAsync();
        if (patient == null) return RedirectToAction("Login", "Auth");

        var appointments = await _db.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.Department)
            .Where(a => a.PatientId == patient.Id && !a.IsDeleted)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        // Fetch invoice IDs for PendingPayment appointments
        var aptIds = appointments.Select(a => a.Id).ToList();
        var invoiceMap = await _db.Invoices
            .Where(i => aptIds.Contains(i.AppointmentId) && !i.IsDeleted)
            .ToDictionaryAsync(i => i.AppointmentId, i => i.Id);

        var items = appointments.Select(a => new MyAppointmentItem
        {
            Id             = a.Id,
            AppointmentNo  = a.AppointmentNo,
            DoctorName     = a.Doctor.FullName,
            DepartmentName = a.Doctor.Department.Name,
            Date           = a.AppointmentDate,
            Status         = a.Status,
            InvoiceId      = invoiceMap.TryGetValue(a.Id, out var iid) ? iid : null
        }).ToList();

        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var patient = await GetCurrentPatientAsync();
        if (patient == null) return RedirectToAction("Login", "Auth");

        var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patient.Id && !a.IsDeleted);

        if (appointment == null)
        {
            TempData["Error"] = "Appointment not found or access denied.";
            return RedirectToAction(nameof(MyAppointments));
        }

        if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Cancelled)
        {
            TempData["Error"] = "Cannot cancel an already completed or cancelled appointment.";
            return RedirectToAction(nameof(MyAppointments));
        }

        appointment.Status    = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Appointment cancelled successfully.";
        return RedirectToAction(nameof(MyAppointments));
    }

    public async Task<IActionResult> MyPrescriptions()
    {
        var patient = await GetCurrentPatientAsync();
        if (patient == null) return RedirectToAction("Login", "Auth");

        var prescriptions = await _db.Prescriptions
            .Include(p => p.Appointment)
            .Include(p => p.Doctor)
            .Where(p => p.PatientId == patient.Id)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(prescriptions);
    }

    [HttpGet]
    public async Task<IActionResult> BookAppointment()
    {
        ViewBag.Departments = await _db.Departments.Where(d => !d.IsDeleted).ToListAsync();
        ViewBag.Doctors     = await _db.Doctors.Where(d => !d.IsDeleted && d.Status == DoctorStatus.Available).ToListAsync();
        return View(new BookAppointmentViewModel
        {
            AppointmentDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            AppointmentTime = new TimeOnly(9, 0)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookAppointment(BookAppointmentViewModel model)
    {
        var patient = await GetCurrentPatientAsync();
        if (patient == null) return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await _db.Departments.Where(d => !d.IsDeleted).ToListAsync();
            ViewBag.Doctors     = await _db.Doctors
                .Include(d => d.Department)
                .Where(d => !d.IsDeleted && d.Status == DoctorStatus.Available).ToListAsync();
            return View(model);
        }

        var doctor = await _db.Doctors.Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.Id == model.DoctorId && !d.IsDeleted);
        if (doctor == null) { TempData["Error"] = "Doctor not found."; return RedirectToAction(nameof(BookAppointment)); }

        var count      = await _db.Appointments.CountAsync() + 1;
        var tokenCount = await _db.Appointments
            .CountAsync(a => a.DoctorId == model.DoctorId && a.AppointmentDate == model.AppointmentDate && !a.IsDeleted);
        var userId = GetCurrentUserId();

        // 1. Create appointment with PendingPayment status
        var appointment = new Appointment
        {
            AppointmentNo   = $"APT-{DateTime.Today.Year}-{count:D4}",
            PatientId       = patient.Id,
            DoctorId        = model.DoctorId,
            AppointmentDate = model.AppointmentDate,
            AppointmentTime = model.AppointmentTime,
            TokenNumber     = tokenCount + 1,
            Status          = AppointmentStatus.PendingPayment,
            ChiefComplaint  = model.Symptoms,
            CreatedAt       = DateTime.UtcNow
        };
        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        // 2. Auto-create invoice for consultation fee
        var invoiceCount = await _db.Invoices.CountAsync() + 1;
        var invoice = new Invoice
        {
            InvoiceNo     = $"INV-{DateTime.Today.Year}-{invoiceCount:D4}",
            AppointmentId = appointment.Id,
            PatientId     = patient.Id,
            DoctorId      = doctor.Id,
            ConsultationFee = doctor.ConsultationFee,
            TestFee        = 0,
            OtherCharges   = 0,
            Discount       = 0,
            TotalAmount    = doctor.ConsultationFee,
            PaidAmount     = 0,
            Status         = PaymentStatus.Unpaid,
            DueDate        = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            Notes          = $"Consultation fee for appointment {appointment.AppointmentNo}",
            CreatedAt      = DateTime.UtcNow,
            CreatedBy      = userId
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
        var success = Url.Action("Success", "Payment", null, Request.Scheme, Request.Host.ToString())!;
        var fail    = Url.Action("Fail",    "Payment", null, Request.Scheme, Request.Host.ToString())!;
        var cancel  = Url.Action("Cancel",  "Payment", null, Request.Scheme, Request.Host.ToString())!;
        var ipn     = Url.Action("Ipn",     "Payment", null, Request.Scheme, Request.Host.ToString())!;

        var initResponse = await _ssl.InitiatePaymentAsync(new SslCommerzInitRequest
        {
            TransactionId   = txnId,
            Amount          = doctor.ConsultationFee,
            CustomerName    = patient.FullName,
            CustomerEmail   = User.FindFirstValue(ClaimTypes.Email) ?? "patient@medicare.com",
            CustomerPhone   = patient.MobileNumber ?? "01700000000",
            ProductName     = $"Appointment {appointment.AppointmentNo} — Dr. {doctor.FullName}",
            ProductCategory = "Medical Consultation",
            SuccessUrl      = success,
            FailUrl         = fail,
            CancelUrl       = cancel,
            IpnUrl          = ipn
        });

        if (!initResponse.IsSuccess)
        {
            // Gateway failed — keep appointment as PendingPayment, let patient retry
            pending.SslStatus     = SSLCommerzStatus.Failed;
            pending.FailureReason = initResponse.ErrorMessage;
            await _db.SaveChangesAsync();

            _logger.LogError("SSLCommerz init failed for appointment {AptNo}: {Reason}", appointment.AppointmentNo, initResponse.ErrorMessage);
            TempData["Error"] = $"Payment gateway error: {initResponse.ErrorMessage}. Your appointment is reserved — retry payment from My Appointments.";
            return RedirectToAction(nameof(MyAppointments));
        }

        pending.SslSessionKey = initResponse.SessionKey;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Redirecting patient to SSLCommerz for appointment {AptNo} txnId={TxnId}", appointment.AppointmentNo, txnId);
        return Redirect(initResponse.GatewayUrl!);
    }

    public async Task<IActionResult> MyInvoices(CancellationToken ct = default)
    {
        var patient = await GetCurrentPatientAsync();
        if (patient == null) return RedirectToAction("Login", "Auth");

        var invoices = await _db.Invoices
            .Include(i => i.Doctor)
            .Include(i => i.Appointment)
            .Where(i => i.PatientId == patient.Id && !i.IsDeleted)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);

        return View(invoices);
    }

    // ── Family Patient Manager ────────────────────────────────────────────────

    public async Task<IActionResult> FamilyPatients()
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return RedirectToAction("Login", "Auth");

        var patients = await _db.Patients
            .Where(p => p.CreatedBy == userId && !p.IsDeleted)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();

        var vm = new FamilyPatientManagerViewModel
        {
            Patients = patients.Select(p => new FamilyPatientItem
            {
                Id              = p.Id,
                FullName        = p.FullName,
                DateOfBirth     = p.DateOfBirth,
                Gender          = p.Gender.ToString(),
                FatherMotherName = p.EmergencyContactName,
                District        = p.Address?.Split(',').ElementAtOrDefault(0)?.Trim(),
                Thana           = p.Address?.Split(',').ElementAtOrDefault(1)?.Trim()
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFamilyPatient(AddFamilyPatientViewModel model)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return RedirectToAction("Login", "Auth");

        // Enforce 5-patient limit
        var existing = await _db.Patients.CountAsync(p => p.CreatedBy == userId && !p.IsDeleted);
        if (existing >= 5)
        {
            TempData["Error"] = "Maximum 5 patients allowed per account.";
            return RedirectToAction(nameof(FamilyPatients));
        }

        if (!Enum.TryParse<MediCareMS.Models.Enums.Gender>(model.Gender, out var gender))
            gender = MediCareMS.Models.Enums.Gender.Male;

        var count = await _db.Patients.CountAsync() + 1;
        var patient = new MediCareMS.Models.Entities.Patient.Patient
        {
            PatientNo            = $"PAT-{DateTime.Today.Year}-{count:D4}",
            FullName             = model.FullName,
            DateOfBirth          = model.DateOfBirth,
            Gender               = gender,
            EmergencyContactName = model.FatherMotherName,
            Address              = $"{model.District}, {model.Thana}",
            CreatedBy            = userId,
            CreatedAt            = DateTime.UtcNow
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Patient '{patient.FullName}' added successfully."
;
        return RedirectToAction(nameof(FamilyPatients));
    }

    [HttpGet]
    public IActionResult GetThanas(string district)
    {
        var thanas = BangladeshGeoData.GetThanas(district);
        return Json(thanas);
    }
}
