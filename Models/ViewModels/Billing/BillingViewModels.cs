using MediCareMS.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MediCareMS.Models.ViewModels.Billing;

public class InvoiceListViewModel
{
    public int Id { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue => TotalAmount - PaidAmount;
    public PaymentStatus Status { get; set; }
    public DateOnly DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateInvoiceViewModel
{
    [Required(ErrorMessage = "Please select an appointment")]
    public int? AppointmentId { get; set; }

    [Required(ErrorMessage = "Please select a patient")]
    public int? PatientId { get; set; }

    [Required(ErrorMessage = "Please select a doctor")]
    public int? DoctorId { get; set; }

    [Range(0, 1000000)]
    public decimal ConsultationFee { get; set; }

    [Range(0, 1000000)]
    public decimal TestFee { get; set; }

    [Range(0, 1000000)]
    public decimal OtherCharges { get; set; }

    [Range(0, 100)]
    public decimal DiscountPercent { get; set; }

    public string? Notes { get; set; }

    public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(7));

    public List<InvoiceItemViewModel> ExtraItems { get; set; } = new();
}

public class InvoiceItemViewModel
{
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
}

public class InvoiceDetailsViewModel
{
    public int Id { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string PatientPhone { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DoctorSpecialization { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public string AppointmentNo { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public decimal TestFee { get; set; }
    public decimal OtherCharges { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue => TotalAmount - PaidAmount;
    public PaymentStatus Status { get; set; }
    public DateOnly DueDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<InvoiceItemViewModel> Items { get; set; } = new();
    public List<PaymentHistoryItem> Payments { get; set; } = new();
}

public class PaymentHistoryItem
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? TransactionReference { get; set; }
    public string? SslTransactionId { get; set; }
    public SSLCommerzStatus SslStatus { get; set; }
    public DateTime PaidAt { get; set; }
}

public class RecordManualPaymentViewModel
{
    [Required]
    public int InvoiceId { get; set; }

    [Required, Range(1, 1000000)]
    public decimal Amount { get; set; }

    [Required]
    public PaymentMethod Method { get; set; }

    public string? Reference { get; set; }
}
