using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MediCareMS.Models.Entities.Chat;
using Microsoft.Extensions.Caching.Memory;

namespace MediCareMS.Helpers.AI;

/// <summary>
/// AI Service implementation using Groq's OpenAI-compatible API.
/// Groq provides fast, free inference for Llama 3 and other open models.
/// Endpoint: https://api.groq.com/openai/v1/chat/completions
/// </summary>
public class GroqAIService : IAIService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GroqAIService> _logger;

    private static readonly string[] EmergencyKeywords =
    [
        "chest pain", "heart attack", "stroke", "severe bleeding", "can't breathe",
        "cannot breathe", "breathing difficulty", "loss of consciousness", "unconscious",
        "suicidal", "suicide", "kill myself", "want to die",
        // Bangla emergency keywords
        "বুকে ব্যথা", "হার্ট অ্যাটাক", "স্ট্রোক", "শ্বাস নিতে পারছি না", "অজ্ঞান", "আত্মহত্যা"
    ];

    private const string SystemPrompt = """
        You are MediCare AI Assistant — a helpful, empathetic healthcare information assistant.

        CRITICAL RULES (never break these):
        1. You are NOT a doctor and NEVER diagnose diseases.
        2. NEVER prescribe medicines, dosages, or treatments.
        3. NEVER provide specific drug names or quantities.
        4. Always encourage professional medical consultation.
        5. Keep responses clear, concise, and easy to understand.

        YOUR ROLE:
        - Provide general educational health information only.
        - Ask thoughtful follow-up questions to understand symptoms better.
        - Suggest appropriate medical specialists based on symptoms.
        - Explain symptoms in simple, non-technical language.
        - Be warm, empathetic, and supportive.

        SPECIALIST RECOMMENDATION GUIDE:
        - Heart / chest issues → Cardiologist
        - Brain / nerve / headache → Neurologist
        - Skin conditions → Dermatologist
        - Bone / joint / muscle → Orthopedic Specialist
        - Ear / nose / throat → ENT Specialist
        - Women's health → Gynecologist
        - Mental health / anxiety / depression → Psychiatrist
        - Stomach / digestive → Gastroenterologist
        - Children's health → Pediatrician
        - General / unclear symptoms → General Physician

        LANGUAGE RULE:
        - Detect the language of the user's message.
        - If the user writes in Bangla (Bengali), respond entirely in Bangla.
        - If the user writes in English, respond in English.
        - Always match the user's language.

        FORMAT:
        - Use short paragraphs (2-3 sentences max).
        - Use bullet points for lists of symptoms or specialists.
        - End with a helpful question or suggestion to consult a doctor.
        - Keep responses under 200 words unless more detail is clearly needed.

        DISCLAIMER (include naturally, not robotically):
        Always remind users this is educational information and they should consult a real doctor.
        """;

    private const string EmergencyResponse =
        "🚨 **This may be a medical emergency.**\n\n" +
        "Please **immediately**:\n" +
        "- Call emergency services (999 in Bangladesh / 911 in USA)\n" +
        "- Go to the nearest hospital emergency room\n" +
        "- Do not wait or delay\n\n" +
        "*If someone else is with you, ask them to help right away.*";

    private const string EmergencyResponseBangla =
        "🚨 **এটি একটি জরুরি চিকিৎসা পরিস্থিতি হতে পারে।**\n\n" +
        "অনুগ্রহ করে **এখনই**:\n" +
        "- জরুরি সেবায় কল করুন (বাংলাদেশে ৯৯৯)\n" +
        "- নিকটতম হাসপাতালের জরুরি বিভাগে যান\n" +
        "- দেরি করবেন না\n\n" +
        "*যদি কেউ কাছে থাকে, তাদের সাহায্য নিন।*";

    public GroqAIService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        IMemoryCache cache,
        ILogger<GroqAIService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AIResponse> GetResponseAsync(string userMessage, List<ChatMessage> history, int userId)
    {
        // Rate Limiting: max 60 requests per user per hour
        var rateLimitKey = $"chat_rate_{userId}";
        var requestCount = _cache.GetOrCreate(rateLimitKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return 0;
        });

        if (requestCount >= 60)
        {
            return new AIResponse
            {
                Message = "You've reached the maximum of 60 AI requests per hour. Please try again later.",
                IsRateLimited = true
            };
        }

        _cache.Set(rateLimitKey, requestCount + 1, TimeSpan.FromHours(1));

        // Input Sanitization
        var sanitized = SanitizeInput(userMessage);

        // Emergency Detection
        bool isBangla = ContainsBangla(sanitized);
        if (IsEmergency(sanitized))
        {
            return new AIResponse
            {
                Message = isBangla ? EmergencyResponseBangla : EmergencyResponse,
                IsEmergency = true
            };
        }

        // Build Groq API request (OpenAI-compatible format)
        try
        {
            var apiKey = _config["Groq:ApiKey"] ?? throw new InvalidOperationException("Groq API key not configured.");
            var model   = _config["Groq:Model"]  ?? "llama-3.3-70b-versatile";
            const string url = "https://api.groq.com/openai/v1/chat/completions";

            // Build messages array: system prompt + conversation history + current message
            var messages = new List<object>
            {
                new { role = "system", content = SystemPrompt }
            };

            // Add last 10 messages for context
            foreach (var msg in history.TakeLast(10))
            {
                messages.Add(new
                {
                    role    = msg.Sender == ChatSender.User ? "user" : "assistant",
                    content = msg.Message
                });
            }

            // Add current user message
            messages.Add(new { role = "user", content = sanitized });

            var requestBody = new
            {
                model,
                messages,
                max_tokens  = int.TryParse(_config["Groq:MaxTokens"], out var t) ? t : 1024,
                temperature = 0.7,
                top_p       = 0.9,
                stream      = false
            };

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var json    = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response     = await client.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Handle rate limiting (429)
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("Groq rate limit hit for user {UserId}", userId);
                return new AIResponse
                {
                    Message = "⏳ **The AI is momentarily busy.** Please wait a few seconds and try again."
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Groq API error {Status}: {Body}", response.StatusCode, responseBody);
                return new AIResponse
                {
                    Message = $"⚠️ AI service error ({(int)response.StatusCode}). Please try again in a moment."
                };
            }

            // Parse OpenAI-compatible response
            using var doc = JsonDocument.Parse(responseBody);
            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "I couldn't generate a response. Please try again.";

            return new AIResponse { Message = text.Trim() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Groq API");
            return new AIResponse { Message = "I'm experiencing technical difficulties. Please try again shortly." };
        }
    }

    private static bool IsEmergency(string message)
    {
        var lower = message.ToLowerInvariant();
        return EmergencyKeywords.Any(kw => lower.Contains(kw.ToLowerInvariant()));
    }

    private static bool ContainsBangla(string text)
        => text.Any(c => c >= '\u0980' && c <= '\u09FF');

    private static string SanitizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        input = input.Trim();
        if (input.Length > 2000) input = input[..2000];
        input = Regex.Replace(input, "<[^>]+>", string.Empty);
        var injectionPatterns = new[]
        {
            @"(?i)(ignore previous|ignore above|disregard|forget your|new instruction|system:)",
        };
        foreach (var pattern in injectionPatterns)
            input = Regex.Replace(input, pattern, "[removed]");
        return input;
    }
}
