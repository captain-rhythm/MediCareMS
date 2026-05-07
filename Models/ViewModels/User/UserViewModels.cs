using System.ComponentModel.DataAnnotations;
using MediCareMS.Models.Enums;
using MediCareMS.Models.Entities.Patient;

namespace MediCareMS.Models.ViewModels.User;

public class UserDashboardViewModel
{
    public int TotalAppointments { get; set; }
    public int UpcomingAppointments { get; set; }
    public int TotalPrescriptions { get; set; }
    public string? PatientBloodGroup { get; set; }
    public string? PatientMobile { get; set; }
    public List<MyAppointmentItem> RecentAppointments { get; set; } = new();
}

public class MyProfileViewModel
{
    [Required]
    public string FullName { get; set; } = string.Empty;
    
    [Required, Phone, RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Invalid phone number format")]
    public string MobileNumber { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Blood Group is required")]
    public BloodGroup BloodGroup { get; set; }
    
    [Required, DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }
    
    [Required]
    public Gender Gender { get; set; }
    
    public string? Address { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
}

public class MyAppointmentItem
{
    public int Id { get; set; }
    public string AppointmentNo { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public AppointmentStatus Status { get; set; }
}

public class BookAppointmentViewModel
{
    [Required]
    public int DepartmentId { get; set; }
    
    [Required]
    public int DoctorId { get; set; }
    
    [Required]
    [DataType(DataType.Date)]
    public DateOnly AppointmentDate { get; set; }
    
    [Required]
    public string Symptoms { get; set; } = string.Empty;
}
