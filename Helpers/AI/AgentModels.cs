namespace MediCareMS.Helpers.AI;

/// <summary>Extended AI response that carries structured action data for rich UI rendering.</summary>
public class AgentResponse : AIResponse
{
    /// <summary>UI action type: doctor_list | slot_picker | appointment_confirmed | appointment_list | patient_profile | none</summary>
    public string ActionType { get; set; } = "none";

    /// <summary>Serialized JSON payload for the frontend rich card.</summary>
    public object? ActionData { get; set; }
}

// ── Doctor Search ─────────────────────────────────────────────────────────────
public class DoctorCardDto
{
    public int    Id               { get; set; }
    public string Name             { get; set; } = "";
    public string Specialization   { get; set; } = "";
    public string Department       { get; set; } = "";
    public string Qualification    { get; set; } = "";
    public int    ExperienceYears  { get; set; }
    public decimal Fee             { get; set; }
    public string Status           { get; set; } = "";
    public string? ChamberAddress  { get; set; }
    public string? MobileNumber    { get; set; }
}

public class DoctorListResult
{
    public List<DoctorCardDto> Doctors    { get; set; } = [];
    public string              SearchTerm { get; set; } = "";
    public string?             Date       { get; set; }
}

// ── Slot Picker ───────────────────────────────────────────────────────────────
public class SlotDto
{
    public string Time          { get; set; } = "";   // "HH:mm"
    public string DisplayTime   { get; set; } = "";   // "10:00 AM"
    public bool   IsAvailable   { get; set; } = true;
}

public class SlotListResult
{
    public int            DoctorId   { get; set; }
    public string         DoctorName { get; set; } = "";
    public string         Date       { get; set; } = "";
    public List<SlotDto>  Slots      { get; set; } = [];
}

// ── Appointment ───────────────────────────────────────────────────────────────
public class AppointmentCardDto
{
    public int    Id             { get; set; }
    public string AppointmentNo  { get; set; } = "";
    public string DoctorName     { get; set; } = "";
    public string Specialization { get; set; } = "";
    public string Date           { get; set; } = "";
    public string Time           { get; set; } = "";
    public string Status         { get; set; } = "";
    public string? ChiefComplaint{ get; set; }
    public decimal Fee           { get; set; }
}

public class AppointmentListResult
{
    public List<AppointmentCardDto> Appointments { get; set; } = [];
}

// ── Patient Profile ───────────────────────────────────────────────────────────
public class PatientProfileDto
{
    public int    Id            { get; set; }
    public string PatientNo     { get; set; } = "";
    public string FullName      { get; set; } = "";
    public string? MobileNumber { get; set; }
    public string? Email        { get; set; }
    public string? Gender       { get; set; }
    public string? DateOfBirth  { get; set; }
    public string? BloodGroup   { get; set; }
    public string? Address      { get; set; }
}
