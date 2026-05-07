using MediCareMS.Data;
using MediCareMS.Models.Enums;
using MediCareMS.Models.ViewModels.Appointment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCareMS.Controllers;

[Authorize(Roles = "Super Admin,Hospital Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Dashboard()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var monthStart = DateOnly.FromDateTime(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));

        var vm = new DashboardViewModel
        {
            TotalPatients = await _db.Patients.CountAsync(p => !p.IsDeleted),
            TotalDoctors = await _db.Doctors.CountAsync(d => !d.IsDeleted),
            TodayAppointments = await _db.Appointments.CountAsync(a => a.AppointmentDate == today && !a.IsDeleted),
            PendingAppointments = await _db.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending && !a.IsDeleted),
            CompletedAppointments = await _db.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed && !a.IsDeleted),
            TodayRevenue = await _db.Invoices.Where(i => i.CreatedAt.Date == DateTime.Today && !i.IsDeleted).SumAsync(i => (decimal?)i.PaidAmount) ?? 0,
            MonthRevenue = await _db.Invoices.Where(i => i.DueDate >= monthStart && !i.IsDeleted).SumAsync(i => (decimal?)i.PaidAmount) ?? 0,

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
}
