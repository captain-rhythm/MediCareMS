using MediCareMS.Models.Enums;

namespace MediCareMS.Models.Entities.Billing;

public class Invoice
{
    public int Id { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public decimal ConsultationFee { get; set; }
    public decimal TestFee { get; set; }
    public decimal OtherCharges { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Unpaid;
    public DateOnly DueDate { get; set; }
    public string? Notes { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedBy { get; set; }

    public Appointment.Appointment Appointment { get; set; } = null!;
    public Patient.Patient Patient { get; set; } = null!;
    public Doctor.DoctorProfile Doctor { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}
