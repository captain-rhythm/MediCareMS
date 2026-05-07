using System.Security.Claims;
using MediCareMS.Data;
using MediCareMS.Helpers.Security;
using MediCareMS.Models.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCareMS.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHashService _passwordHash;

    public AuthController(AppDbContext db, IPasswordHashService passwordHash)
    {
        _db = db;
        _passwordHash = passwordHash;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u =>
                u.Status == Models.Enums.AccountStatus.Active &&
                u.IsEmailConfirmed &&
                !u.IsDeleted &&
                (u.UserName == model.UserNameOrEmail || u.Email == model.UserNameOrEmail),
                cancellationToken);

        if (user == null || !_passwordHash.VerifyPassword(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props = new AuthenticationProperties { IsPersistent = model.RememberMe };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        if (roles.Contains("Super Admin") || roles.Contains("Hospital Admin") || roles.Contains("Doctor") || roles.Contains("Receptionist") || roles.Contains("Nurse"))
        {
            return RedirectToAction("Dashboard", "Admin");
        }
        
        return RedirectToAction("Dashboard", "User");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult SignUp()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignUp(SignUpViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return View(model);

        var existingUser = await _db.Users.AnyAsync(u => u.Email == model.Email || u.UserName == model.Email, cancellationToken);
        if (existingUser)
        {
            ModelState.AddModelError("Email", "Email is already registered.");
            return View(model);
        }

        var user = new MediCareMS.Models.Entities.Auth.ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            PasswordHash = _passwordHash.HashPassword(model.Password),
            Status = Models.Enums.AccountStatus.Active,
            IsEmailConfirmed = true, // Auto-confirm for demo
            LastLoginAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        // Add Patient Role (RoleId = 6)
        _db.UserRoles.Add(new MediCareMS.Models.Entities.Auth.UserRole { UserId = user.Id, RoleId = 6 });

        // Add basic patient profile
        var count = await _db.Patients.CountAsync(cancellationToken) + 1;
        _db.Patients.Add(new MediCareMS.Models.Entities.Patient.Patient
        {
            PatientNo = $"PAT-{DateTime.Today.Year}-{count:D4}",
            UserId = user.Id,
            FullName = model.FullName,
            Email = model.Email,
            CreatedAt = DateTime.UtcNow,
            DateOfBirth = DateTime.Today.AddYears(-20) // Default arbitrary
        });

        await _db.SaveChangesAsync(cancellationToken);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, "Patient")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props = new AuthenticationProperties { IsPersistent = false };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

        TempData["Success"] = "Account created successfully. Welcome!";
        return RedirectToAction("Dashboard", "User");
    }
}
