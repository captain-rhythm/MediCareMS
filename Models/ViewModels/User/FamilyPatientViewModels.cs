namespace MediCareMS.Models.ViewModels.User;

public class FamilyPatientManagerViewModel
{
    public List<FamilyPatientItem> Patients { get; set; } = new();
    public int MaxAllowed { get; set; } = 5;
    public bool CanAddMore => Patients.Count < MaxAllowed;
}

public class FamilyPatientItem
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? FatherMotherName { get; set; }
    public string? District { get; set; }
    public string? Thana { get; set; }
}

public class AddFamilyPatientViewModel
{
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-20);
    public string Gender { get; set; } = "Male";
    public string? FatherMotherName { get; set; }
    public string? District { get; set; }
    public string? Thana { get; set; }
}
