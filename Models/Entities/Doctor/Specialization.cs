namespace MediCareMS.Models.Entities.Doctor;

public class Specialization
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<DoctorProfile> Doctors { get; set; } = new List<DoctorProfile>();
}
