using MediCareMS.Data;
using MediCareMS.Models.Entities.Appointment;
using MediCareMS.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace MediCareMS.Helpers.AI;

public class AgentActionService : IAgentActionService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AgentActionService> _logger;

    public AgentActionService(AppDbContext db, ILogger<AgentActionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ── 1. Search Doctors ──────────────────────────────────────────────────────
    public async Task<DoctorListResult> SearchDoctorsAsync(string? specialization, string? name, string? date)
    {
        var q = _db.Doctors
            .Include(d => d.Specialization)
            .Include(d => d.Department)
            .Where(d => !d.IsDeleted && d.Status != DoctorStatus.Inactive);

        if (!string.IsNullOrWhiteSpace(specialization))
            q = q.Where(d => d.Specialization.Name.Contains(specialization) ||
                              d.Department.Name.Contains(specialization));

        if (!string.IsNullOrWhiteSpace(name))
            q = q.Where(d => d.FullName.Contains(name));

        // If date provided, only show doctors with a schedule on that day
        if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var dt))
        {
            var dayOfWeek = (DayOfWeekEnum)((int)dt.DayOfWeek);
            q = q.Where(d => d.Schedules.Any(s => s.DayOfWeek == dayOfWeek && s.IsActive));
        }

        var doctors = await q
            .OrderBy(d => d.FullName)
            .Take(6)
            .Select(d => new DoctorCardDto
            {
                Id             = d.Id,
                Name           = d.FullName,
                Specialization = d.Specialization.Name,
                Department     = d.Department.Name,
                Qualification  = d.Qualification ?? "",
                ExperienceYears= d.ExperienceYears ?? 0,
                Fee            = d.ConsultationFee,
                Status         = d.Status.ToString(),
                ChamberAddress = d.ChamberAddress,
                MobileNumber   = d.MobileNumber
            })
            .ToListAsync();

        return new DoctorListResult
        {
            Doctors    = doctors,
            SearchTerm = specialization ?? name ?? "",
            Date       = date
        };
    }

    // ── 2. Get Available Slots ─────────────────────────────────────────────────
    public async Task<SlotListResult> GetAvailableSlotsAsync(int doctorId, string date)
    {
        var doctor = await _db.Doctors
            .Include(d => d.Schedules)
            .FirstOrDefaultAsync(d => d.Id == doctorId && !d.IsDeleted);

        if (doctor == null) return new SlotListResult { DoctorId = doctorId, Date = date };

        if (!DateOnly.TryParse(date, out var apptDate))
            return new SlotListResult { DoctorId = doctorId, DoctorName = doctor.FullName, Date = date };

        var dayOfWeek = (DayOfWeekEnum)((int)apptDate.DayOfWeek);
        var schedule  = doctor.Schedules.FirstOrDefault(s => s.DayOfWeek == dayOfWeek && s.IsActive);

        if (schedule == null)
            return new SlotListResult { DoctorId = doctorId, DoctorName = doctor.FullName, Date = date };

        // Get already booked slots
        var booked = await _db.Appointments
            .Where(a => a.DoctorId == doctorId &&
                        a.AppointmentDate == apptDate &&
                        !a.IsDeleted &&
                        a.Status != AppointmentStatus.Cancelled)
            .Select(a => a.AppointmentTime)
            .ToListAsync();

        // Generate slots
        var slots = new List<SlotDto>();
        var current = schedule.StartTime;
        while (current.Add(TimeSpan.FromMinutes(schedule.SlotDurationMinutes)) <= schedule.EndTime &&
               slots.Count < schedule.MaxPatients)
        {
            slots.Add(new SlotDto
            {
                Time        = current.ToString("HH:mm"),
                DisplayTime = current.ToString("h:mm tt"),
                IsAvailable = !booked.Contains(current)
            });
            current = current.Add(TimeSpan.FromMinutes(schedule.SlotDurationMinutes));
        }

        return new SlotListResult
        {
            DoctorId   = doctorId,
            DoctorName = doctor.FullName,
            Date       = date,
            Slots      = slots
        };
    }

    // ── 3. Create Appointment ─────────────────────────────────────────────────
    public async Task<(bool Success, AppointmentCardDto? Card, string Error)> CreateAppointmentAsync(
        int doctorId, string date, string time, string? chiefComplaint, int userId)
    {
        try
        {
            // Get patient for this user
            var patient = await _db.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
            if (patient == null)
                return (false, null, "No patient profile found. Please create a patient profile first.");

            if (!DateOnly.TryParse(date, out var apptDate))
                return (false, null, "Invalid date format.");

            if (!TimeOnly.TryParse(time, out var apptTime))
                return (false, null, "Invalid time format.");

            var doctor = await _db.Doctors
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.Id == doctorId && !d.IsDeleted);
            if (doctor == null) return (false, null, "Doctor not found.");

            // Check slot not already taken
            var conflict = await _db.Appointments.AnyAsync(a =>
                a.DoctorId == doctorId &&
                a.AppointmentDate == apptDate &&
                a.AppointmentTime == apptTime &&
                !a.IsDeleted &&
                a.Status != AppointmentStatus.Cancelled);

            if (conflict) return (false, null, "This time slot is already booked. Please choose another.");

            // Generate appointment number
            var apptNo = $"APT-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

            var appointment = new Appointment
            {
                AppointmentNo   = apptNo,
                PatientId       = patient.Id,
                DoctorId        = doctorId,
                AppointmentDate = apptDate,
                AppointmentTime = apptTime,
                ChiefComplaint  = chiefComplaint,
                Status          = AppointmentStatus.Pending,
                CreatedAt       = DateTime.UtcNow,
                CreatedBy       = userId
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            var card = new AppointmentCardDto
            {
                Id             = appointment.Id,
                AppointmentNo  = apptNo,
                DoctorName     = doctor.FullName,
                Specialization = doctor.Specialization.Name,
                Date           = apptDate.ToString("dddd, MMMM d, yyyy"),
                Time           = apptTime.ToString("h:mm tt"),
                Status         = "Pending",
                ChiefComplaint = chiefComplaint,
                Fee            = doctor.ConsultationFee
            };

            return (true, card, "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment");
            return (false, null, "Failed to create appointment. Please try again.");
        }
    }

    // ── 4. Cancel Appointment ──────────────────────────────────────────────────
    public async Task<(bool Success, string Message)> CancelAppointmentAsync(int appointmentId, int userId)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
        if (patient == null) return (false, "No patient profile found.");

        var appt = await _db.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patient.Id && !a.IsDeleted);

        if (appt == null) return (false, "Appointment not found or access denied.");
        if (appt.Status == AppointmentStatus.Cancelled) return (false, "This appointment is already cancelled.");

        appt.Status            = AppointmentStatus.Cancelled;
        appt.CancellationReason = "Cancelled via AI assistant";
        appt.UpdatedAt         = DateTime.UtcNow;
        appt.UpdatedBy         = userId;
        await _db.SaveChangesAsync();

        return (true, $"Appointment #{appt.AppointmentNo} has been cancelled successfully.");
    }

    // ── 5. Reschedule Appointment ──────────────────────────────────────────────
    public async Task<(bool Success, AppointmentCardDto? Card, string Error)> RescheduleAppointmentAsync(
        int appointmentId, string newDate, string newTime, int userId)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
        if (patient == null) return (false, null, "No patient profile found.");

        var appt = await _db.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patient.Id && !a.IsDeleted);

        if (appt == null) return (false, null, "Appointment not found or access denied.");
        if (!DateOnly.TryParse(newDate, out var nd)) return (false, null, "Invalid date.");
        if (!TimeOnly.TryParse(newTime, out var nt)) return (false, null, "Invalid time.");

        // Check conflict
        var conflict = await _db.Appointments.AnyAsync(a =>
            a.Id != appointmentId &&
            a.DoctorId == appt.DoctorId &&
            a.AppointmentDate == nd &&
            a.AppointmentTime == nt &&
            !a.IsDeleted &&
            a.Status != AppointmentStatus.Cancelled);

        if (conflict) return (false, null, "That slot is already taken. Please choose another time.");

        appt.AppointmentDate = nd;
        appt.AppointmentTime = nt;
        appt.Status          = AppointmentStatus.Pending;
        appt.UpdatedAt       = DateTime.UtcNow;
        appt.UpdatedBy       = userId;
        await _db.SaveChangesAsync();

        var card = new AppointmentCardDto
        {
            Id             = appt.Id,
            AppointmentNo  = appt.AppointmentNo,
            DoctorName     = appt.Doctor.FullName,
            Specialization = appt.Doctor.Specialization.Name,
            Date           = nd.ToString("dddd, MMMM d, yyyy"),
            Time           = nt.ToString("h:mm tt"),
            Status         = "Pending",
            Fee            = appt.Doctor.ConsultationFee
        };

        return (true, card, "");
    }

    // ── 6. Get My Appointments ─────────────────────────────────────────────────
    public async Task<AppointmentListResult> GetMyAppointmentsAsync(int userId)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
        if (patient == null) return new AppointmentListResult();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var list  = await _db.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
            .Where(a => a.PatientId == patient.Id && !a.IsDeleted && a.AppointmentDate >= today)
            .OrderBy(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime)
            .Take(10)
            .Select(a => new AppointmentCardDto
            {
                Id             = a.Id,
                AppointmentNo  = a.AppointmentNo,
                DoctorName     = a.Doctor.FullName,
                Specialization = a.Doctor.Specialization.Name,
                Date           = a.AppointmentDate.ToString("dddd, MMM d, yyyy"),
                Time           = a.AppointmentTime.ToString("h:mm tt"),
                Status         = a.Status.ToString(),
                ChiefComplaint = a.ChiefComplaint,
                Fee            = a.Doctor.ConsultationFee
            })
            .ToListAsync();

        return new AppointmentListResult { Appointments = list };
    }

    // ── 7. Get Patient Profile ─────────────────────────────────────────────────
    public async Task<PatientProfileDto?> GetPatientProfileAsync(int userId)
    {
        var p = await _db.Patients.FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
        if (p == null) return null;

        return new PatientProfileDto
        {
            Id           = p.Id,
            PatientNo    = p.PatientNo,
            FullName     = p.FullName,
            MobileNumber = p.MobileNumber,
            Email        = p.Email,
            Gender       = p.Gender.ToString(),
            DateOfBirth  = p.DateOfBirth.ToString("MMMM d, yyyy"),
            BloodGroup   = p.BloodGroup?.ToString(),
            Address      = p.Address
        };
    }
}
