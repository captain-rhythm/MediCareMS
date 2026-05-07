namespace MediCareMS.Models.Entities.Patient;

public class PatientDocument
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Patient Patient { get; set; } = null!;
}
