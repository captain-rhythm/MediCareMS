using MediCareMS.Models.Enums;

namespace MediCareMS.Models.ViewModels.Report;

// ── Filters passed via query string ─────────────────────────────────────────
public class RevenueReportFilters
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo   { get; set; }
    public int?      DoctorId { get; set; }
    public string?   Method   { get; set; }   // "" | "Cash" | "Card" | "Online" | "Insurance"
    public string?   Status   { get; set; }   // "" | "Paid" | "Partial" | "Unpaid" | "Cancelled"
}

// ── Single row in the report table ──────────────────────────────────────────
public class RevenueReportRow
{
    public DateTime   Date            { get; set; }
    public string     AppointmentNo   { get; set; } = string.Empty;
    public string     InvoiceNo       { get; set; } = string.Empty;
    public string     PatientName     { get; set; } = string.Empty;
    public string     DoctorName      { get; set; } = string.Empty;
    public decimal    TotalAmount     { get; set; }
    public decimal    PaidAmount      { get; set; }
    public string     PaymentMethod   { get; set; } = string.Empty;
    public string     PaymentStatus   { get; set; } = string.Empty;
}

// ── Top-level summary card values ────────────────────────────────────────────
public class RevenueSummary
{
    public decimal TotalRevenue   { get; set; }
    public decimal TodayRevenue   { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal PaidRevenue    { get; set; }
    public decimal PendingRevenue { get; set; }
}

// ── Chart data points ────────────────────────────────────────────────────────
public class MonthlyRevenueStat
{
    public string  Label  { get; set; } = string.Empty;   // "Jan 2025"
    public decimal Amount { get; set; }
}

public class DoctorRevenueStat
{
    public string  DoctorName { get; set; } = string.Empty;
    public decimal Amount     { get; set; }
}

public class MethodStat
{
    public string  Method { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

// ── Dropdown item for doctor list ────────────────────────────────────────────
public class DoctorDropdownItem
{
    public int    Id       { get; set; }
    public string FullName { get; set; } = string.Empty;
}

// ── Top-level view model ──────────────────────────────────────────────────────
public class RevenueReportViewModel
{
    public RevenueSummary          Summary      { get; set; } = new();
    public List<RevenueReportRow>  Rows         { get; set; } = new();
    public List<MonthlyRevenueStat> MonthlyStats { get; set; } = new();
    public List<DoctorRevenueStat>  DoctorStats  { get; set; } = new();
    public List<MethodStat>         MethodStats  { get; set; } = new();
    public List<DoctorDropdownItem> Doctors      { get; set; } = new();
    public RevenueReportFilters     Filters      { get; set; } = new();
}
