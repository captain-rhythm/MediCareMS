using MediCareMS.Data;
using MediCareMS.Helpers;
using MediCareMS.Models.Entities.Billing;
using MediCareMS.Models.Enums;
using MediCareMS.Models.ViewModels.Billing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MediCareMS.Controllers;

[Authorize]
public class BillingController : Controller
{
    private readonly AppDbContext _db;

    public BillingController(AppDbContext db)
    {
        _db = db;
    }

    // ── Admin: Invoice List ────────────────────────────────────────────────
    [Authorize(Roles = "Super Admin,Hospital Admin,Receptionist")]
    public async Task<IActionResult> Index(string? search, string? status, CancellationToken ct = default)
    {
        var query = _db.Invoices
            .Include(i => i.Patient)
            .Include(i => i.Doctor)
            .Where(i => !i.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(i => i.InvoiceNo.Contains(search) ||
                                     i.Patient.FullName.Contains(search) ||
                                     i.Doctor.FullName.Contains(search));

        if (Enum.TryParse<PaymentStatus>(status, out var ps))
            query = query.Where(i => i.Status == ps);

        var invoices = await query
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new InvoiceListViewModel
            {
                Id = i.Id,
                InvoiceNo = i.InvoiceNo,
                PatientName = i.Patient.FullName,
                DoctorName = i.Doctor.FullName,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                Status = i.Status,
                DueDate = i.DueDate,
                CreatedAt = i.CreatedAt
            }).ToListAsync(ct);

        ViewBag.Search = search;
        ViewBag.Status = status;
        return View(invoices);
    }

    // ── Admin: Create Invoice ───────────────────────────────────────────────
    [Authorize(Roles = "Super Admin,Hospital Admin,Receptionist")]
    [HttpGet]
    public async Task<IActionResult> Create(int? appointmentId, CancellationToken ct = default)
    {
        var vm = new CreateInvoiceViewModel();

        if (appointmentId.HasValue)
        {
            var apt = await _db.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId.Value && !a.IsDeleted, ct);

            if (apt != null)
            {
                vm.AppointmentId = apt.Id;
                vm.PatientId = apt.PatientId;
                vm.DoctorId = apt.DoctorId;
                vm.ConsultationFee = apt.Doctor.ConsultationFee;
            }
        }

        await LoadCreateDropdowns(ct);
        return View(vm);
    }

    [Authorize(Roles = "Super Admin,Hospital Admin,Receptionist")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateInvoiceViewModel model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid) { await LoadCreateDropdowns(ct); return View(model); }

        // Check no duplicate invoice for appointment
        if (await _db.Invoices.AnyAsync(i => i.AppointmentId == model.AppointmentId && !i.IsDeleted, ct))
        {
            ModelState.AddModelError("", "An invoice already exists for this appointment.");
            await LoadCreateDropdowns(ct);
            return View(model);
        }

        var subTotal = model.ConsultationFee + model.TestFee + model.OtherCharges;
        var discountAmt = Math.Round(subTotal * model.DiscountPercent / 100m, 2);
        var total = subTotal - discountAmt;

        var count = await _db.Invoices.CountAsync(ct) + 1;
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        var invoice = new Invoice
        {
            InvoiceNo = $"INV-{DateTime.Today.Year}-{count:D4}",
            AppointmentId = model.AppointmentId ?? 0,
            PatientId = model.PatientId ?? 0,
            DoctorId = model.DoctorId ?? 0,
            ConsultationFee = model.ConsultationFee,
            TestFee = model.TestFee,
            OtherCharges = model.OtherCharges,
            Discount = discountAmt,
            TotalAmount = total,
            PaidAmount = 0,
            Status = PaymentStatus.Unpaid,
            DueDate = model.DueDate,
            Notes = model.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(ct);

        // Extra line items
        var validItems = model.ExtraItems.Where(x => !string.IsNullOrWhiteSpace(x.Description) && x.UnitPrice > 0).ToList();
        foreach (var item in validItems)
        {
            _db.InvoiceItems.Add(new InvoiceItem
            {
                InvoiceId = invoice.Id,
                Description = item.Description,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                CreatedAt = DateTime.UtcNow
            });
        }
        if (validItems.Any()) await _db.SaveChangesAsync(ct);

        TempData["Success"] = $"Invoice {invoice.InvoiceNo} created successfully.";
        return RedirectToAction(nameof(Details), new { id = invoice.Id });
    }

    // ── Invoice Details ─────────────────────────────────────────────────────
    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Patient)
            .Include(i => i.Doctor).ThenInclude(d => d.Department)
            .Include(i => i.Doctor).ThenInclude(d => d.Specialization)
            .Include(i => i.Appointment)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, ct);

        if (invoice == null) return NotFound();

        // Security: patients can only see their own invoices
        if (User.IsInRole("Patient"))
        {
            var patient = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!), ct);
            if (patient == null || patient.Id != invoice.PatientId) return Forbid();
        }

        var vm = new InvoiceDetailsViewModel
        {
            Id = invoice.Id,
            InvoiceNo = invoice.InvoiceNo,
            PatientName = invoice.Patient.FullName,
            PatientPhone = invoice.Patient.MobileNumber ?? "-",
            DoctorName = invoice.Doctor.FullName,
            DoctorSpecialization = invoice.Doctor.Specialization?.Name ?? "-",
            Department = invoice.Doctor.Department?.Name ?? "-",
            AppointmentDate = invoice.Appointment.AppointmentDate,
            AppointmentNo = invoice.Appointment.AppointmentNo,
            ConsultationFee = invoice.ConsultationFee,
            TestFee = invoice.TestFee,
            OtherCharges = invoice.OtherCharges,
            Discount = invoice.Discount,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            Status = invoice.Status,
            DueDate = invoice.DueDate,
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt,
            Items = invoice.Items.Select(x => new InvoiceItemViewModel { Description = x.Description, UnitPrice = x.UnitPrice, Quantity = x.Quantity }).ToList(),
            Payments = invoice.Payments.Select(p => new PaymentHistoryItem
            {
                Id = p.Id,
                Amount = p.Amount,
                Method = p.Method,
                TransactionReference = p.TransactionReference,
                SslTransactionId = p.SslTransactionId,
                SslStatus = p.SslStatus,
                PaidAt = p.PaidAt
            }).ToList()
        };

        return View(vm);
    }

    // ── Admin: Manual Payment ───────────────────────────────────────────────
    [Authorize(Roles = "Super Admin,Hospital Admin,Receptionist")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(RecordManualPaymentViewModel model, CancellationToken ct = default)
    {
        var invoice = await _db.Invoices.Include(i => i.Payments).FirstOrDefaultAsync(i => i.Id == model.InvoiceId && !i.IsDeleted, ct);
        if (invoice == null) return NotFound();

        var maxPayable = invoice.TotalAmount - invoice.PaidAmount;
        if (model.Amount > maxPayable)
        {
            TempData["Error"] = $"Amount exceeds balance due (৳{maxPayable:N2}).";
            return RedirectToAction(nameof(Details), new { id = model.InvoiceId });
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var payment = new Payment
        {
            InvoiceId = invoice.Id,
            Amount = model.Amount,
            Method = model.Method,
            TransactionReference = model.Reference,
            SslStatus = SSLCommerzStatus.Success,
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        invoice.PaidAmount += model.Amount;
        invoice.Status = invoice.PaidAmount >= invoice.TotalAmount ? PaymentStatus.Paid : PaymentStatus.Partial;

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);

        TempData["Success"] = $"Payment of ৳{model.Amount:N2} recorded successfully.";
        return RedirectToAction(nameof(Details), new { id = model.InvoiceId });
    }

    // ── Cancel Invoice ──────────────────────────────────────────────────────
    [Authorize(Roles = "Super Admin,Hospital Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct = default)
    {
        var invoice = await _db.Invoices.FindAsync(new object[] { id }, ct);
        if (invoice == null) return NotFound();

        invoice.Status = PaymentStatus.Cancelled;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        TempData["Success"] = "Invoice cancelled.";
        return RedirectToAction(nameof(Index));
    }

    // ── PDF Download ────────────────────────────────────────────────────────
    public async Task<IActionResult> DownloadPdf(int id, CancellationToken ct = default)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Patient)
            .Include(i => i.Doctor).ThenInclude(d => d.Department)
            .Include(i => i.Appointment)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, ct);

        if (invoice == null) return NotFound();

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                page.Header()
                    .Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("MediCare").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text("123 Health Avenue, Medical City").FontSize(10).FontColor(Colors.Grey.Medium);
                        });
                        row.ConstantItem(150).AlignRight().Column(col =>
                        {
                            col.Item().Text("INVOICE").FontSize(18).SemiBold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"#{invoice.InvoiceNo}").FontSize(12).SemiBold();
                            col.Item().Text($"Date: {invoice.CreatedAt:dd MMM yyyy}").FontSize(10).FontColor(Colors.Grey.Medium);
                            col.Item().Text($"Due: {invoice.DueDate:dd MMM yyyy}").FontSize(10).FontColor(Colors.Grey.Medium);
                        });
                    });

                page.Content()
                    .PaddingVertical(20)
                    .Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Bill To:").SemiBold().FontColor(Colors.Grey.Medium);
                                c.Item().Text(invoice.Patient.FullName).SemiBold().FontSize(13);
                                c.Item().Text(invoice.Patient.MobileNumber ?? "").FontColor(Colors.Grey.Medium);
                            });
                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("Consulting Doctor:").SemiBold().FontColor(Colors.Grey.Medium);
                                c.Item().Text(invoice.Doctor.FullName).SemiBold().FontSize(13);
                                c.Item().Text(invoice.Doctor.Department?.Name ?? "").FontColor(Colors.Grey.Medium);
                                c.Item().Text($"Apt: {invoice.Appointment.AppointmentNo} ({invoice.Appointment.AppointmentDate:dd MMM yyyy})").FontSize(10).FontColor(Colors.Grey.Medium);
                            });
                        });

                        col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5).Text("Description").SemiBold();
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5).AlignRight().Text("Unit Price").SemiBold();
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5).AlignCenter().Text("Qty").SemiBold();
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5).AlignRight().Text("Total").SemiBold();
                            });

                            void AddRow(string desc, decimal price, int qty)
                            {
                                table.Cell().Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Text(desc);
                                table.Cell().Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).AlignRight().Text($"BDT {price:N2}");
                                table.Cell().Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).AlignCenter().Text($"{qty}");
                                table.Cell().Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).AlignRight().Text($"BDT {price * qty:N2}");
                            }

                            if (invoice.ConsultationFee > 0) AddRow("Consultation Fee", invoice.ConsultationFee, 1);
                            if (invoice.TestFee > 0) AddRow("Lab / Test Fee", invoice.TestFee, 1);
                            if (invoice.OtherCharges > 0) AddRow("Other Charges", invoice.OtherCharges, 1);
                            foreach (var item in invoice.Items) AddRow(item.Description, item.UnitPrice, item.Quantity);
                        });

                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem();
                            row.ConstantItem(200).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                var subTotal = invoice.ConsultationFee + invoice.TestFee + invoice.OtherCharges + invoice.Items.Sum(x => x.UnitPrice * x.Quantity);
                                
                                table.Cell().AlignRight().Text("Sub Total:").SemiBold();
                                table.Cell().AlignRight().Text($"BDT {subTotal:N2}");

                                if (invoice.Discount > 0)
                                {
                                    table.Cell().AlignRight().Text("Discount:").SemiBold().FontColor(Colors.Green.Medium);
                                    table.Cell().AlignRight().Text($"- BDT {invoice.Discount:N2}").FontColor(Colors.Green.Medium);
                                }

                                table.Cell().AlignRight().Text("Total Amount:").SemiBold().FontSize(12);
                                table.Cell().AlignRight().Text($"BDT {invoice.TotalAmount:N2}").SemiBold().FontSize(12).FontColor(Colors.Blue.Medium);

                                table.Cell().AlignRight().Text("Paid Amount:").SemiBold();
                                table.Cell().AlignRight().Text($"BDT {invoice.PaidAmount:N2}").FontColor(Colors.Green.Medium);

                                table.Cell().AlignRight().Text("Balance Due:").SemiBold().FontSize(12);
                                table.Cell().AlignRight().Text($"BDT {invoice.TotalAmount - invoice.PaidAmount:N2}").SemiBold().FontSize(12).FontColor(Colors.Red.Medium);
                            });
                        });

                        var statusText = invoice.Status == PaymentStatus.Paid ? "PAID" : invoice.Status == PaymentStatus.Partial ? "PARTIAL" : invoice.Status == PaymentStatus.Cancelled ? "CANCELLED" : "UNPAID";
                        var statusColor = invoice.Status == PaymentStatus.Paid ? Colors.Green.Medium : invoice.Status == PaymentStatus.Partial ? Colors.Blue.Medium : invoice.Status == PaymentStatus.Cancelled ? Colors.Red.Medium : Colors.Orange.Medium;

                        col.Item().PaddingTop(10).Text($"Status: {statusText}").FontSize(16).SemiBold().FontColor(statusColor);

                        if (!string.IsNullOrEmpty(invoice.Notes))
                        {
                            col.Item().PaddingTop(10).Text("Notes:").SemiBold().FontColor(Colors.Grey.Medium);
                            col.Item().Text(invoice.Notes).FontSize(10).FontColor(Colors.Grey.Medium);
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text("Thank you for choosing MediCare Hospital ❤️").FontSize(10).FontColor(Colors.Grey.Medium);
            });
        });

        var pdfBytes = document.GeneratePdf();
        return File(pdfBytes, "application/pdf", $"Invoice_{invoice.InvoiceNo}.pdf");
    }

    private async Task LoadCreateDropdowns(CancellationToken ct)
    {
        ViewBag.Appointments = await _db.Appointments
            .Include(a => a.Patient)
            .Where(a => !a.IsDeleted && !_db.Invoices.Any(i => i.AppointmentId == a.Id && !i.IsDeleted))
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
        ViewBag.Patients = await _db.Patients.Where(p => !p.IsDeleted).OrderBy(p => p.FullName).ToListAsync(ct);
        ViewBag.Doctors = await _db.Doctors.Where(d => !d.IsDeleted).OrderBy(d => d.FullName).ToListAsync(ct);
    }
}
