using MediCareMS.Data;
using MediCareMS.Helpers;
using MediCareMS.Models.Entities.Billing;
using MediCareMS.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MediCareMS.Controllers;

public class PaymentController : Controller
{
    private readonly AppDbContext _db;
    private readonly ISslCommerzService _ssl;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(AppDbContext db, ISslCommerzService ssl, ILogger<PaymentController> logger)
    {
        _db = db; _ssl = ssl; _logger = logger;
    }

    // ── Admin: Payment Ledger ─────────────────────────────────────────────────
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

    // ── Patient: Initiate SSLCommerz Payment ──────────────────────────────────
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

        // Unique, traceable transaction ID
        var txnId = $"MCR-{invoice.Id}-{DateTime.UtcNow.Ticks}";

        // Store pending payment BEFORE redirecting to gateway
        var pending = new Payment
        {
            InvoiceId  = invoice.Id,
            Amount     = amount,
            Method     = PaymentMethod.Online,
            TransactionReference = txnId,
            SslStatus  = SSLCommerzStatus.Pending,
            PaidAt     = DateTime.UtcNow,
            CreatedAt  = DateTime.UtcNow,
            CreatedBy  = userId
        };
        _db.Payments.Add(pending);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("SSLCommerz: Initiating payment txnId={TxnId} invoiceId={InvoiceId} amount={Amount}",
            txnId, invoiceId, amount);

        // Build callback URLs — must be absolute, publicly reachable
        // For localhost use ngrok: ngrok http 5002
        var success = Url.Action("Success", "Payment", null, Request.Scheme, Request.Host.ToString())!;
        var fail    = Url.Action("Fail",    "Payment", null, Request.Scheme, Request.Host.ToString())!;
        var cancel  = Url.Action("Cancel",  "Payment", null, Request.Scheme, Request.Host.ToString())!;
        var ipn     = Url.Action("Ipn",     "Payment", null, Request.Scheme, Request.Host.ToString())!;

        var initResponse = await _ssl.InitiatePaymentAsync(new SslCommerzInitRequest
        {
            TransactionId   = txnId,
            Amount          = amount,
            CustomerName    = invoice.Patient.FullName,
            CustomerEmail   = User.FindFirstValue(ClaimTypes.Email) ?? "patient@medicare.com",
            CustomerPhone   = invoice.Patient.MobileNumber ?? "01700000000",
            ProductName     = $"Invoice {invoice.InvoiceNo}",
            SuccessUrl      = success,
            FailUrl         = fail,
            CancelUrl       = cancel,
            IpnUrl          = ipn         // ← server-to-server, works even if browser closes
        });

        if (!initResponse.IsSuccess)
        {
            pending.SslStatus     = SSLCommerzStatus.Failed;
            pending.FailureReason = initResponse.ErrorMessage;
            await _db.SaveChangesAsync(ct);

            _logger.LogError("SSLCommerz: Init failed txnId={TxnId} reason={Reason}", txnId, initResponse.ErrorMessage);
            TempData["Error"] = $"Payment gateway error: {initResponse.ErrorMessage}";
            return RedirectToAction("Details", "Billing", new { id = invoiceId });
        }

        pending.SslSessionKey = initResponse.SessionKey;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("SSLCommerz: Redirecting to gateway txnId={TxnId} url={Url}", txnId, initResponse.GatewayUrl);
        return Redirect(initResponse.GatewayUrl!);
    }

    // ── SSLCommerz Success Callback (browser redirect) ─────────────────────────
    // MUST be [AllowAnonymous] — SSLCommerz POSTs here, no auth cookie
    [AllowAnonymous]
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Success([FromForm] IFormCollection form, CancellationToken ct = default)
    {
        return await HandleCallback(form, "Success", ct);
    }

    // ── SSLCommerz Fail Callback ───────────────────────────────────────────────
    [AllowAnonymous]
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Fail([FromForm] IFormCollection form, CancellationToken ct = default)
    {
        var txnId = form["tran_id"].ToString();
        _logger.LogWarning("SSLCommerz: Payment FAILED txnId={TxnId}", txnId);

        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.TransactionReference == txnId, ct);
        if (payment != null)
        {
            payment.SslStatus     = SSLCommerzStatus.Failed;
            payment.FailureReason = form["error"].ToString();
            await _db.SaveChangesAsync(ct);
        }

        TempData["Error"] = "Payment failed. Please try again.";
        return payment != null
            ? RedirectToAction("Details", "Billing", new { id = payment.InvoiceId })
            : RedirectToAction("MyInvoices", "User");
    }

    // ── SSLCommerz Cancel Callback ─────────────────────────────────────────────
    [AllowAnonymous]
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Cancel([FromForm] IFormCollection form, CancellationToken ct = default)
    {
        var txnId = form["tran_id"].ToString();
        _logger.LogInformation("SSLCommerz: Payment CANCELLED txnId={TxnId}", txnId);

        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.TransactionReference == txnId, ct);
        if (payment != null)
        {
            payment.SslStatus = SSLCommerzStatus.Cancelled;
            await _db.SaveChangesAsync(ct);
        }

        TempData["Error"] = "Payment was cancelled.";
        return payment != null
            ? RedirectToAction("Details", "Billing", new { id = payment.InvoiceId })
            : RedirectToAction("MyInvoices", "User");
    }

    // ── SSLCommerz IPN (Instant Payment Notification) ─────────────────────────
    // Called server-to-server by SSLCommerz even if browser is closed.
    // MUST be [AllowAnonymous] and [IgnoreAntiforgeryToken]
    [AllowAnonymous]
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Ipn([FromForm] IFormCollection form, CancellationToken ct = default)
    {
        var txnId  = form["tran_id"].ToString();
        var status = form["status"].ToString();
        _logger.LogInformation("SSLCommerz IPN: txnId={TxnId} status={Status}", txnId, status);

        await ProcessPaymentCallback(form, ct);
        return Ok("IPN_RECEIVED");   // SSLCommerz expects a 200 OK
    }

    // ── Shared callback processing logic ──────────────────────────────────────
    private async Task<IActionResult> HandleCallback(IFormCollection form, string source, CancellationToken ct)
    {
        var txnId  = form["tran_id"].ToString();
        var valId  = form["val_id"].ToString();
        var status = form["status"].ToString();   // "VALID", "FAILED", "CANCELLED", "EXPIRED"

        _logger.LogInformation("SSLCommerz {Source}: txnId={TxnId} valId={ValId} status={Status}",
            source, txnId, valId, status);

        var payment = await _db.Payments
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.TransactionReference == txnId, ct);

        if (payment == null)
        {
            _logger.LogWarning("SSLCommerz {Source}: No payment found for txnId={TxnId}", source, txnId);
            TempData["Error"] = "Payment reference not found.";
            return RedirectToAction("MyInvoices", "User");
        }

        // Prevent double-processing
        if (payment.SslStatus == SSLCommerzStatus.Success)
        {
            _logger.LogInformation("SSLCommerz {Source}: txnId={TxnId} already marked Success — skipping", source, txnId);
            TempData["Success"] = $"Payment of ৳{payment.Amount:N2} already processed.";
            return RedirectToAction("Details", "Billing", new { id = payment.InvoiceId });
        }

        await ProcessPaymentCallback(form, ct);

        // Reload payment after processing
        payment = await _db.Payments.FirstOrDefaultAsync(p => p.TransactionReference == txnId, ct);

        if (payment?.SslStatus == SSLCommerzStatus.Success)
        {
            TempData["Success"] = $"Payment of ৳{payment.Amount:N2} successful! Txn: {txnId}";
            return RedirectToAction("Details", "Billing", new { id = payment.InvoiceId });
        }

        TempData["Error"] = "Payment validation failed. Contact support if amount was deducted.";
        return RedirectToAction("Details", "Billing", new { id = payment?.InvoiceId });
    }

    private async Task ProcessPaymentCallback(IFormCollection form, CancellationToken ct)
    {
        var txnId       = form["tran_id"].ToString();
        var valId       = form["val_id"].ToString();
        var status      = form["status"].ToString();
        var paidAmount  = decimal.TryParse(form["amount"].ToString(), out var pa) ? pa : 0m;
        var cardType    = form["card_type"].ToString();
        var cardIssuer  = form["card_issuer"].ToString();
        var bankTxnId   = form["bank_tran_id"].ToString();

        var payment = await _db.Payments
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.TransactionReference == txnId, ct);

        if (payment == null || payment.SslStatus == SSLCommerzStatus.Success)
            return;

        payment.SslTransactionId    = txnId;
        payment.SslValidationId     = valId;
        payment.SslCardType         = cardType;
        payment.SslCardIssuer       = cardIssuer;
        payment.SslBankTransactionId = bankTxnId;
        payment.PaidAt              = DateTime.UtcNow;

        // 1st check: status field from the POST body
        if (status != "VALID" && status != "VALIDATED")
        {
            payment.SslStatus     = SSLCommerzStatus.Invalid;
            payment.FailureReason = $"Status from gateway: {status}";
            _logger.LogWarning("SSLCommerz: Invalid status={Status} txnId={TxnId}", status, txnId);
            await _db.SaveChangesAsync(ct);
            return;
        }

        // 2nd check: server-to-server validation via val_id
        var isValid = await _ssl.ValidateTransactionAsync(valId, paidAmount, "BDT");

        if (isValid)
        {
            payment.SslStatus = SSLCommerzStatus.Success;
            var invoice = payment.Invoice;
            invoice.PaidAmount += payment.Amount;
            invoice.Status = invoice.PaidAmount >= invoice.TotalAmount
                ? PaymentStatus.Paid : PaymentStatus.Partial;
            invoice.UpdatedAt = DateTime.UtcNow;

            // ── Confirm the appointment linked to this invoice ──────────────
            if (invoice.AppointmentId > 0)
            {
                var appointment = await _db.Appointments.FindAsync(invoice.AppointmentId, ct);
                if (appointment != null && appointment.Status == AppointmentStatus.PendingPayment)
                {
                    appointment.Status    = AppointmentStatus.Confirmed;
                    appointment.UpdatedAt = DateTime.UtcNow;
                    _logger.LogInformation("Appointment {AptNo} confirmed after payment {TxnId}",
                        appointment.AppointmentNo, txnId);
                }
            }

            _logger.LogInformation("SSLCommerz: Payment SUCCESS txnId={TxnId} invoiceId={InvoiceId} amount={Amount}",
                txnId, invoice.Id, payment.Amount);
        }
        else
        {
            payment.SslStatus     = SSLCommerzStatus.Invalid;
            payment.FailureReason = "Server-side validation failed";
            _logger.LogError("SSLCommerz: Validation FAILED txnId={TxnId} valId={ValId}", txnId, valId);
        }

        await _db.SaveChangesAsync(ct);
    }
}
