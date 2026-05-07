using System.ComponentModel.DataAnnotations;
using MediCareMS.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediCareMS.Models.ViewModels.Appointment;

public class AppointmentListViewModel
{
    public int Id { get; set; }
    public string AppointmentNo { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateOnly AppointmentDate { get; set; }
    public TimeOnly AppointmentTime { get; set; }
    public int? TokenNumber { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? ChiefComplaint { get; set; }
}

public class AppointmentCreateViewModel
{
    public int Id { get; set; }
    [Required] public int PatientId { get; set; }
    [Required] public int DoctorId { get; set; }
    [Required] public DateOnly AppointmentDate { get; set; }
    [Required] public TimeOnly AppointmentTime { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? Notes { get; set; }

    public List<SelectListItem> Patients { get; set; } = new();
    public List<SelectListItem> Doctors { get; set; } = new();
}

public class DashboardViewModel
{
    public int TotalPatients { get; set; }
    public int TotalDoctors { get; set; }
    public int TodayAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public List<RecentAppointmentItem> RecentAppointments { get; set; } = new();
    public List<DoctorAvailabilityItem> AvailableDoctors { get; set; } = new();
}

public class RecentAppointmentItem
{
    public string AppointmentNo { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public AppointmentStatus Status { get; set; }
}

public class DoctorAvailabilityItem
{
    public string DoctorName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DoctorStatus Status { get; set; }
    public decimal ConsultationFee { get; set; }
}
