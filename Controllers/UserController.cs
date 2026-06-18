using System.Security.Claims;
using MediCareMS.Data;
using MediCareMS.Helpers;
using MediCareMS.Models.Enums;
using MediCareMS.Models.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCareMS.Models.Entities.Appointment;

namespace MediCareMS.Controllers;

[Authorize(Roles = "Patient")]
public class UserController : Controller
{
    private readonly AppDbContext _db;

    public UserController(AppDbContext db)
    {
        _db = db;
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

    public async Task<IActionResult> Dashboard()
    {
        var patient = await GetCurrentPatientAsync();
        if (patient == null) return RedirectToAction("Login", "Auth");

        var patientId = patient.Id;
        var today = DateOnly.FromDateTime(DateTime.Today);

        var vm = new UserDashboardViewModel
        {
            TotalAppointments    = await _db.Appointments.CountAsync(a => a.PatientId == patientId && !a.IsDeleted),
            UpcomingAppointments = await _db.Appointments.CountAsync(a => a.PatientId == patientId && a.AppointmentDate >= today && a.Status != AppointmentStatus.Cancelled && !a.IsDeleted),
            TotalPrescriptions   = await _db.Prescriptions.CountAsync(p => p.PatientId == patientId),
            PatientBloodGroup    = patient.BloodGroup?.ToString(),
            PatientMobile        = patient.MobileNumber,
            RecentAppointments   = await _db.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.Department)
                .Where(a => a.PatientId == patientId && !a.IsDeleted)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(5)
                .Select(a => new MyAppointmentItem
                {
                    Id             = a.Id,
                    AppointmentNo  = a.AppointmentNo,
                    DoctorName     = a.Doctor.FullName,
                    DepartmentName = a.Doctor.Department.Name,
                    Date           = a.AppointmentDate,
                    Status         = a.Status
                }).ToListAsync()
        };

        return View(vm);
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
            .Select(a => new MyAppointmentItem
            {
                Id             = a.Id,
                AppointmentNo  = a.AppointmentNo,
                DoctorName     = a.Doctor.FullName,
                DepartmentName = a.Doctor.Department.Name,
                Date           = a.AppointmentDate,
                Status         = a.Status
            }).ToListAsync();

        return View(appointments);
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
            ViewBag.Doctors     = await _db.Doctors.Where(d => !d.IsDeleted && d.Status == DoctorStatus.Available).ToListAsync();
            return View(model);
        }

        var count      = await _db.Appointments.CountAsync() + 1;
        var tokenCount = await _db.Appointments
            .CountAsync(a => a.DoctorId == model.DoctorId && a.AppointmentDate == model.AppointmentDate && !a.IsDeleted);

        var appointment = new Appointment
        {
            AppointmentNo   = $"APT-{DateTime.Today.Year}-{count:D4}",
            PatientId       = patient.Id,
            DoctorId        = model.DoctorId,
            AppointmentDate = model.AppointmentDate,
            AppointmentTime = model.AppointmentTime,
            TokenNumber     = tokenCount + 1,
            Status          = AppointmentStatus.Pending,
            ChiefComplaint  = model.Symptoms,
            CreatedAt       = DateTime.UtcNow
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Appointment booked! Token #{appointment.TokenNumber}. Waiting for confirmation.";
        return RedirectToAction(nameof(MyAppointments));
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
