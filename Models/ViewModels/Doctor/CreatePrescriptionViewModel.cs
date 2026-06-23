namespace MediCareMS.Models.ViewModels.Doctor;

public class CreatePrescriptionViewModel
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientIdString { get; set; } = string.Empty;
    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }
    
    public List<PrescriptionMedicineViewModel> Medicines { get; set; } = new();
}

public class PrescriptionMedicineViewModel
{
    public string? MedicineName { get; set; }
    public string? Dosage { get; set; }
    public string? Duration { get; set; }
    public string? Instructions { get; set; }
}
