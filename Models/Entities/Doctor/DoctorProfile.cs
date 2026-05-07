using MediCareMS.Models.Enums;

namespace MediCareMS.Models.Entities.Doctor;

public class DoctorProfile
{
    public int Id { get; set; }
    public string DoctorNo { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public int SpecializationId { get; set; }
    public string? Qualification { get; set; }
    public int? ExperienceYears { get; set; }
    public string? BmdcRegNo { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public decimal ConsultationFee { get; set; }
    public string? ChamberAddress { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImagePath { get; set; }
    public DoctorStatus Status { get; set; } = DoctorStatus.Available;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public Department Department { get; set; } = null!;
    public Specialization Specialization { get; set; } = null!;
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
    public ICollection<Appointment.Appointment> Appointments { get; set; } = new List<Appointment.Appointment>();
}
