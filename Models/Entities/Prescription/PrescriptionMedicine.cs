namespace MediCareMS.Models.Entities.Prescription;

public class PrescriptionMedicine
{
    public int Id { get; set; }
    public int PrescriptionId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public string? Instructions { get; set; }
    public DateTime CreatedAt { get; set; }

    public Prescription Prescription { get; set; } = null!;
}
