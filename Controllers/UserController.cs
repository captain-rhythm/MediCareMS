using MediCareMS.Data;
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

    private async Task<int> GetPatientIdAsync()
    {
        var email = User.Identity?.Name;
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Email == email && !p.IsDeleted);
        return patient?.Id ?? 0;
    }

    public async Task<IActionResult> Dashboard()
    {
        var email = User.Identity?.Name;
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Email == email && !p.IsDeleted);
        var patientId = patient?.Id ?? 0;
        if (patientId == 0) return RedirectToAction("Login", "Auth");

        var today = DateOnly.FromDateTime(DateTime.Today);

        var vm = new UserDashboardViewModel
        {
            TotalAppointments = await _db.Appointments.CountAsync(a => a.PatientId == patientId && !a.IsDeleted),
            UpcomingAppointments = await _db.Appointments.CountAsync(a => a.PatientId == patientId && a.AppointmentDate >= today && a.Status != AppointmentStatus.Cancelled && !a.IsDeleted),
            TotalPrescriptions = await _db.Prescriptions.CountAsync(p => p.PatientId == patientId),
            PatientBloodGroup = patient?.BloodGroup?.ToString(),
            PatientMobile = patient?.MobileNumber,
            RecentAppointments = await _db.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.Department)
                .Where(a => a.PatientId == patientId && !a.IsDeleted)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(5)
                .Select(a => new MyAppointmentItem
                {
                    Id = a.Id,
                    AppointmentNo = a.AppointmentNo,
                    DoctorName = a.Doctor.FullName,
                    DepartmentName = a.Doctor.Department.Name,
                    Date = a.AppointmentDate,
                    Status = a.Status
                }).ToListAsync()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> MyProfile()
    {
        var email = User.Identity?.Name;
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Email == email && !p.IsDeleted);
        if (patient == null) return RedirectToAction("Login", "Auth");

        var model = new MyProfileViewModel
        {
            FullName = patient.FullName,
            MobileNumber = patient.MobileNumber ?? "",
            BloodGroup = patient.BloodGroup ?? BloodGroup.Unknown,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            Address = patient.Address,
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MyProfile(MyProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = User.Identity?.Name;
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.Email == email && !p.IsDeleted);
        if (patient == null) return RedirectToAction("Login", "Auth");

        patient.FullName = model.FullName;
        patient.MobileNumber = model.MobileNumber;
        patient.BloodGroup = model.BloodGroup;
        patient.DateOfBirth = model.DateOfBirth;
        patient.Gender = model.Gender;
        patient.Address = model.Address;
        patient.EmergencyContactName = model.EmergencyContactName;
        patient.EmergencyContactPhone = model.EmergencyContactPhone;
        patient.UpdatedAt = DateTime.UtcNow;

        // Also update ApplicationUser if needed
        if (patient.UserId.HasValue)
        {
            var user = await _db.Users.FindAsync(patient.UserId.Value);
            if (user != null)
            {
                user.PhoneNumber = model.MobileNumber;
                user.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();

        TempData["Success"] = "Profile updated successfully!";
        return RedirectToAction(nameof(MyProfile));
    }


    public async Task<IActionResult> MyAppointments()
    {
        var patientId = await GetPatientIdAsync();
        var appointments = await _db.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.Department)
            .Where(a => a.PatientId == patientId && !a.IsDeleted)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new MyAppointmentItem
            {
                Id = a.Id,
                AppointmentNo = a.AppointmentNo,
                DoctorName = a.Doctor.FullName,
                DepartmentName = a.Doctor.Department.Name,
                Date = a.AppointmentDate,
                Status = a.Status
            }).ToListAsync();

        return View(appointments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var patientId = await GetPatientIdAsync();
        var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.PatientId == patientId && !a.IsDeleted);
        
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

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Appointment cancelled successfully.";
        return RedirectToAction(nameof(MyAppointments));
    }

    public async Task<IActionResult> MyPrescriptions()
    {
        var patientId = await GetPatientIdAsync();
        var prescriptions = await _db.Prescriptions
            .Include(p => p.Appointment)
            .Include(p => p.Doctor)
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(prescriptions);
    }

    [HttpGet]
    public async Task<IActionResult> BookAppointment()
    {
        ViewBag.Departments = await _db.Departments.Where(d => !d.IsDeleted).ToListAsync();
        ViewBag.Doctors = await _db.Doctors.Where(d => !d.IsDeleted && d.Status == DoctorStatus.Available).ToListAsync();
        return View(new BookAppointmentViewModel { AppointmentDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookAppointment(BookAppointmentViewModel model)
    {
        var patientId = await GetPatientIdAsync();
        if (patientId == 0) return RedirectToAction("Login", "Auth");

        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await _db.Departments.Where(d => !d.IsDeleted).ToListAsync();
            ViewBag.Doctors = await _db.Doctors.Where(d => !d.IsDeleted && d.Status == DoctorStatus.Available).ToListAsync();
            return View(model);
        }

        var count = await _db.Appointments.CountAsync() + 1;
        var appointment = new Appointment
        {
            AppointmentNo = $"APT-{DateTime.Today.Year}-{count:D4}",
            PatientId = patientId,
            DoctorId = model.DoctorId,
            AppointmentDate = model.AppointmentDate,
            Status = AppointmentStatus.Pending,
            ChiefComplaint = model.Symptoms,
            CreatedAt = DateTime.UtcNow
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Appointment booked successfully! Waiting for confirmation.";
        return RedirectToAction(nameof(MyAppointments));
    }
}
