using MediCareMS.Models.Enums;

namespace MediCareMS.Models.Entities.Prescription;

public class Prescription
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }
    public int PatientId { get; set; }
    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }
    public string? FollowUpInstructions { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Draft;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Appointment.Appointment Appointment { get; set; } = null!;
    public Doctor.DoctorProfile Doctor { get; set; } = null!;
    public Patient.Patient Patient { get; set; } = null!;
    public ICollection<PrescriptionMedicine> Medicines { get; set; } = new List<PrescriptionMedicine>();
    public ICollection<LabTestRequest> LabRequests { get; set; } = new List<LabTestRequest>();
}
