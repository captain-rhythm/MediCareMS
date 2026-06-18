using System.Security.Claims;
using MediCareMS.Data;
using MediCareMS.Helpers.Security;
using MediCareMS.Helpers.Email;
using MediCareMS.Models.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCareMS.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHashService _passwordHash;
    private readonly IEmailService _emailService;

    public AuthController(AppDbContext db, IPasswordHashService passwordHash, IEmailService emailService)
    {
        _db = db;
        _passwordHash = passwordHash;
        _emailService = emailService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Patient"))
                return RedirectToAction("MyAppointments", "User");
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
        
        return RedirectToAction("MyAppointments", "User");
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
                return RedirectToAction("MyAppointments", "User");
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
        return RedirectToAction("MyAppointments", "User");
    }

    [HttpGet]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleCallback") };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public async Task<IActionResult> GoogleCallback(CancellationToken cancellationToken = default)
    {
        var result = await HttpContext.AuthenticateAsync("ExternalCookie");
        if (!result.Succeeded || result.Principal == null)
        {
            TempData["Error"] = "Google authentication failed.";
            return RedirectToAction("Login");
        }

        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var fullName = result.Principal.FindFirstValue(ClaimTypes.Name) ?? result.Principal.FindFirstValue("name") ?? "Google User";

        if (string.IsNullOrEmpty(email))
        {
            TempData["Error"] = "Email claim not found from Google login.";
            return RedirectToAction("Login");
        }

        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => !u.IsDeleted && u.Email == email, cancellationToken);

        if (user == null)
        {
            var baseUsername = fullName.ToLower().Replace(" ", ".");
            baseUsername = System.Text.RegularExpressions.Regex.Replace(baseUsername, @"[^a-z0-9\.]", "");
            if (string.IsNullOrWhiteSpace(baseUsername))
            {
                baseUsername = "google.user";
            }
            var username = baseUsername;
            var suffix = 1;
            while (await _db.Users.AnyAsync(u => u.UserName == username, cancellationToken))
                username = $"{baseUsername}{suffix++}";

            user = new MediCareMS.Models.Entities.Auth.ApplicationUser
            {
                UserName = username,
                Email = email,
                PasswordHash = Guid.NewGuid().ToString("N"),
                Status = Models.Enums.AccountStatus.Active,
                IsEmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);

            _db.UserRoles.Add(new MediCareMS.Models.Entities.Auth.UserRole { UserId = user.Id, RoleId = 6 });

            var count = await _db.Patients.CountAsync(cancellationToken) + 1;
            _db.Patients.Add(new MediCareMS.Models.Entities.Patient.Patient
            {
                PatientNo = $"PAT-{DateTime.Today.Year}-{count:D4}",
                UserId = user.Id,
                FullName = fullName,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                DateOfBirth = DateTime.Today.AddYears(-20)
            });

            await _db.SaveChangesAsync(cancellationToken);

            user = await _db.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);
        }

        if (user!.Status == Models.Enums.AccountStatus.Inactive)
        {
            TempData["Error"] = "Your account is pending administrator approval.";
            return RedirectToAction("Login");
        }
        if (user.Status == Models.Enums.AccountStatus.Suspended)
        {
            TempData["Error"] = "Your account has been suspended. Please contact support.";
            return RedirectToAction("Login");
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
        var props = new AuthenticationProperties { IsPersistent = false };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
        await HttpContext.SignOutAsync("ExternalCookie");

        TempData["Success"] = "Welcome back, " + fullName;

        if (roles.Contains("Super Admin") || roles.Contains("Hospital Admin") || roles.Contains("Doctor") || roles.Contains("Receptionist") || roles.Contains("Nurse"))
        {
            return RedirectToAction("Dashboard", "Admin");
        }
        
        return RedirectToAction("MyAppointments", "User");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => !u.IsDeleted && u.Email == model.Email);
        if (user != null)
        {
            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);
            await _db.SaveChangesAsync();

            var resetLink = Url.Action("ResetPassword", "Auth", 
                new { email = user.Email, token = user.ResetToken }, 
                Request.Scheme) ?? "";

            try 
            {
                await _emailService.SendPasswordResetAsync(user.Email, resetLink);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.ToString();
                return View(model);
            }
        }

        // Always show success message to prevent email enumeration
        TempData["Success"] = "If an account with that email exists, a password reset link has been sent.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string email, string token)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            TempData["Error"] = "Invalid password reset link.";
            return RedirectToAction("Login");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.ResetToken == token);
        if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
        {
            TempData["Error"] = "The password reset link is invalid or has expired.";
            return RedirectToAction("Login");
        }

        var model = new ResetPasswordViewModel { Email = email, Token = token };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.ResetToken == model.Token);
        if (user == null || user.ResetTokenExpiry < DateTime.UtcNow)
        {
            TempData["Error"] = "The password reset link is invalid or has expired.";
            return RedirectToAction("Login");
        }

        user.PasswordHash = _passwordHash.HashPassword(model.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Your password has been successfully reset. You can now log in.";
        return RedirectToAction("Login");
    }
}
