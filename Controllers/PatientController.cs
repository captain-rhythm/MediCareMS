using MediCareMS.Data;
using MediCareMS.Helpers;
using MediCareMS.Models.Entities.Patient;
using MediCareMS.Models.ViewModels.Patient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCareMS.Controllers;

[Authorize(Roles = "Super Admin,Hospital Admin,Doctor,Receptionist,Nurse")]
public class PatientController : Controller
{
    private readonly AppDbContext _db;

    public PatientController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        const int pageSize = 10;
        var query = _db.Patients.Where(p => !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.FullName.Contains(search) || p.PatientNo.Contains(search) || p.MobileNumber!.Contains(search));

        var projected = query
            .OrderBy(p => p.FullName)
            .Select(p => new PatientListViewModel
            {
                Id = p.Id,
                PatientNo = p.PatientNo,
                FullName = p.FullName,
                MobileNumber = p.MobileNumber,
                Gender = p.Gender,
                BloodGroup = p.BloodGroup,
                Age = (int)((DateTime.Today - p.DateOfBirth).TotalDays / 365.25),
                TotalVisits = p.Appointments.Count(a => !a.IsDeleted)
            });

        var paged = await PaginatedList<PatientListViewModel>.CreateAsync(projected, page, pageSize);

        ViewBag.Search = search;
        ViewData["PageIndex"]  = paged.PageIndex;
        ViewData["TotalPages"] = paged.TotalPages;
        ViewData["TotalCount"] = paged.TotalCount;
        ViewData["PageSize"]   = paged.PageSize;
        ViewData["PagAction"]  = "Index";
        ViewData["PagSearch"]  = search;
        return View(paged.Items);
    }

    [HttpGet]
    public IActionResult Create() => View(new PatientCreateEditViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PatientCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var count = await _db.Patients.CountAsync() + 1;
        var patient = new Patient
        {
            PatientNo = $"PAT-{DateTime.Today.Year}-{count:D4}",
            FullName = vm.FullName,
            DateOfBirth = vm.DateOfBirth,
            Gender = vm.Gender,
            BloodGroup = vm.BloodGroup,
            MobileNumber = vm.MobileNumber,
            Email = vm.Email,
            Address = vm.Address,
            Nationality = vm.Nationality,
            EmergencyContactName = vm.EmergencyContactName,
            EmergencyContactPhone = vm.EmergencyContactPhone,
            EmergencyContactRelation = vm.EmergencyContactRelation,
            KnownAllergies = vm.KnownAllergies,
            ChronicConditions = vm.ChronicConditions,
            CreatedAt = DateTime.UtcNow
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Patient registered successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var p = await _db.Patients.FindAsync(id);
        if (p == null) return NotFound();

        return View(new PatientCreateEditViewModel
        {
            Id = p.Id,
            FullName = p.FullName,
            DateOfBirth = p.DateOfBirth,
            Gender = p.Gender,
            BloodGroup = p.BloodGroup,
            MobileNumber = p.MobileNumber,
            Email = p.Email,
            Address = p.Address,
            Nationality = p.Nationality,
            EmergencyContactName = p.EmergencyContactName,
            EmergencyContactPhone = p.EmergencyContactPhone,
            EmergencyContactRelation = p.EmergencyContactRelation,
            KnownAllergies = p.KnownAllergies,
            ChronicConditions = p.ChronicConditions
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PatientCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var p = await _db.Patients.FindAsync(vm.Id);
        if (p == null) return NotFound();

        p.FullName = vm.FullName;
        p.DateOfBirth = vm.DateOfBirth;
        p.Gender = vm.Gender;
        p.BloodGroup = vm.BloodGroup;
        p.MobileNumber = vm.MobileNumber;
        p.Email = vm.Email;
        p.Address = vm.Address;
        p.Nationality = vm.Nationality;
        p.EmergencyContactName = vm.EmergencyContactName;
        p.EmergencyContactPhone = vm.EmergencyContactPhone;
        p.EmergencyContactRelation = vm.EmergencyContactRelation;
        p.KnownAllergies = vm.KnownAllergies;
        p.ChronicConditions = vm.ChronicConditions;
        p.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        TempData["Success"] = "Patient updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var p = await _db.Patients
            .Include(x => x.Appointments).ThenInclude(a => a.Doctor)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (p == null) return NotFound();

        return View(new PatientDetailViewModel
        {
            Id = p.Id,
            PatientNo = p.PatientNo,
            FullName = p.FullName,
            DateOfBirth = p.DateOfBirth,
            Gender = p.Gender,
            BloodGroup = p.BloodGroup,
            MobileNumber = p.MobileNumber,
            Address = p.Address,
            KnownAllergies = p.KnownAllergies,
            ChronicConditions = p.ChronicConditions,
            RecentAppointments = p.Appointments
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(5)
                .Select(a => new AppointmentSummary
                {
                    AppointmentNo = a.AppointmentNo,
                    DoctorName = a.Doctor.FullName,
                    Date = a.AppointmentDate,
                    Status = a.Status
                }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Patients.FindAsync(id);
        if (p != null) { p.IsDeleted = true; await _db.SaveChangesAsync(); }
        TempData["Success"] = "Patient removed.";
        return RedirectToAction(nameof(Index));
    }
}
