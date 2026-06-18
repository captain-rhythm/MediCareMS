using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using MediCareMS.Models.Entities.Chat;
using Microsoft.Extensions.Caching.Memory;

namespace MediCareMS.Helpers.AI;

/// <summary>
/// AI Agent using a reliable 2-step pattern:
///   Step 1 — Ask Groq to detect intent and extract parameters (returns JSON).
///   Step 2 — Execute the action via AgentActionService, then ask Groq to
///             generate a natural-language response with the results injected.
/// This avoids Groq tool-calling API edge cases entirely.
/// </summary>
public class AgentAIService
{
    private readonly IHttpClientFactory      _http;
    private readonly IConfiguration          _config;
    private readonly IMemoryCache            _cache;
    private readonly IAgentActionService     _actions;
    private readonly ILogger<AgentAIService> _logger;

    private const string GroqUrl = "https://api.groq.com/openai/v1/chat/completions";

    private static readonly string[] EmergencyKeywords =
    [
        "chest pain", "heart attack", "stroke", "severe bleeding", "can't breathe",
        "cannot breathe", "unconscious", "suicidal", "suicide", "kill myself",
        "বুকে ব্যথা", "হার্ট অ্যাটাক", "স্ট্রোক", "শ্বাস নিতে পারছি না", "আত্মহত্যা"
    ];

    private const string EmergencyMsg =
        "🚨 **Medical Emergency Detected.**\n\n" +
        "Please **immediately**:\n" +
        "- Call **999** (Bangladesh) or **911** (USA)\n" +
        "- Go to the nearest **hospital emergency room**\n\n" +
        "*Do not delay — call for help right now.*";

    // ── Step 1: Intent detection prompt ──────────────────────────────────────
    private static string IntentPrompt(string today) => $$"""
        You are an intent classifier for MediCare AI Agent.
        Today's date: {{today}}

        Analyze the user's message and respond with ONLY a JSON object (no markdown, no explanation):

        {
          "intent": "<one of: search_doctors | get_slots | create_appointment | cancel_appointment | reschedule_appointment | get_appointments | get_profile | general_chat>",
          "params": {
            "specialization": "<e.g. Cardiologist, Dermatologist — only if mentioned>",
            "doctor_name": "<partial doctor name if mentioned>",
            "doctor_id": <number if mentioned, else null>,
            "date": "<YYYY-MM-DD, resolve 'tomorrow'/'Friday'/etc from today's date>",
            "time": "<HH:mm 24-hour if mentioned>",
            "appointment_id": <number if user references a specific appointment, else null>,
            "new_date": "<YYYY-MM-DD for reschedule>",
            "new_time": "<HH:mm for reschedule>",
            "chief_complaint": "<reason for visit if mentioned>"
          },
          "clarification_needed": "<empty string if params are sufficient, otherwise a question to ask the user>"
        }

        Rules:
        - If user wants a doctor/specialist/appointment => intent = search_doctors
        - If user wants to see available slots/times => intent = get_slots
        - If user wants to book/schedule => intent = create_appointment
        - If user mentions cancel => intent = cancel_appointment
        - If user mentions reschedule/change time => intent = reschedule_appointment
        - If user asks about their appointments/bookings => intent = get_appointments
        - If user asks about their profile/info => intent = get_profile
        - Otherwise => intent = general_chat
        - For relative dates: "tomorrow" = {{DateTime.Today.AddDays(1).ToString("yyyy-MM-dd")}}, resolve day names from today
        - Only set params that were clearly stated. Leave others null/empty.
        """;

    // ── Step 2: Response generation prompt ───────────────────────────────────
    private const string ResponsePrompt = """
        You are Aletta — a friendly, professional AI healthcare agent for MediCare Management System.

        You help patients:
        - Find and book doctors
        - Manage their appointments
        - View their health profile

        Rules:
        - Never diagnose or prescribe medicine
        - Be warm, concise, and professional
        - Use markdown for formatting
        - Respond in the user's language (English or Bangla)
        - For emergencies direct to 999 immediately

        The system has already executed the requested action. Use the CONTEXT provided to write a helpful response.
        """;

    public AgentAIService(
        IHttpClientFactory      http,
        IConfiguration          config,
        IMemoryCache            cache,
        IAgentActionService     actions,
        ILogger<AgentAIService> logger)
    {
        _http    = http;
        _config  = config;
        _cache   = cache;
        _actions = actions;
        _logger  = logger;
    }

    public async Task<AgentResponse> ProcessAsync(
        string           userMessage,
        List<ChatMessage> history,
        int              userId)
    {
        // ── Rate limit ────────────────────────────────────────────────────────
        var rKey  = $"agent_rate_{userId}";
        var count = _cache.GetOrCreate(rKey, e => { e.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1); return 0; });
        if (count >= 80) return new AgentResponse { Message = "You've reached the hourly limit. Please try again later.", IsRateLimited = true };
        _cache.Set(rKey, count + 1, TimeSpan.FromHours(1));

        var clean = Sanitize(userMessage);
        if (IsEmergency(clean)) return new AgentResponse { Message = EmergencyMsg, IsEmergency = true };

        var apiKey = _config["Groq:ApiKey"] ?? throw new InvalidOperationException("Groq:ApiKey missing.");
        var model  = _config["Groq:Model"] ?? "llama-3.3-70b-versatile";
        var maxTok = int.TryParse(_config["Groq:MaxTokens"], out var t) ? t : 1024;
        var today  = DateTime.Today.ToString("yyyy-MM-dd");

        try
        {
            // ══ STEP 1: Detect intent ═════════════════════════════════════════
            var intentMessages = new List<object>
            {
                new { role = "system", content = IntentPrompt(today) },
                new { role = "user",   content = clean }
            };

            var intentJson = await CallGroq(apiKey, model, 512, intentMessages, temperature: 0.1);
            if (intentJson == null) return FallbackResponse(history, clean, apiKey, model, maxTok).Result;

            // Parse intent
            var intentText = intentJson["choices"]?[0]?["message"]?["content"]?.GetValue<string>() ?? "{}";
            intentText     = ExtractJson(intentText);

            JsonNode? intent = null;
            try { intent = JsonNode.Parse(intentText); } catch { }

            var intentName = intent?["intent"]?.GetValue<string>() ?? "general_chat";
            var clarify    = intent?["params"] != null
                ? intent["clarification_needed"]?.GetValue<string>() ?? ""
                : "";

            // If clarification needed, just ask
            if (!string.IsNullOrWhiteSpace(clarify))
                return new AgentResponse { Message = clarify };

            // ══ STEP 2: Execute action ════════════════════════════════════════
            var (actionContext, richResponse) = await ExecuteIntent(intentName, intent?["params"], userId);

            // ══ STEP 3: Generate natural language response ════════════════════
            var historySnippet = string.Join("\n", history.TakeLast(6)
                .Where(m => !m.Message.StartsWith("[RATING:"))
                .Select(m => $"{(m.Sender == ChatSender.User ? "User" : "Aletta")}: {m.Message}"));

            var contextMsg = $"""
                Conversation so far:
                {historySnippet}

                User just said: "{clean}"

                Action result:
                {actionContext}

                Write a helpful, concise response in 2-4 sentences. If showing a list of items, keep it brief — the UI will display the full cards.
                """;

            var responseMessages = new List<object>
            {
                new { role = "system", content = ResponsePrompt },
                new { role = "user",   content = contextMsg }
            };

            var finalJson = await CallGroq(apiKey, model, maxTok, responseMessages, temperature: 0.7);
            var finalText = finalJson?["choices"]?[0]?["message"]?["content"]?.GetValue<string>()?.Trim();

            if (string.IsNullOrWhiteSpace(finalText))
                finalText = DefaultMessage(intentName, richResponse?.ActionType);

            if (richResponse != null)
            {
                richResponse.Message = finalText;
                return richResponse;
            }

            return new AgentResponse { Message = finalText };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AgentAIService error userId={UserId}", userId);
            return new AgentResponse { Message = "I encountered a temporary error. Please try again." };
        }
    }

    // ── Execute intent → return (text context for AI, rich UI response) ───────
    private async Task<(string Context, AgentResponse? Rich)> ExecuteIntent(
        string intentName, JsonNode? p, int userId)
    {
        string Get(string key)  => p?[key]?.GetValue<string>() ?? "";
        int    GetInt(string k) {
            if (p?[k] is JsonNode n) {
                try { return n.GetValue<int>(); } catch { }
                if (int.TryParse(n.GetValue<string?>(), out var v)) return v;
            }
            return 0;
        }

        switch (intentName)
        {
            case "search_doctors":
            {
                var spec   = Get("specialization");
                var name   = Get("doctor_name");
                var date   = Get("date");
                var result = await _actions.SearchDoctorsAsync(
                    string.IsNullOrEmpty(spec) ? null : spec,
                    string.IsNullOrEmpty(name) ? null : name,
                    string.IsNullOrEmpty(date) ? null : date);
                var ctx  = result.Doctors.Count == 0
                    ? "No doctors found matching the criteria."
                    : $"Found {result.Doctors.Count} doctor(s): " +
                      string.Join("; ", result.Doctors.Select(d => $"{d.Name} ({d.Specialization}, ৳{d.Fee})"));
                return (ctx, new AgentResponse { ActionType = "doctor_list", ActionData = result });
            }

            case "get_slots":
            {
                var did      = GetInt("doctor_id");
                var docName  = Get("doctor_name");
                var date     = Get("date");

                // If no date provided, use today
                if (string.IsNullOrEmpty(date))
                    date = DateTime.Today.ToString("yyyy-MM-dd");

                // If we have a name but no ID, look up the doctor
                if (did == 0 && !string.IsNullOrEmpty(docName))
                {
                    var found = await _actions.SearchDoctorsAsync(null, docName, null);
                    if (found.Doctors.Count == 0)
                        return ($"No doctor found with the name '{docName}'. Please check the name and try again.", null);
                    if (found.Doctors.Count > 1)
                    {
                        // Multiple matches — show list to let user pick
                        return ($"Found {found.Doctors.Count} doctors matching '{docName}'. Showing doctor list so you can pick.",
                            new AgentResponse { ActionType = "doctor_list", ActionData = found });
                    }
                    did = found.Doctors[0].Id;
                }

                if (did == 0)
                    return ("I need a doctor name or ID to show available slots. Which doctor would you like to see?", null);

                var result = await _actions.GetAvailableSlotsAsync(did, date);
                if (result.Slots.Count == 0)
                    return ($"Dr. {result.DoctorName} has no schedule on {date}. Try a different date.", null);

                var avail = result.Slots.Count(s => s.IsAvailable);
                var ctx   = $"Dr. {result.DoctorName} has {avail} available slot(s) on {date}.";
                return (ctx, new AgentResponse { ActionType = "slot_picker", ActionData = result });
            }

            case "create_appointment":
            {
                var did       = GetInt("doctor_id");
                var docName   = Get("doctor_name");
                var date      = Get("date");
                var time      = Get("time");
                var complaint = Get("chief_complaint");

                // Resolve doctor by name if no ID
                if (did == 0 && !string.IsNullOrEmpty(docName))
                {
                    var found = await _actions.SearchDoctorsAsync(null, docName, null);
                    if (found.Doctors.Count == 1)
                        did = found.Doctors[0].Id;
                    else if (found.Doctors.Count > 1)
                        return ($"Found multiple doctors named '{docName}'. Please be more specific.",
                            new AgentResponse { ActionType = "doctor_list", ActionData = found });
                }

                if (did == 0 || string.IsNullOrEmpty(date) || string.IsNullOrEmpty(time))
                    return ("I need the doctor, date, and time to book. Which doctor, date, and time slot would you like?", null);

                var (ok, card, err) = await _actions.CreateAppointmentAsync(did, date, time,
                    string.IsNullOrEmpty(complaint) ? null : complaint, userId);
                if (!ok) return (err, null);
                var ctx = $"Appointment booked: {card!.AppointmentNo} with {card.DoctorName} on {card.Date} at {card.Time}.";
                return (ctx, new AgentResponse { ActionType = "appointment_confirmed", ActionData = card });
            }

            case "cancel_appointment":
            {
                var aid = GetInt("appointment_id");
                if (aid == 0) return ("Please specify which appointment to cancel (appointment number/ID).", null);
                var (ok, msg) = await _actions.CancelAppointmentAsync(aid, userId);
                return (msg, null);
            }

            case "reschedule_appointment":
            {
                var aid = GetInt("appointment_id");
                var nd  = Get("new_date");
                var nt  = Get("new_time");
                if (aid == 0 || string.IsNullOrEmpty(nd) || string.IsNullOrEmpty(nt))
                    return ("Need appointment ID, new date, and new time to reschedule.", null);
                var (ok, card, err) = await _actions.RescheduleAppointmentAsync(aid, nd, nt, userId);
                if (!ok) return (err, null);
                var ctx = $"Appointment rescheduled to {card!.Date} at {card.Time}.";
                return (ctx, new AgentResponse { ActionType = "appointment_confirmed", ActionData = card });
            }

            case "get_appointments":
            {
                var result = await _actions.GetMyAppointmentsAsync(userId);
                var ctx    = result.Appointments.Count == 0
                    ? "No upcoming appointments found."
                    : $"{result.Appointments.Count} upcoming appointment(s) found.";
                return (ctx, new AgentResponse { ActionType = "appointment_list", ActionData = result });
            }

            case "get_profile":
            {
                var profile = await _actions.GetPatientProfileAsync(userId);
                if (profile == null) return ("No patient profile found for this account.", null);
                var ctx = $"Patient: {profile.FullName}, Blood: {profile.BloodGroup ?? "N/A"}, DOB: {profile.DateOfBirth}.";
                return (ctx, new AgentResponse { ActionType = "patient_profile", ActionData = profile });
            }

            default:
                return ("General conversation — no specific action taken.", null);
        }
    }

    // ── Groq API call ─────────────────────────────────────────────────────────
    private async Task<JsonNode?> CallGroq(
        string         apiKey,
        string         model,
        int            maxTokens,
        List<object>   messages,
        double         temperature = 0.7)
    {
        var body = new
        {
            model,
            messages,
            max_tokens  = maxTokens,
            temperature,
            stream      = false
        };

        var client = _http.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(40);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var content  = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(GroqUrl, content);
        var json     = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Groq {Status}: {Body}", response.StatusCode, json);
            return null;
        }

        return JsonNode.Parse(json);
    }

    // ── Fallback: plain AI response if intent fails ───────────────────────────
    private async Task<AgentResponse> FallbackResponse(
        List<ChatMessage> history, string message, string apiKey, string model, int maxTok)
    {
        var messages = new List<object> { new { role = "system", content = ResponsePrompt } };
        foreach (var h in history.TakeLast(8).Where(m => !m.Message.StartsWith("[RATING:")))
            messages.Add(new { role = h.Sender == ChatSender.User ? "user" : "assistant", content = h.Message });
        messages.Add(new { role = "user", content = message });

        var r    = await CallGroq(apiKey, model, maxTok, messages);
        var text = r?["choices"]?[0]?["message"]?["content"]?.GetValue<string>()?.Trim()
                   ?? "I'm here to help. What would you like to do?";
        return new AgentResponse { Message = text };
    }

    private static string DefaultMessage(string intent, string? actionType) => actionType switch
    {
        "doctor_list"           => "Here are the doctors I found! Click **Book** to see available slots.",
        "slot_picker"           => "Here are the available time slots. Click one to book.",
        "appointment_confirmed" => "Your appointment has been confirmed! ✅",
        "appointment_list"      => "Here are your upcoming appointments.",
        "patient_profile"       => "Here is your patient profile.",
        _                       => "I'm here to help! What would you like to do?"
    };

    // ── Extract JSON from a string that may have markdown fences ─────────────
    private static string ExtractJson(string text)
    {
        var match = Regex.Match(text, @"\{[\s\S]*\}", RegexOptions.Multiline);
        return match.Success ? match.Value : text.Trim();
    }

    private static bool IsEmergency(string m)
    {
        var lower = m.ToLowerInvariant();
        return EmergencyKeywords.Any(k => lower.Contains(k.ToLowerInvariant()));
    }

    private static string Sanitize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        input = input.Trim();
        if (input.Length > 2000) input = input[..2000];
        input = Regex.Replace(input, "<[^>]+>", "");
        input = Regex.Replace(input, @"(?i)(ignore previous|ignore above|disregard|new instruction|system:)", "[removed]");
        return input;
    }
}
