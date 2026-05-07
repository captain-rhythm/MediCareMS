using MediCareMS.Models.Enums;

namespace MediCareMS.Models.Entities.Doctor;

public class DoctorSchedule
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotDurationMinutes { get; set; } = 20;
    public int MaxPatients { get; set; } = 15;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DoctorProfile Doctor { get; set; } = null!;
}
