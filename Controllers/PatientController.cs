using MediCareMS.Data;
using MediCareMS.Helpers;
using MediCareMS.Models.Entities.Patient;
using MediCareMS.Models.Enums;
using MediCareMS.Models.ViewModels.Patient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MediCareMS.Controllers;

[Authorize(Roles = "Super Admin,Hospital Admin,Doctor,Receptionist,Nurse")]
public class PatientController : Controller
{
    private readonly AppDbContext    _db;
    private readonly IMemoryCache    _cache;

    // Cache key prefix and duration
    private const string CacheKeyPrefix    = "patient_count";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

    public PatientController(AppDbContext db, IMemoryCache cache)
    {
        _db    = db;
        _cache = cache;
    }

    // ── Index: returns view immediately — data loads via JS ──────────────────
    public IActionResult Index() => View();

    // ── Server-side paged list API ───────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetPagedList([FromQuery] PatientPageRequest req)
    {
        // Clamp page size to sensible bounds
        req.PageSize = Math.Clamp(req.PageSize, 5, 100);
        req.Page     = Math.Max(1, req.Page);

        var search     = (req.Search     ?? "").Trim().ToLower();
        var bloodGroup = (req.BloodGroup ?? "").Trim();

        // ── Build base IQueryable (fully translated to SQL) ──────────────────
        var query = _db.Patients
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        // Search filter: patient no, full name, or mobile
        if (!string.IsNullOrEmpty(search))
            query = query.Where(p =>
                p.FullName.ToLower().Contains(search)      ||
                p.PatientNo.ToLower().Contains(search)     ||
                (p.MobileNumber != null && p.MobileNumber.Contains(search)));

        // Blood-group filter: map display string → enum value
        if (!string.IsNullOrEmpty(bloodGroup))
        {
            // Convert "A Positive" → "A_Positive" → BloodGroup enum
            var enumName = bloodGroup.Replace(" ", "_");
            if (Enum.TryParse<BloodGroup>(enumName, out var bgEnum))
                query = query.Where(p => p.BloodGroup == bgEnum);
        }

        // ── Run COUNT and PAGE queries in parallel ────────────────────────────
        var countTask = query.CountAsync();

        // ── Apply sort ───────────────────────────────────────────────────────
        query = (req.SortField?.ToLower(), req.SortDir?.ToLower()) switch
        {
            ("patientno",   "desc") => query.OrderByDescending(p => p.PatientNo),
            ("patientno",   _     ) => query.OrderBy(p => p.PatientNo),
            ("mobile",      "desc") => query.OrderByDescending(p => p.MobileNumber),
            ("mobile",      _     ) => query.OrderBy(p => p.MobileNumber),
            ("totalvisits", "desc") => query.OrderByDescending(p =>
                                            p.Appointments.Count(a => !a.IsDeleted)),
            ("totalvisits", _     ) => query.OrderBy(p =>
                                            p.Appointments.Count(a => !a.IsDeleted)),
            ("fullname",    "desc") => query.OrderByDescending(p => p.FullName),
            _                       => query.OrderBy(p => p.FullName),
        };

        // Projection — only the columns the table needs, age computed in SQL
        var today = DateTime.Today;
        var pageQuery = query
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(p => new
            {
                p.Id,
                p.PatientNo,
                p.FullName,
                p.MobileNumber,
                p.Gender,
                p.BloodGroup,
                p.DateOfBirth,
                TotalVisits = p.Appointments.Count(a => !a.IsDeleted)
            })
            .ToListAsync();

        // Await both tasks together
        await Task.WhenAll(countTask, pageQuery);

        var total = await countTask;
        var raw   = await pageQuery;

        // Age computed in C# after fetch (EF Core 8 + SQL Server safe)
        var rows = raw.Select(p => new PatientRowDto
        {
            Id          = p.Id,
            PatientNo   = p.PatientNo,
            FullName    = p.FullName,
            Mobile      = p.MobileNumber ?? "",
            Age         = (int)((today - p.DateOfBirth).TotalDays / 365.25),
            AgeGender   = $"{(int)((today - p.DateOfBirth).TotalDays / 365.25)} yrs / {p.Gender}",
            BloodGroup  = p.BloodGroup.HasValue
                            ? p.BloodGroup.Value.ToString().Replace("_", " ")
                            : "",
            TotalVisits = p.TotalVisits,
            DetailUrl   = Url.Action("Details", "Patient", new { id = p.Id }) ?? "#",
            EditUrl     = Url.Action("Edit",    "Patient", new { id = p.Id }) ?? "#",
            DeleteUrl   = Url.Action("Delete",  "Patient", new { id = p.Id }) ?? "#"
        }).ToList();

        // Cache total count (used by skeleton loader on next visit)
        _cache.Set(CacheKeyPrefix, total, new MemoryCacheEntryOptions
        {
            SlidingExpiration = CacheDuration
        });

        return Json(new PatientPageResponse
        {
            Data     = rows,
            Total    = total,
            Page     = req.Page,
            PageSize = req.PageSize
        });
    }

    // ── Cached total count (skeleton hint) ──────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetCount()
    {
        if (!_cache.TryGetValue(CacheKeyPrefix, out int count))
        {
            count = await _db.Patients.AsNoTracking().CountAsync(p => !p.IsDeleted);
            _cache.Set(CacheKeyPrefix, count, new MemoryCacheEntryOptions
            {
                SlidingExpiration = CacheDuration
            });
        }
        return Json(new { count });
    }

    // ── Legacy full list (kept for backward compatibility) ───────────────────
    [HttpGet]
    public async Task<IActionResult> GetList()
    {
        var raw = await _db.Patients
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.FullName)
            .Select(p => new
            {
                p.Id, p.PatientNo, p.FullName, p.MobileNumber,
                p.Gender, p.BloodGroup, p.DateOfBirth,
                TotalVisits = p.Appointments.Count(a => !a.IsDeleted)
            })
            .ToListAsync();

        var today = DateTime.Today;
        var patients = raw.Select(p => new
        {
            id          = p.Id,
            patientNo   = p.PatientNo,
            fullName    = p.FullName,
            mobile      = p.MobileNumber ?? "",
            ageGender   = $"{(int)((today - p.DateOfBirth).TotalDays / 365.25)} yrs / {p.Gender}",
            age         = (int)((today - p.DateOfBirth).TotalDays / 365.25),
            bloodGroup  = p.BloodGroup.HasValue
                            ? p.BloodGroup.Value.ToString().Replace("_", " ")
                            : "",
            totalVisits = p.TotalVisits,
            detailUrl   = Url.Action("Details", "Patient", new { id = p.Id }),
            editUrl     = Url.Action("Edit",    "Patient", new { id = p.Id }),
            deleteUrl   = Url.Action("Delete",  "Patient", new { id = p.Id })
        }).ToList();

        return Json(patients);
    }

    // ── CRUD ─────────────────────────────────────────────────────────────────
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
            PatientNo                = $"PAT-{DateTime.Today.Year}-{count:D4}",
            FullName                 = vm.FullName,
            DateOfBirth              = vm.DateOfBirth,
            Gender                   = vm.Gender,
            BloodGroup               = vm.BloodGroup,
            MobileNumber             = vm.MobileNumber,
            Email                    = vm.Email,
            Address                  = vm.Address,
            Nationality              = vm.Nationality,
            EmergencyContactName     = vm.EmergencyContactName,
            EmergencyContactPhone    = vm.EmergencyContactPhone,
            EmergencyContactRelation = vm.EmergencyContactRelation,
            KnownAllergies           = vm.KnownAllergies,
            ChronicConditions        = vm.ChronicConditions,
            CreatedAt                = DateTime.UtcNow
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();
        _cache.Remove(CacheKeyPrefix);          // Invalidate count cache
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
            Id                       = p.Id,
            FullName                 = p.FullName,
            DateOfBirth              = p.DateOfBirth,
            Gender                   = p.Gender,
            BloodGroup               = p.BloodGroup,
            MobileNumber             = p.MobileNumber,
            Email                    = p.Email,
            Address                  = p.Address,
            Nationality              = p.Nationality,
            EmergencyContactName     = p.EmergencyContactName,
            EmergencyContactPhone    = p.EmergencyContactPhone,
            EmergencyContactRelation = p.EmergencyContactRelation,
            KnownAllergies           = p.KnownAllergies,
            ChronicConditions        = p.ChronicConditions
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PatientCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var p = await _db.Patients.FindAsync(vm.Id);
        if (p == null) return NotFound();

        p.FullName                 = vm.FullName;
        p.DateOfBirth              = vm.DateOfBirth;
        p.Gender                   = vm.Gender;
        p.BloodGroup               = vm.BloodGroup;
        p.MobileNumber             = vm.MobileNumber;
        p.Email                    = vm.Email;
        p.Address                  = vm.Address;
        p.Nationality              = vm.Nationality;
        p.EmergencyContactName     = vm.EmergencyContactName;
        p.EmergencyContactPhone    = vm.EmergencyContactPhone;
        p.EmergencyContactRelation = vm.EmergencyContactRelation;
        p.KnownAllergies           = vm.KnownAllergies;
        p.ChronicConditions        = vm.ChronicConditions;
        p.UpdatedAt                = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _cache.Remove(CacheKeyPrefix);          // Invalidate count cache
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
            Id                  = p.Id,
            PatientNo           = p.PatientNo,
            FullName            = p.FullName,
            DateOfBirth         = p.DateOfBirth,
            Gender              = p.Gender,
            BloodGroup          = p.BloodGroup,
            MobileNumber        = p.MobileNumber,
            Address             = p.Address,
            KnownAllergies      = p.KnownAllergies,
            ChronicConditions   = p.ChronicConditions,
            RecentAppointments  = p.Appointments
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(5)
                .Select(a => new AppointmentSummary
                {
                    AppointmentNo = a.AppointmentNo,
                    DoctorName    = a.Doctor.FullName,
                    Date          = a.AppointmentDate,
                    Status        = a.Status
                }).ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Patients.FindAsync(id);
        if (p != null)
        {
            p.IsDeleted = true;
            await _db.SaveChangesAsync();
            _cache.Remove(CacheKeyPrefix);      // Invalidate count cache
        }
        TempData["Success"] = "Patient removed.";
        return RedirectToAction(nameof(Index));
    }
}
