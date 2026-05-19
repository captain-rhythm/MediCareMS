using System.Security.Claims;
using MediCareMS.Data;
using MediCareMS.Helpers.Email;
using MediCareMS.Helpers.Security;
using MediCareMS.Helpers.QRCode;
using MediCareMS.Models.Entities.Auth;
using MediCareMS.Models.Enums;
using MediCareMS.Models.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MediCareMS.Controllers;

[Authorize(Roles = "Super Admin,Hospital Admin")]
public class UserManagementController : Controller
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly IPasswordHashService _passwordHash;
    private readonly IQRCodeService _qrCode;

    public UserManagementController(AppDbContext db, IEmailService email, IPasswordHashService passwordHash, IQRCodeService qrCode)
    {
        _db = db;
        _email = email;
        _passwordHash = passwordHash;
        _qrCode = qrCode;
    }

    // GET: /UserManagement/Index — list all invitations
    public async Task<IActionResult> Index()
    {
        var invitations = await _db.Invitations
            .Include(i => i.InvitedByUser)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return View(invitations);
    }

    // POST: /UserManagement/SendInvite
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendInvite(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["Error"] = "Email address is required.";
            return RedirectToAction(nameof(Index));
        }

        // Duplicate active invite check
        var exists = await _db.Invitations
            .AnyAsync(i => i.Email == email && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow);

        if (exists)
        {
            TempData["Error"] = $"An active invitation already exists for {email}.";
            return RedirectToAction(nameof(Index));
        }

        var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var invitation = new Invitation
        {
            Email = email,
            Token = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(3),
            InvitedByUserId = adminId,
            Status = "Pending"
        };

        _db.Invitations.Add(invitation);
        await _db.SaveChangesAsync();

        var link = Url.Action("Register", "Auth",
            new { token = invitation.Token, email = invitation.Email },
            Request.Scheme)!;

        try
        {
            await _email.SendInvitationAsync(email, link);
            TempData["Success"] = $"Invitation sent to {email}!";
        }
        catch (Exception ex)
        {
            // Still saved the invite, just couldn't email — show the link
            TempData["Warning"] = $"Invitation saved but email failed: {ex.Message}. Manual link: {link}";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /UserManagement/Approve
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var inv = await _db.Invitations.FindAsync(id);
        if (inv == null) return NotFound();

        inv.Status = "Accepted";
        await _db.SaveChangesAsync();

        // Activate the user
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == inv.Email);
        if (user != null)
        {
            user.Status = AccountStatus.Active;
            user.IsEmailConfirmed = true;

            // Auto-create Doctor profile if the requested role is Doctor
            if (inv.RequestedRole == "Doctor")
            {
                var doctorExists = await _db.Doctors.AnyAsync(d => d.UserId == user.Id || d.Email == user.Email);
                if (!doctorExists)
                {
                    var count = await _db.Doctors.CountAsync(d => !d.IsDeleted) + 1;
                    var doctor = new MediCareMS.Models.Entities.Doctor.DoctorProfile
                    {
                        DoctorNo = $"DOC-{count:D3}",
                        UserId = user.Id,
                        FullName = inv.RegisteredFullName ?? user.UserName,
                        MobileNumber = inv.RegisteredPhone ?? user.PhoneNumber ?? "",
                        Email = user.Email,
                        DepartmentId = 1, // Default General Medicine
                        SpecializationId = 1, // Default General Physician
                        ConsultationFee = 500, // Default fee
                        Status = MediCareMS.Models.Enums.DoctorStatus.Available,
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.Doctors.Add(doctor);
                }
            }

            await _db.SaveChangesAsync();
        }

        TempData["Success"] = $"{inv.Email} has been approved and can now log in.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /UserManagement/Decline
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decline(int id)
    {
        var inv = await _db.Invitations.FindAsync(id);
        if (inv == null) return NotFound();

        inv.Status = "Declined";
        await _db.SaveChangesAsync();

        // Suspend the user account if created
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == inv.Email);
        if (user != null)
        {
            user.Status = AccountStatus.Suspended;
            await _db.SaveChangesAsync();
        }

        TempData["Error"] = $"{inv.Email} has been declined.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /UserManagement/Resend
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resend(int id)
    {
        var inv = await _db.Invitations.FindAsync(id);
        if (inv == null) return NotFound();

        inv.Token = Guid.NewGuid().ToString("N");
        inv.ExpiresAt = DateTime.UtcNow.AddDays(3);
        inv.IsUsed = false;
        inv.Status = "Pending";
        await _db.SaveChangesAsync();

        var link = Url.Action("Register", "Auth",
            new { token = inv.Token, email = inv.Email },
            Request.Scheme)!;

        try
        {
            await _email.SendInvitationAsync(inv.Email, link);
            TempData["Success"] = $"Invitation resent to {inv.Email}.";
        }
        catch (Exception ex)
        {
            TempData["Warning"] = $"Invitation updated but email failed: {ex.Message}. Manual link: {link}";
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: /UserManagement/GetQRCode/{id}
    [HttpGet]
    public async Task<IActionResult> GetQRCode(int id)
    {
        var inv = await _db.Invitations.FindAsync(id);
        if (inv == null)
            return NotFound(new { message = "Invitation not found" });

        if (inv.IsUsed)
            return BadRequest(new { message = "This invitation has already been used" });

        if (inv.ExpiresAt < DateTime.UtcNow)
            return BadRequest(new { message = "This invitation has expired" });

        try
        {
            var link = Url.Action("Register", "Auth",
                new { token = inv.Token, email = inv.Email },
                Request.Scheme)!;

            var qrCodePng = _qrCode.GenerateQRCodePNG(link);
            return File(qrCodePng, "image/png", $"invitation-qr-{inv.Id}.png");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error generating QR code", error = ex.Message });
        }
    }

    // GET: /UserManagement/GetQRCodeBase64/{id}
    [HttpGet]
    public async Task<IActionResult> GetQRCodeBase64(int id)
    {
        var inv = await _db.Invitations.FindAsync(id);
        if (inv == null)
            return NotFound(new { message = "Invitation not found" });

        if (inv.IsUsed)
            return BadRequest(new { message = "This invitation has already been used" });

        if (inv.ExpiresAt < DateTime.UtcNow)
            return BadRequest(new { message = "This invitation has expired" });

        try
        {
            var link = Url.Action("Register", "Auth",
                new { token = inv.Token, email = inv.Email },
                Request.Scheme)!;

            var qrCodeBase64 = _qrCode.GenerateQRCodeBase64(link);
            return Json(new { success = true, qrCode = qrCodeBase64, invitationLink = link });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error generating QR code", error = ex.Message });
        }
    }
}

