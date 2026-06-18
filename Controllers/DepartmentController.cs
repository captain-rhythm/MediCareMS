using MediCareMS.Data;
using MediCareMS.Models.Entities.Doctor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCareMS.Controllers;

[Authorize(Roles = "Super Admin,Hospital Admin")]
public class DepartmentController : Controller
{
    private readonly AppDbContext _db;

    public DepartmentController(AppDbContext db) => _db = db;

    // ── LIST ──────────────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(string? search)
    {
        var query = _db.Departments
            .Include(d => d.Doctors.Where(doc => !doc.IsDeleted))
            .Where(d => !d.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.Name.Contains(search) || (d.Description != null && d.Description.Contains(search)));

        var list = await query.OrderBy(d => d.Name).ToListAsync();
        ViewBag.Search = search;
        return View(list);
    }

    // ── CREATE GET ────────────────────────────────────────────────────────────
    public IActionResult Create() => View(new Department());

    // ── CREATE POST ───────────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Department dept)
    {
        if (!ModelState.IsValid) return View(dept);

        // Check duplicate name
        if (await _db.Departments.AnyAsync(d => d.Name == dept.Name && !d.IsDeleted))
        {
            ModelState.AddModelError("Name", "A department with this name already exists.");
            return View(dept);
        }

        dept.CreatedAt = DateTime.UtcNow;
        _db.Departments.Add(dept);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Department '{dept.Name}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── EDIT GET ──────────────────────────────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var dept = await _db.Departments.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        if (dept == null) return NotFound();
        return View(dept);
    }

    // ── EDIT POST ─────────────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Department dept)
    {
        if (id != dept.Id) return BadRequest();
        if (!ModelState.IsValid) return View(dept);

        var existing = await _db.Departments.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        if (existing == null) return NotFound();

        // Check duplicate name (exclude self)
        if (await _db.Departments.AnyAsync(d => d.Name == dept.Name && d.Id != id && !d.IsDeleted))
        {
            ModelState.AddModelError("Name", "A department with this name already exists.");
            return View(dept);
        }

        existing.Name        = dept.Name;
        existing.Description = dept.Description;
        existing.IsActive    = dept.IsActive;
        existing.UpdatedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Department '{existing.Name}' updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── DETAILS ───────────────────────────────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var dept = await _db.Departments
            .Include(d => d.Doctors.Where(doc => !doc.IsDeleted))
                .ThenInclude(doc => doc.Specialization)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        if (dept == null) return NotFound();
        return View(dept);
    }

    // ── DELETE (soft) ─────────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var dept = await _db.Departments
            .Include(d => d.Doctors.Where(doc => !doc.IsDeleted))
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        if (dept == null) return NotFound();

        if (dept.Doctors.Any())
        {
            TempData["Error"] = $"Cannot delete '{dept.Name}' — it has {dept.Doctors.Count} active doctor(s) assigned.";
            return RedirectToAction(nameof(Index));
        }

        dept.IsDeleted  = true;
        dept.UpdatedAt  = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Department '{dept.Name}' deleted.";
        return RedirectToAction(nameof(Index));
    }

    // ── TOGGLE STATUS ─────────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var dept = await _db.Departments.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        if (dept == null) return NotFound();
        dept.IsActive  = !dept.IsActive;
        dept.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Department '{dept.Name}' is now {(dept.IsActive ? "Active" : "Inactive")}.";
        return RedirectToAction(nameof(Index));
    }
}
