using MediCareMS.Models.Enums;

namespace MediCareMS.Models.Entities.Doctor;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<DoctorProfile> Doctors { get; set; } = new List<DoctorProfile>();
}
