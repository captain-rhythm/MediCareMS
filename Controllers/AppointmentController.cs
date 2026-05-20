using MediCareMS.Data;
using MediCareMS.Models.Entities.Appointment;
using MediCareMS.Models.Enums;
using MediCareMS.Models.ViewModels.Appointment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MediCareMS.Controllers;

[Authorize(Roles = "Super Admin,Hospital Admin,Doctor,Receptionist,Nurse")]
public class AppointmentController : Controller
{
    private readonly AppDbContext _db;

    public AppointmentController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search, AppointmentStatus? status, DateOnly? date)
    {
        var query = _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor).ThenInclude(d => d.Department)
            .Where(a => !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.AppointmentNo.Contains(search) || a.Patient.FullName.Contains(search) || a.Doctor.FullName.Contains(search));

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (date.HasValue)
            query = query.Where(a => a.AppointmentDate == date.Value);

        var appointments = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentListViewModel
            {
                Id = a.Id,
                AppointmentNo = a.AppointmentNo,
                PatientName = a.Patient.FullName,
                DoctorName = a.Doctor.FullName,
                Department = a.Doctor.Department.Name,
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime,
                TokenNumber = a.TokenNumber,
                Status = a.Status,
                ChiefComplaint = a.ChiefComplaint
            }).ToListAsync();

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.Date = date;
        return View(appointments);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(await BuildCreateViewModel(new AppointmentCreateViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(await BuildCreateViewModel(vm));

        var count = await _db.Appointments.CountAsync() + 1;
        var tokenCount = await _db.Appointments
            .CountAsync(a => a.DoctorId == vm.DoctorId && a.AppointmentDate == vm.AppointmentDate && !a.IsDeleted);

        var appt = new Appointment
        {
            AppointmentNo = $"APT-{DateTime.Today.Year}-{count:D4}",
            PatientId = vm.PatientId,
            DoctorId = vm.DoctorId,
            AppointmentDate = vm.AppointmentDate,
            AppointmentTime = vm.AppointmentTime,
            TokenNumber = tokenCount + 1,
            ChiefComplaint = vm.ChiefComplaint,
            Notes = vm.Notes,
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.Appointments.Add(appt);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Appointment {appt.AppointmentNo} booked. Token #{appt.TokenNumber}";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, AppointmentStatus status, string? reason)
    {
        var appt = await _db.Appointments.FindAsync(id);
        if (appt == null) return NotFound();

        appt.Status = status;
        appt.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(reason))
            appt.CancellationReason = reason;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Appointment status updated to {status}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var appt = await _db.Appointments.FindAsync(id);
        if (appt != null) { appt.IsDeleted = true; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Appointment deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<AppointmentCreateViewModel> BuildCreateViewModel(AppointmentCreateViewModel vm)
    {
        vm.Patients = await _db.Patients.Where(p => !p.IsDeleted)
            .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"{p.PatientNo} - {p.FullName}" }).ToListAsync();
        vm.Doctors = await _db.Doctors.Where(d => !d.IsDeleted)
            .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = $"{d.FullName} ({d.DoctorNo})" }).ToListAsync();
        return vm;
    }
}
