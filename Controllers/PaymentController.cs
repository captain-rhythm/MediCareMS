using MediCareMS.Data;
using MediCareMS.Helpers;
using MediCareMS.Models.Entities.Billing;
using MediCareMS.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MediCareMS.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly AppDbContext _db;
    private readonly ISslCommerzService _ssl;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(AppDbContext db, ISslCommerzService ssl, ILogger<PaymentController> logger)
    {
        _db = db;
        _ssl = ssl;
        _logger = logger;
    }

    // ── Admin: Payment Ledger ───────────────────────────────────────────────
    [Authorize(Roles = "Super Admin,Hospital Admin,Receptionist")]
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var payments = await _db.Payments
            .Include(p => p.Invoice).ThenInclude(i => i.Patient)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync(ct);

        ViewBag.TotalRevenue = payments.Where(p => p.SslStatus == SSLCommerzStatus.Success).Sum(p => p.Amount);
        ViewBag.TodayRevenue = payments.Where(p => p.SslStatus == SSLCommerzStatus.Success && p.PaidAt.Date == DateTime.Today).Sum(p => p.Amount);
        return View(payments);
    }

    // ── Patient: Initiate SSLCommerz Payment ────────────────────────────────
    [Authorize(Roles = "Patient")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Initiate(int invoiceId, decimal amount, CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (patient == null) return Forbid();

        var invoice = await _db.Invoices
            .Include(i => i.Patient)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.PatientId == patient.Id && !i.IsDeleted, ct);
        if (invoice == null) return NotFound();

        var balance = invoice.TotalAmount - invoice.PaidAmount;
        if (amount <= 0 || amount > balance)
        {
            TempData["Error"] = "Invalid payment amount.";
            return RedirectToAction("Details", "Billing", new { id = invoiceId });
        }

        var txnId = $"MCR-{DateTime.UtcNow.Ticks}";

        // Store pending payment
        var pending = new Payment
        {
            InvoiceId = invoice.Id,
            Amount = amount,
            Method = PaymentMethod.Online,
            TransactionReference = txnId,
            SslStatus = SSLCommerzStatus.Pending,
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        _db.Payments.Add(pending);
        await _db.SaveChangesAsync(ct);

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var initResponse = await _ssl.InitiatePaymentAsync(new SslCommerzInitRequest
        {
            TransactionId = txnId,
            Amount = amount,
            CustomerName = invoice.Patient.FullName,
            CustomerEmail = User.FindFirstValue(ClaimTypes.Email) ?? "patient@medicare.com",
            CustomerPhone = invoice.Patient.MobileNumber ?? "01700000000",
            ProductName = $"Invoice {invoice.InvoiceNo}",
            SuccessUrl = Url.Action("Success", "Payment", null, Request.Scheme)!,
            FailUrl = Url.Action("Fail", "Payment", null, Request.Scheme)!,
            CancelUrl = Url.Action("Cancel", "Payment", null, Request.Scheme)!
        });

        if (!initResponse.IsSuccess)
        {
            pending.SslStatus = SSLCommerzStatus.Failed;
            pending.FailureReason = initResponse.ErrorMessage;
            await _db.SaveChangesAsync(ct);

            TempData["Error"] = $"Payment gateway error: {initResponse.ErrorMessage}";
            return RedirectToAction("Details", "Billing", new { id = invoiceId });
        }

        pending.SslSessionKey = initResponse.SessionKey;
        await _db.SaveChangesAsync(ct);

        return Redirect(initResponse.GatewayUrl!);
    }

    // ── SSLCommerz Success Callback ─────────────────────────────────────────
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Success([FromForm] IFormCollection form, CancellationToken ct = default)
    {
        var txnId = form["tran_id"].ToString();
        var valId = form["val_id"].ToString();
        var paidAmount = decimal.TryParse(form["amount"].ToString(), out var pa) ? pa : 0m;
        var cardType = form["card_type"].ToString();
        var cardIssuer = form["card_issuer"].ToString();
        var bankTxnId = form["bank_tran_id"].ToString();
        var status = form["status"].ToString();

        var payment = await _db.Payments.Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.TransactionReference == txnId, ct);

        if (payment == null)
        {
            _logger.LogWarning("SSLCommerz success callback: no payment found for txn {TxnId}", txnId);
            TempData["Error"] = "Payment reference not found.";
            return RedirectToAction("Dashboard", "User");
        }

        // Validate with SSLCommerz
        var isValid = await _ssl.ValidateTransactionAsync(valId, paidAmount, "BDT");

        payment.SslTransactionId = txnId;
        payment.SslValidationId = valId;
        payment.SslCardType = cardType;
        payment.SslCardIssuer = cardIssuer;
        payment.SslBankTransactionId = bankTxnId;
        payment.SslStatus = isValid ? SSLCommerzStatus.Success : SSLCommerzStatus.Invalid;
        payment.PaidAt = DateTime.UtcNow;

        if (isValid)
        {
            var invoice = payment.Invoice;
            invoice.PaidAmount += payment.Amount;
            invoice.Status = invoice.PaidAmount >= invoice.TotalAmount ? PaymentStatus.Paid : PaymentStatus.Partial;
        }

        await _db.SaveChangesAsync(ct);

        if (isValid)
        {
            TempData["Success"] = $"Payment of ৳{payment.Amount:N2} successful! Transaction ID: {txnId}";
            return RedirectToAction("Details", "Billing", new { id = payment.InvoiceId });
        }

        TempData["Error"] = "Payment validation failed. Please contact support.";
        return RedirectToAction("MyInvoices", "User");
    }

    // ── SSLCommerz Fail Callback ────────────────────────────────────────────
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Fail([FromForm] IFormCollection form, CancellationToken ct = default)
    {
        var txnId = form["tran_id"].ToString();
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.TransactionReference == txnId, ct);
        if (payment != null)
        {
            payment.SslStatus = SSLCommerzStatus.Failed;
            payment.FailureReason = form["error"].ToString();
            await _db.SaveChangesAsync(ct);
        }
        TempData["Error"] = "Payment failed. Please try again.";
        return RedirectToAction(payment != null ? "Details" : "MyInvoices", payment != null ? "Billing" : "User",
            payment != null ? new { id = payment.InvoiceId } : null);
    }

    // ── SSLCommerz Cancel Callback ──────────────────────────────────────────
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Cancel([FromForm] IFormCollection form, CancellationToken ct = default)
    {
        var txnId = form["tran_id"].ToString();
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.TransactionReference == txnId, ct);
        if (payment != null)
        {
            payment.SslStatus = SSLCommerzStatus.Cancelled;
            await _db.SaveChangesAsync(ct);
        }
        TempData["Error"] = "Payment was cancelled.";
        return RedirectToAction(payment != null ? "Details" : "MyInvoices", payment != null ? "Billing" : "User",
            payment != null ? new { id = payment.InvoiceId } : null);
    }
}
