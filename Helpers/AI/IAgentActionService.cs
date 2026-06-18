namespace MediCareMS.Helpers.AI;

/// <summary>
/// Provides database-backed actions the AI agent can call.
/// All mutation methods validate that the acting userId owns the resource.
/// </summary>
public interface IAgentActionService
{
    /// <summary>Search doctors by specialization, name, and/or date availability.</summary>
    Task<DoctorListResult> SearchDoctorsAsync(string? specialization, string? name, string? date);

    /// <summary>Return available time slots for a doctor on a given date.</summary>
    Task<SlotListResult> GetAvailableSlotsAsync(int doctorId, string date);

    /// <summary>Create a new appointment for the authenticated patient.</summary>
    Task<(bool Success, AppointmentCardDto? Card, string Error)> CreateAppointmentAsync(
        int doctorId, string date, string time, string? chiefComplaint, int userId);

    /// <summary>Cancel an appointment (soft-cancel). Validates ownership.</summary>
    Task<(bool Success, string Message)> CancelAppointmentAsync(int appointmentId, int userId);

    /// <summary>Reschedule an existing appointment to a new date/time.</summary>
    Task<(bool Success, AppointmentCardDto? Card, string Error)> RescheduleAppointmentAsync(
        int appointmentId, string newDate, string newTime, int userId);

    /// <summary>List upcoming/recent appointments for a user.</summary>
    Task<AppointmentListResult> GetMyAppointmentsAsync(int userId);

    /// <summary>Retrieve the patient profile linked to a user account.</summary>
    Task<PatientProfileDto?> GetPatientProfileAsync(int userId);
}
