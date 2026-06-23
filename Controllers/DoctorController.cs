using MediCareMS.Data;
using MediCareMS.Helpers;
using MediCareMS.Models.Entities.Doctor;
using MediCareMS.Models.ViewModels.Doctor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MediCareMS.Helpers.Security;

namespace MediCareMS.Controllers;

[Authorize(Roles = "Super Admin,Hospital Admin,Doctor,Receptionist,Nurse")]
public class DoctorController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IPasswordHashService _passwordHash;

    public DoctorController(AppDbContext db, IWebHostEnvironment env, IPasswordHashService passwordHash)
    {
        _db = db;
        _env = env;
        _passwordHash = passwordHash;
    }

    public async Task<IActionResult> Index()
    {
        var doctors = await _db.Doctors
            .Include(d => d.Department)
            .Include(d => d.Specialization)
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.FullName)
            .Select(d => new
            {
                id             = d.Id,
                doctorNo       = d.DoctorNo,
                fullName       = d.FullName,
                department     = d.Department != null ? d.Department.Name : "",
                specialization = d.Specialization != null ? d.Specialization.Name : "",
                qualification  = d.Qualification ?? "",
                experience     = d.ExperienceYears ?? 0,
                fee            = d.ConsultationFee,
                status         = d.Status.ToString(),
                editUrl        = Url.Action("Edit",   "Doctor", new { id = d.Id }),
                deleteUrl      = Url.Action("Delete", "Doctor", new { id = d.Id })
            })
            .ToListAsync();

        var opts = new System.Text.Json.JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        ViewBag.DoctorJson = System.Text.Json.JsonSerializer.Serialize(doctors, opts);
        return View();
    }


    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var doctors = await _db.Doctors
            .Include(d => d.Department)
            .Include(d => d.Specialization)
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.FullName)
            .Select(d => new
            {
                id             = d.Id,
                doctorNo       = d.DoctorNo,
                fullName       = d.FullName,
                department     = d.Department != null ? d.Department.Name : "",
                specialization = d.Specialization != null ? d.Specialization.Name : "",
                qualification  = d.Qualification ?? "",
                experience     = d.ExperienceYears ?? 0,
                fee            = d.ConsultationFee,
                status         = d.Status.ToString(),
                editUrl        = Url.Action("Edit",   "Doctor", new { id = d.Id }),
                deleteUrl      = Url.Action("Delete", "Doctor", new { id = d.Id })
            })
            .ToListAsync();

        return Json(doctors);
    }


    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View(await BuildViewModel(new DoctorCreateEditViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DoctorCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(await BuildViewModel(vm));

        var count = await _db.Doctors.CountAsync() + 1;
        var doctor = new DoctorProfile
        {
            DoctorNo = $"DOC-{count:D3}",
            FullName = vm.FullName,
            DepartmentId = vm.DepartmentId,
            SpecializationId = vm.SpecializationId,
            Qualification = vm.Qualification,
            ExperienceYears = vm.ExperienceYears,
            BmdcRegNo = vm.BmdcRegNo,
            MobileNumber = vm.MobileNumber,
            Email = vm.Email,
            ConsultationFee = vm.ConsultationFee,
            ChamberAddress = vm.ChamberAddress,
            Bio = vm.Bio,
            Status = vm.Status,
            PasswordHash = _passwordHash.HashPassword("123"),
            CreatedAt = DateTime.UtcNow
        };

        if (vm.ProfileImage != null)
            doctor.ProfileImagePath = await SaveFile(vm.ProfileImage);

        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Doctor added successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var doctor = await _db.Doctors.FindAsync(id);
        if (doctor == null) return NotFound();

        var vm = new DoctorCreateEditViewModel
        {
            Id = doctor.Id,
            FullName = doctor.FullName,
            DepartmentId = doctor.DepartmentId,
            SpecializationId = doctor.SpecializationId,
            Qualification = doctor.Qualification,
            ExperienceYears = doctor.ExperienceYears,
            BmdcRegNo = doctor.BmdcRegNo,
            MobileNumber = doctor.MobileNumber,
            Email = doctor.Email,
            ConsultationFee = doctor.ConsultationFee,
            ChamberAddress = doctor.ChamberAddress,
            Bio = doctor.Bio,
            Status = doctor.Status,
            ExistingProfileImagePath = doctor.ProfileImagePath
        };
        return View(await BuildViewModel(vm));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DoctorCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(await BuildViewModel(vm));

        var doctor = await _db.Doctors.FindAsync(vm.Id);
        if (doctor == null) return NotFound();

        doctor.FullName = vm.FullName;
        doctor.DepartmentId = vm.DepartmentId;
        doctor.SpecializationId = vm.SpecializationId;
        doctor.Qualification = vm.Qualification;
        doctor.ExperienceYears = vm.ExperienceYears;
        doctor.BmdcRegNo = vm.BmdcRegNo;
        doctor.MobileNumber = vm.MobileNumber;
        doctor.Email = vm.Email;
        doctor.ConsultationFee = vm.ConsultationFee;
        doctor.ChamberAddress = vm.ChamberAddress;
        doctor.Bio = vm.Bio;
        doctor.Status = vm.Status;
        doctor.UpdatedAt = DateTime.UtcNow;

        if (vm.ProfileImage != null)
            doctor.ProfileImagePath = await SaveFile(vm.ProfileImage);

        await _db.SaveChangesAsync();
        TempData["Success"] = "Doctor updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var doctor = await _db.Doctors.FindAsync(id);
        if (doctor != null) { doctor.IsDeleted = true; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Doctor removed.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<DoctorCreateEditViewModel> BuildViewModel(DoctorCreateEditViewModel vm)
    {
        vm.Departments = await _db.Departments.Where(d => d.IsActive)
            .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToListAsync();
        vm.Specializations = await _db.Specializations
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToListAsync();
        return vm;
    }

    private async Task<string> SaveFile(IFormFile file)
    {
        var uploads = Path.Combine(_env.WebRootPath, "uploads", "doctors");
        Directory.CreateDirectory(uploads);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var path = Path.Combine(uploads, fileName);
        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream);
        return $"/uploads/doctors/{fileName}";
    }

    [HttpGet]
    [Authorize(Roles = "Doctor")]
    public IActionResult Portal()
    {
        return View();
    }

    [HttpGet]
    [Authorize(Roles = "Doctor")]
    public IActionResult Appointments()
    {
        return View();
    }

    [HttpGet]
    [Authorize(Roles = "Doctor")]
    public IActionResult Patients()
    {
        return View();
    }
}
