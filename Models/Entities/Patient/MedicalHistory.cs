namespace MediCareMS.Models.Entities.Patient;

public class MedicalHistory
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime DiagnosedOn { get; set; }
    public bool IsOngoing { get; set; }
    public DateTime CreatedAt { get; set; }

    public Patient Patient { get; set; } = null!;
}
