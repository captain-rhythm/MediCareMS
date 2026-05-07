using System.ComponentModel.DataAnnotations;
using MediCareMS.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediCareMS.Models.ViewModels.Doctor;

public class DoctorListViewModel
{
    public int Id { get; set; }
    public string DoctorNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? Qualification { get; set; }
    public int? ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }
    public DoctorStatus Status { get; set; }
    public string? ProfileImagePath { get; set; }
}

public class DoctorCreateEditViewModel
{
    public int Id { get; set; }

    [Required] public string FullName { get; set; } = string.Empty;
    [Required] public int DepartmentId { get; set; }
    [Required] public int SpecializationId { get; set; }
    public string? Qualification { get; set; }
    public int? ExperienceYears { get; set; }
    public string? BmdcRegNo { get; set; }
    [Required] public string MobileNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    [Required] public decimal ConsultationFee { get; set; }
    public string? ChamberAddress { get; set; }
    public string? Bio { get; set; }
    public DoctorStatus Status { get; set; } = DoctorStatus.Available;
    public IFormFile? ProfileImage { get; set; }
    public string? ExistingProfileImagePath { get; set; }

    public List<SelectListItem> Departments { get; set; } = new();
    public List<SelectListItem> Specializations { get; set; } = new();
}
