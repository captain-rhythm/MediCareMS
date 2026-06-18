using System.Text.Json;

namespace MediCareMS.Helpers.AI;

/// <summary>
/// Returns the 7 Groq/OpenAI-compatible tool (function) definitions
/// that the AI agent can call during a conversation.
/// </summary>
public static class AgentToolDefinitions
{
    public static IReadOnlyList<object> All => _tools;

    private static readonly List<object> _tools =
    [
        Tool("search_doctors",
             "Search for doctors by medical specialization, doctor name, or availability on a specific date. Use this when the user wants to find or see a doctor.",
             new
             {
                 type = "object",
                 properties = new
                 {
                     specialization = new { type = "string", description = "Medical specialization e.g. Cardiologist, Dermatologist, Neurologist, Orthopedic, ENT, Gynecologist, Pediatrician, General Physician" },
                     name           = new { type = "string", description = "Doctor's full name or partial name" },
                     date           = new { type = "string", description = "Date in YYYY-MM-DD format to filter by availability" }
                 },
                 required = Array.Empty<string>()
             }),

        Tool("get_available_slots",
             "Get available appointment time slots for a specific doctor on a specific date. Always call this after the user picks a doctor, before booking.",
             new
             {
                 type = "object",
                 properties = new
                 {
                     doctor_id = new { type = "integer", description = "The numeric ID of the doctor" },
                     date      = new { type = "string",  description = "The appointment date in YYYY-MM-DD format" }
                 },
                 required = new[] { "doctor_id", "date" }
             }),

        Tool("create_appointment",
             "Book a new appointment for the patient. Only call this after confirming doctor, date, and time with the user.",
             new
             {
                 type = "object",
                 properties = new
                 {
                     doctor_id       = new { type = "integer", description = "Doctor's numeric ID" },
                     date            = new { type = "string",  description = "Appointment date YYYY-MM-DD" },
                     time            = new { type = "string",  description = "Appointment time HH:mm (24-hour format)" },
                     chief_complaint = new { type = "string",  description = "Patient's main reason for the visit (optional)" }
                 },
                 required = new[] { "doctor_id", "date", "time" }
             }),

        Tool("cancel_appointment",
             "Cancel an existing appointment by its ID. Ask the user to confirm before cancelling.",
             new
             {
                 type = "object",
                 properties = new
                 {
                     appointment_id = new { type = "integer", description = "The numeric ID of the appointment to cancel" }
                 },
                 required = new[] { "appointment_id" }
             }),

        Tool("reschedule_appointment",
             "Reschedule an existing appointment to a new date and time.",
             new
             {
                 type = "object",
                 properties = new
                 {
                     appointment_id = new { type = "integer", description = "The appointment ID to reschedule" },
                     new_date       = new { type = "string",  description = "New date in YYYY-MM-DD format" },
                     new_time       = new { type = "string",  description = "New time in HH:mm (24-hour) format" }
                 },
                 required = new[] { "appointment_id", "new_date", "new_time" }
             }),

        Tool("get_my_appointments",
             "Retrieve the patient's upcoming appointments. Use when the user asks to view, check, or list their appointments.",
             new
             {
                 type       = "object",
                 properties = new { },
                 required   = Array.Empty<string>()
             }),

        Tool("get_patient_profile",
             "Retrieve the patient's profile information including name, contact, blood group, and medical history overview.",
             new
             {
                 type       = "object",
                 properties = new { },
                 required   = Array.Empty<string>()
             })
    ];

    private static object Tool(string name, string description, object parameters) => new
    {
        type     = "function",
        function = new { name, description, parameters }
    };

    /// <summary>Serialize all tools to JSON string for logging/debugging.</summary>
    public static string ToJson() =>
        JsonSerializer.Serialize(_tools, new JsonSerializerOptions { WriteIndented = true });
}
