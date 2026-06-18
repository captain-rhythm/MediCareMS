using MediCareMS.Data;
using MediCareMS.Models.Entities.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCareMS.Controllers;

[Authorize(Roles = "Super Admin,Hospital Admin")]
public class SettingsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public SettingsController(AppDbContext db, IConfiguration config, IWebHostEnvironment env)
    {
        _db = db; _config = config; _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var profile = await _db.HospitalProfiles.FirstOrDefaultAsync()
                      ?? new HospitalProfile { Name = "MediCare Hospital", CreatedAt = DateTime.UtcNow };

        ViewBag.EmailConfig  = _config.GetSection("Email");
        ViewBag.SslConfig    = _config.GetSection("SslCommerz");
        ViewBag.GroqConfig   = _config.GetSection("Groq");
        ViewBag.GeminiConfig = _config.GetSection("Gemini");
        ViewBag.Environment  = _env.EnvironmentName;
        ViewBag.AppVersion   = typeof(SettingsController).Assembly.GetName().Version?.ToString() ?? "1.0.0";

        // Stats
        ViewBag.TotalDoctors      = await _db.Doctors.CountAsync(d => !d.IsDeleted);
        ViewBag.TotalPatients     = await _db.Patients.CountAsync(p => !p.IsDeleted);
        ViewBag.TotalDepartments  = await _db.Departments.CountAsync(d => !d.IsDeleted);
        ViewBag.TotalAppointments = await _db.Appointments.CountAsync();

        return View(profile);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveHospital(HospitalProfile model)
    {
        var existing = await _db.HospitalProfiles.FirstOrDefaultAsync();
        if (existing == null)
        {
            model.CreatedAt = DateTime.UtcNow;
            _db.HospitalProfiles.Add(model);
        }
        else
        {
            existing.Name      = model.Name;
            existing.Address   = model.Address;
            existing.Phone     = model.Phone;
            existing.Email     = model.Email;
            existing.Website   = model.Website;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Hospital profile saved successfully.";
        return RedirectToAction(nameof(Index));
    }
}
