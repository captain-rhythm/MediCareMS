namespace MediCareMS.Models.ViewModels.Doctor;

public class CreatePrescriptionViewModel
{
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientIdString { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    
    public List<PrescriptionMedicineViewModel> Medicines { get; set; } = new();
}

public class PrescriptionMedicineViewModel
{
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
}
