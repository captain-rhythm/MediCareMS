using MediCareMS.Data;
using MediCareMS.Models.Enums;
using MediCareMS.Models.ViewModels.Appointment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCareMS.Controllers;

[Authorize(Roles = "Super Admin,Hospital Admin,Doctor,Receptionist,Nurse")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Dashboard()
    {
        var todayStart = DateTime.Today;
        var tomorrowStart = todayStart.AddDays(1);
        var today = DateOnly.FromDateTime(todayStart);
        var monthStart = DateOnly.FromDateTime(new DateTime(todayStart.Year, todayStart.Month, 1));

        var vm = new DashboardViewModel
        {
            TotalPatients = await _db.Patients.CountAsync(p => !p.IsDeleted),
            TotalDoctors = await _db.Doctors.CountAsync(d => !d.IsDeleted),
            TodayAppointments = await _db.Appointments.CountAsync(a => a.AppointmentDate == today && !a.IsDeleted),
            PendingAppointments = await _db.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending && !a.IsDeleted),
            CompletedAppointments = await _db.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed && !a.IsDeleted),
            TodayRevenue = (decimal)(await _db.Invoices.Where(i => i.CreatedAt >= todayStart && i.CreatedAt < tomorrowStart && !i.IsDeleted).SumAsync(i => (double?)i.PaidAmount) ?? 0),
            MonthRevenue = (decimal)(await _db.Invoices.Where(i => i.DueDate >= monthStart && !i.IsDeleted).SumAsync(i => (double?)i.PaidAmount) ?? 0),
            PendingRegistrationRequests = await _db.Invitations.CountAsync(i => i.Status == "Registered"),

            RecentAppointments = await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.CreatedAt)
                .Take(8)
                .Select(a => new RecentAppointmentItem
                {
                    AppointmentNo = a.AppointmentNo,
                    PatientName = a.Patient.FullName,
                    DoctorName = a.Doctor.FullName,
                    Date = a.AppointmentDate,
                    Status = a.Status
                }).ToListAsync(),

            AvailableDoctors = await _db.Doctors
                .Include(d => d.Department)
                .Where(d => !d.IsDeleted && d.Status == DoctorStatus.Available)
                .OrderBy(d => d.FullName)
                .Take(6)
                .Select(d => new DoctorAvailabilityItem
                {
                    DoctorName = d.FullName,
                    Department = d.Department.Name,
                    Status = d.Status,
                    ConsultationFee = d.ConsultationFee
                }).ToListAsync()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> PendingAppointments()
    {
        var pending = await _db.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.Status == AppointmentStatus.Pending && !a.IsDeleted)
            .OrderBy(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime)
            .ToListAsync();
        
        return View(pending);
    }

    [HttpPost]
    public async Task<IActionResult> ApproveAppointment(int id)
    {
        var appt = await _db.Appointments.FindAsync(id);
        if (appt != null && appt.Status == AppointmentStatus.Pending)
        {
            appt.Status = AppointmentStatus.Confirmed;
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Appointment #{appt.AppointmentNo} confirmed.";
        }
        return RedirectToAction(nameof(PendingAppointments));
    }
}

