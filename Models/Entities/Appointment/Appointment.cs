using MediCareMS.Models.Enums;

namespace MediCareMS.Models.Entities.Appointment;

public class Appointment
{
    public int Id { get; set; }
    public string AppointmentNo { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly AppointmentTime { get; set; }
    public int? TokenNumber { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string? ChiefComplaint { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public Patient.Patient Patient { get; set; } = null!;
    public Doctor.DoctorProfile Doctor { get; set; } = null!;
    public Prescription.Prescription? Prescription { get; set; }
    public Billing.Invoice? Invoice { get; set; }
}
