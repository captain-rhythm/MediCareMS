namespace MediCareMS.Models.Entities.Billing;

public class InvoiceItem
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal Total => UnitPrice * Quantity;
    public DateTime CreatedAt { get; set; }

    public Invoice Invoice { get; set; } = null!;
}
