using MediCareMS.Models.Enums;

namespace MediCareMS.Models.Entities.Billing;

public class Payment
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public string? TransactionReference { get; set; }

    // SSLCommerz fields
    public string? SslTransactionId { get; set; }
    public string? SslSessionKey { get; set; }
    public string? SslValidationId { get; set; }
    public string? SslCardType { get; set; }
    public string? SslCardIssuer { get; set; }
    public SSLCommerzStatus SslStatus { get; set; } = SSLCommerzStatus.Pending;
    public string? SslBankTransactionId { get; set; }
    public string? SslGwVersion { get; set; }
    public string? FailureReason { get; set; }

    public DateTime PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedBy { get; set; }

    public Invoice Invoice { get; set; } = null!;
}
