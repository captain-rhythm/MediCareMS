using System.ComponentModel.DataAnnotations;
using MediCareMS.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediCareMS.Models.ViewModels.Patient;

public class PatientListViewModel
{
    public int Id { get; set; }
    public string PatientNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? MobileNumber { get; set; }
    public Gender Gender { get; set; }
    public BloodGroup? BloodGroup { get; set; }
    public int Age { get; set; }
    public int TotalVisits { get; set; }
}

public class PatientCreateEditViewModel
{
    public int Id { get; set; }
    [Required] public string FullName { get; set; } = string.Empty;
    [Required] public DateTime DateOfBirth { get; set; }
    [Required] public Gender Gender { get; set; }
    public BloodGroup? BloodGroup { get; set; }
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Nationality { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? KnownAllergies { get; set; }
    public string? ChronicConditions { get; set; }
}

public class PatientDetailViewModel
{
    public int Id { get; set; }
    public string PatientNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int Age => (int)((DateTime.Today - DateOfBirth).TotalDays / 365.25);
    public Gender Gender { get; set; }
    public BloodGroup? BloodGroup { get; set; }
    public string? MobileNumber { get; set; }
    public string? Address { get; set; }
    public string? KnownAllergies { get; set; }
    public string? ChronicConditions { get; set; }
    public List<AppointmentSummary> RecentAppointments { get; set; } = new();
}

public class AppointmentSummary
{
    public string AppointmentNo { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public AppointmentStatus Status { get; set; }
}
