using MediCareMS.Data;
using MediCareMS.Models.Enums;
using MediCareMS.Models.ViewModels.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Text;

namespace MediCareMS.Controllers;

[Authorize(Roles = "Super Admin,Hospital Admin")]
public class ReportController : Controller
{
    private readonly AppDbContext _db;

    public ReportController(AppDbContext db) => _db = db;

    // ── Main report page ─────────────────────────────────────────────────────
    public async Task<IActionResult> Index(
        DateTime? dateFrom, DateTime? dateTo,
        int? doctorId, string? method, string? status,
        CancellationToken ct = default)
    {
        var filters = new RevenueReportFilters
        {
            DateFrom = dateFrom,
            DateTo   = dateTo?.Date.AddDays(1).AddSeconds(-1),  // end of day
            DoctorId = doctorId,
            Method   = method,
            Status   = status
        };

        var vm = await BuildViewModel(filters, ct);
        return View(vm);
    }

    // ── Export as CSV (Excel-compatible) ─────────────────────────────────────
    public async Task<IActionResult> ExportCsv(
        DateTime? dateFrom, DateTime? dateTo,
        int? doctorId, string? method, string? status,
        CancellationToken ct = default)
    {
        var filters = new RevenueReportFilters
        {
            DateFrom = dateFrom,
            DateTo   = dateTo?.Date.AddDays(1).AddSeconds(-1),
            DoctorId = doctorId, Method = method, Status = status
        };

        var vm = await BuildViewModel(filters, ct);

        var sb = new StringBuilder();
        sb.AppendLine("Date,Appointment No,Invoice No,Patient Name,Doctor Name,Total Amount,Paid Amount,Payment Method,Status");
        foreach (var r in vm.Rows)
        {
            sb.AppendLine(
                $"{r.Date:dd-MMM-yyyy}," +
                $"\"{r.AppointmentNo}\"," +
                $"\"{r.InvoiceNo}\"," +
                $"\"{r.PatientName}\"," +
                $"\"{r.DoctorName}\"," +
                $"{r.TotalAmount:F2}," +
                $"{r.PaidAmount:F2}," +
                $"{r.PaymentMethod}," +
                $"{r.PaymentStatus}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return File(bytes, "text/csv", $"RevenueReport_{DateTime.Today:yyyyMMdd}.csv");
    }

    // ── Export as PDF ─────────────────────────────────────────────────────────
    public async Task<IActionResult> ExportPdf(
        DateTime? dateFrom, DateTime? dateTo,
        int? doctorId, string? method, string? status,
        CancellationToken ct = default)
    {
        var filters = new RevenueReportFilters
        {
            DateFrom = dateFrom,
            DateTo   = dateTo?.Date.AddDays(1).AddSeconds(-1),
            DoctorId = doctorId, Method = method, Status = status
        };

        var vm = await BuildViewModel(filters, ct);

        QuestPDF.Settings.License = LicenseType.Community;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(t => t.FontSize(9).FontFamily(Fonts.Arial));

                // ── Header ────────────────────────────────────────────────
                page.Header().Padding(10).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("MediCare HMS").FontSize(18).SemiBold().FontColor(Colors.Purple.Medium);
                        col.Item().Text("Revenue Report").FontSize(12).FontColor(Colors.Grey.Medium);
                        if (filters.DateFrom.HasValue || filters.DateTo.HasValue)
                            col.Item().Text($"Period: {filters.DateFrom:dd MMM yyyy} – {filters.DateTo:dd MMM yyyy}")
                               .FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                    row.ConstantItem(200).AlignRight().Column(col =>
                    {
                        col.Item().Text($"Generated: {DateTime.Now:dd MMM yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                        col.Item().Text($"Total Revenue: BDT {vm.Summary.TotalRevenue:N2}").FontSize(11).SemiBold().FontColor(Colors.Purple.Medium);
                    });
                });

                // ── Summary cards ─────────────────────────────────────────
                page.Content().PaddingTop(5).Column(col =>
                {
                    col.Spacing(8);

                    // Summary row
                    col.Item().Row(row =>
                    {
                        void Card(string label, decimal val, string color)
                        {
                            row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                               .Padding(8).Column(c =>
                               {
                                   c.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Medium);
                                   c.Item().Text($"BDT {val:N2}").FontSize(11).SemiBold().FontColor(color == "green" ? Colors.Green.Medium : color == "red" ? Colors.Red.Medium : Colors.Purple.Medium);
                               });
                        }
                        Card("Total Revenue",   vm.Summary.TotalRevenue,   "purple");
                        Card("Today's Revenue", vm.Summary.TodayRevenue,   "purple");
                        Card("Monthly Revenue", vm.Summary.MonthlyRevenue, "green");
                        Card("Paid Revenue",    vm.Summary.PaidRevenue,    "green");
                        Card("Pending Revenue", vm.Summary.PendingRevenue, "red");
                    });

                    // Table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(65);   // Date
                            c.RelativeColumn(1.2f); // Appt No
                            c.RelativeColumn(1.5f); // Invoice
                            c.RelativeColumn(2);    // Patient
                            c.RelativeColumn(2);    // Doctor
                            c.ConstantColumn(70);   // Total
                            c.ConstantColumn(70);   // Paid
                            c.ConstantColumn(60);   // Method
                            c.ConstantColumn(55);   // Status
                        });

                        // Header — correct QuestPDF v2 API
                        table.Header(header =>
                        {
                            foreach (var h in new[] { "Date","Appt No","Invoice No","Patient","Doctor","Total (BDT)","Paid (BDT)","Method","Status" })
                                header.Cell().Background(Colors.Purple.Lighten4).Padding(4)
                                      .Text(h).SemiBold().FontSize(8);
                        });

                        bool alt = false;
                        foreach (var r in vm.Rows)
                        {
                            var bg = alt ? Colors.Grey.Lighten5 : Colors.White;
                            alt = !alt;
                            void Cell(string v, bool right = false)
                            {
                                var cell = table.Cell().Background(bg).Padding(3)
                                    .BorderBottom(1).BorderColor(Colors.Grey.Lighten3);
                                if (right) cell.AlignRight().Text(v).FontSize(8);
                                else cell.Text(v).FontSize(8);
                            }
                            Cell(r.Date.ToString("dd MMM yy"));
                            Cell(r.AppointmentNo);
                            Cell(r.InvoiceNo);
                            Cell(r.PatientName);
                            Cell(r.DoctorName);
                            Cell($"{r.TotalAmount:N2}", true);
                            Cell($"{r.PaidAmount:N2}", true);
                            Cell(r.PaymentMethod);
                            Cell(r.PaymentStatus);
                        }
                    });
                });

                page.Footer().AlignCenter()
                    .Text(x =>
                    {
                        x.Span("MediCare HMS — Revenue Report — Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                        x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Medium);
                        x.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
                    });
            });
        });

        return File(doc.GeneratePdf(), "application/pdf",
                    $"RevenueReport_{DateTime.Today:yyyyMMdd}.pdf");
    }

    // ── Core query + aggregation ──────────────────────────────────────────────
    private async Task<RevenueReportViewModel> BuildViewModel(
        RevenueReportFilters filters, CancellationToken ct)
    {
        // ── Base invoice query (with successful payments) ─────────────────
        var invoiceQuery = _db.Invoices
            .Include(i => i.Patient)
            .Include(i => i.Doctor)
            .Include(i => i.Appointment)
            .Include(i => i.Payments)
            .Where(i => !i.IsDeleted);

        if (filters.DateFrom.HasValue)
            invoiceQuery = invoiceQuery.Where(i => i.CreatedAt >= filters.DateFrom.Value);
        if (filters.DateTo.HasValue)
            invoiceQuery = invoiceQuery.Where(i => i.CreatedAt <= filters.DateTo.Value);
        if (filters.DoctorId.HasValue)
            invoiceQuery = invoiceQuery.Where(i => i.DoctorId == filters.DoctorId.Value);
        if (!string.IsNullOrWhiteSpace(filters.Status) &&
            Enum.TryParse<PaymentStatus>(filters.Status, out var ps))
            invoiceQuery = invoiceQuery.Where(i => i.Status == ps);

        var invoices = await invoiceQuery.OrderByDescending(i => i.CreatedAt).ToListAsync(ct);

        // Filter by payment method (in memory, since method lives on Payment rows)
        PaymentMethod? pmFilter = null;
        if (!string.IsNullOrWhiteSpace(filters.Method) &&
            Enum.TryParse<PaymentMethod>(filters.Method, out var pmParsed))
            pmFilter = pmParsed;

        // ── Build table rows ──────────────────────────────────────────────
        var rows = new List<RevenueReportRow>();
        foreach (var inv in invoices)
        {
            var successPayments = inv.Payments
                .Where(p => p.SslStatus == SSLCommerzStatus.Success)
                .Where(p => pmFilter == null || p.Method == pmFilter)
                .ToList();

            if (pmFilter.HasValue && !successPayments.Any()) continue;

            var primaryMethod = successPayments.Any()
                ? successPayments.OrderByDescending(p => p.Amount).First().Method.ToString()
                : inv.Payments.Any()
                    ? inv.Payments.OrderByDescending(p => p.Amount).First().Method.ToString()
                    : "Cash";

            rows.Add(new RevenueReportRow
            {
                Date          = inv.CreatedAt,
                AppointmentNo = inv.Appointment.AppointmentNo,
                InvoiceNo     = inv.InvoiceNo,
                PatientName   = inv.Patient.FullName,
                DoctorName    = inv.Doctor.FullName,
                TotalAmount   = inv.TotalAmount,
                PaidAmount    = inv.PaidAmount,
                PaymentMethod = primaryMethod,
                PaymentStatus = inv.Status.ToString()
            });
        }

        // ── Summary cards (all-time, unfiltered for context) ─────────────
        var today     = DateTime.Today;
        var monthStart= new DateTime(today.Year, today.Month, 1);

        var allPaid = await _db.Invoices
            .Where(i => !i.IsDeleted && i.Status == PaymentStatus.Paid)
            .SumAsync(i => i.PaidAmount, ct);

        var allPending = await _db.Invoices
            .Where(i => !i.IsDeleted &&
                        (i.Status == PaymentStatus.Unpaid || i.Status == PaymentStatus.Partial))
            .SumAsync(i => i.TotalAmount - i.PaidAmount, ct);

        var todayRev = await _db.Payments
            .Where(p => p.SslStatus == SSLCommerzStatus.Success &&
                        p.PaidAt >= today && p.PaidAt < today.AddDays(1))
            .SumAsync(p => p.Amount, ct);

        var monthlyRev = await _db.Payments
            .Where(p => p.SslStatus == SSLCommerzStatus.Success &&
                        p.PaidAt >= monthStart)
            .SumAsync(p => p.Amount, ct);

        var totalRev = await _db.Payments
            .Where(p => p.SslStatus == SSLCommerzStatus.Success)
            .SumAsync(p => p.Amount, ct);

        // ── Monthly trend (last 12 months) ────────────────────────────────
        var twelveAgo = today.AddMonths(-11);
        var payments  = await _db.Payments
            .Where(p => p.SslStatus == SSLCommerzStatus.Success &&
                        p.PaidAt >= new DateTime(twelveAgo.Year, twelveAgo.Month, 1))
            .Select(p => new { p.PaidAt, p.Amount })
            .ToListAsync(ct);

        var monthlyStats = payments
            .GroupBy(p => new { p.PaidAt.Year, p.PaidAt.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyRevenueStat
            {
                Label  = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yy"),
                Amount = g.Sum(x => x.Amount)
            })
            .ToList();

        // ── Doctor-wise revenue (top 10) ──────────────────────────────────
        var doctorStats = await _db.Payments
            .Where(p => p.SslStatus == SSLCommerzStatus.Success)
            .GroupBy(p => p.Invoice.Doctor.FullName)
            .Select(g => new DoctorRevenueStat
            {
                DoctorName = g.Key,
                Amount     = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .Take(10)
            .ToListAsync(ct);

        // ── Payment method distribution ───────────────────────────────────
        var methodStats = await _db.Payments
            .Where(p => p.SslStatus == SSLCommerzStatus.Success)
            .GroupBy(p => p.Method)
            .Select(g => new MethodStat
            {
                Method = g.Key.ToString(),
                Amount = g.Sum(x => x.Amount)
            })
            .ToListAsync(ct);

        // ── Doctor dropdown ───────────────────────────────────────────────
        var doctors = await _db.Doctors
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.FullName)
            .Select(d => new DoctorDropdownItem { Id = d.Id, FullName = d.FullName })
            .ToListAsync(ct);

        return new RevenueReportViewModel
        {
            Filters      = filters,
            Summary      = new RevenueSummary
            {
                TotalRevenue   = totalRev,
                TodayRevenue   = todayRev,
                MonthlyRevenue = monthlyRev,
                PaidRevenue    = allPaid,
                PendingRevenue = allPending
            },
            Rows         = rows,
            MonthlyStats = monthlyStats,
            DoctorStats  = doctorStats,
            MethodStats  = methodStats,
            Doctors      = doctors
        };
    }
}
