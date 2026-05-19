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
        {
            if (User.IsInRole("Patient"))
                return RedirectToAction("Dashboard", "User");
            return RedirectToAction("Dashboard", "Admin");
        }
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
                !u.IsDeleted &&
                (u.UserName == model.UserNameOrEmail || u.Email == model.UserNameOrEmail),
                cancellationToken);

        if (user == null || !_passwordHash.VerifyPassword(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        if (user.Status == Models.Enums.AccountStatus.Inactive)
        {
            ModelState.AddModelError(string.Empty, "Your account is pending administrator approval.");
            return View(model);
        }

        if (user.Status == Models.Enums.AccountStatus.Suspended)
        {
            ModelState.AddModelError(string.Empty, "Your account has been suspended. Please contact support.");
            return View(model);
        }

        if (!user.IsEmailConfirmed)
        {
            ModelState.AddModelError(string.Empty, "Please confirm your email address before logging in.");
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
    public async Task<IActionResult> Register(string token, string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            return RedirectToAction("Login");

        var invitation = await _db.Invitations
            .FirstOrDefaultAsync(i => i.Token == token && i.Email == email
                && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow, cancellationToken);

        if (invitation == null)
        {
            ViewBag.Error = "This invitation link is invalid or has expired. Please contact your administrator.";
            return View("InviteError");
        }

        return View(new InviteRegisterViewModel { Token = token, Email = email });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(InviteRegisterViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return View(model);

        var invitation = await _db.Invitations
            .FirstOrDefaultAsync(i => i.Token == model.Token && i.Email == model.Email
                && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow, cancellationToken);

        if (invitation == null)
        {
            ModelState.AddModelError("", "Invitation is invalid or has expired.");
            return View(model);
        }

        if (await _db.Users.AnyAsync(u => u.Email == model.Email, cancellationToken))
        {
            ModelState.AddModelError("", "This email is already registered.");
            return View(model);
        }

        // Build unique username from full name
        var baseUsername = model.FullName.ToLower().Replace(" ", ".");
        var username = baseUsername;
        var suffix = 1;
        while (await _db.Users.AnyAsync(u => u.UserName == username, cancellationToken))
            username = $"{baseUsername}{suffix++}";

        var user = new MediCareMS.Models.Entities.Auth.ApplicationUser
        {
            UserName = username,
            Email = model.Email,
            PasswordHash = _passwordHash.HashPassword(model.Password),
            PhoneNumber = model.Phone,
            Status = Models.Enums.AccountStatus.Inactive, // Awaits admin approval
            IsEmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        // Assign role by name
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == model.Role, cancellationToken);
        if (role != null)
        {
            _db.UserRoles.Add(new MediCareMS.Models.Entities.Auth.UserRole { UserId = user.Id, RoleId = role.Id });
        }

        // Update invitation
        invitation.IsUsed = true;
        invitation.Status = "Registered";
        invitation.RegisteredFullName = model.FullName;
        invitation.RegisteredPhone = model.Phone;
        invitation.RequestedRole = model.Role;

        await _db.SaveChangesAsync(cancellationToken);

        return View("RegisterSuccess");
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
        {
            if (User.IsInRole("Patient"))
                return RedirectToAction("Dashboard", "User");
            return RedirectToAction("Dashboard", "Admin");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignUp(SignUpViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid) return View(model);

        var emailExists = await _db.Users.AnyAsync(u => u.Email == model.Email, cancellationToken);
        if (emailExists)
        {
            ModelState.AddModelError("Email", "Email is already registered.");
            return View(model);
        }

        // Build a safe username from the full name
        var baseUsername = model.FullName.ToLower().Replace(" ", ".");
        var username = baseUsername;
        var suffix = 1;
        while (await _db.Users.AnyAsync(u => u.UserName == username, cancellationToken))
            username = $"{baseUsername}{suffix++}";

        var user = new MediCareMS.Models.Entities.Auth.ApplicationUser
        {
            UserName = username,
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
            MobileNumber = model.MobileNumber,
            Gender = model.Gender,
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
