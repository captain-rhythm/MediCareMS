using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MediCareMS.Models.Entities.Chat;
using Microsoft.Extensions.Caching.Memory;

namespace MediCareMS.Helpers.AI;

public class GeminiAIService : IAIService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GeminiAIService> _logger;

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

    public GeminiAIService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        IMemoryCache cache,
        ILogger<GeminiAIService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AIResponse> GetResponseAsync(string userMessage, List<ChatMessage> history, int userId)
    {
        // --- Rate Limiting: max 30 requests per user per hour ---
        var rateLimitKey = $"chat_rate_{userId}";
        var requestCount = _cache.GetOrCreate(rateLimitKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return 0;
        });

        if (requestCount >= 30)
        {
            return new AIResponse
            {
                Message = "You've reached the maximum of 30 AI requests per hour. Please try again later.",
                IsRateLimited = true
            };
        }

        _cache.Set(rateLimitKey, requestCount + 1, TimeSpan.FromHours(1));

        // --- Input Sanitization ---
        var sanitized = SanitizeInput(userMessage);

        // --- Emergency Detection ---
        bool isBangla = ContainsBangla(sanitized);
        if (IsEmergency(sanitized))
        {
            return new AIResponse
            {
                Message = isBangla ? EmergencyResponseBangla : EmergencyResponse,
                IsEmergency = true
            };
        }

        // --- Build Gemini API request ---
        try
        {
            var apiKey = _config["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured.");
            var model = _config["Gemini:Model"] ?? "gemini-2.0-flash";
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            // Build conversation contents for Gemini
            var contents = new List<object>();

            // Add conversation history (last 10 messages for context window)
            foreach (var msg in history.TakeLast(10))
            {
                contents.Add(new
                {
                    role = msg.Sender == ChatSender.User ? "user" : "model",
                    parts = new[] { new { text = msg.Message } }
                });
            }

            // Add current user message
            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = sanitized } }
            });

            var requestBody = new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = SystemPrompt } }
                },
                contents,
                generationConfig = new
                {
                    maxOutputTokens = int.TryParse(_config["Gemini:MaxTokens"], out var t) ? t : 1024,
                    temperature = 0.7,
                    topP = 0.9
                },
                safetySettings = new[]
                {
                    new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                    new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                    new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_MEDIUM_AND_ABOVE" }
                }
            };

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Handle rate limiting (429) specifically
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                // Try to extract retryDelay from response body
                var retrySeconds = 30;
                try
                {
                    using var errDoc = JsonDocument.Parse(responseBody);
                    var details = errDoc.RootElement
                        .GetProperty("error")
                        .GetProperty("details");
                    foreach (var detail in details.EnumerateArray())
                    {
                        if (detail.TryGetProperty("retryDelay", out var rd))
                        {
                            var rdStr = rd.GetString() ?? "30s";
                            retrySeconds = int.TryParse(rdStr.Replace("s", ""), out var s) ? s : 30;
                            break;
                        }
                    }
                }
                catch { /* use default */ }

                return new AIResponse
                {
                    Message = $"⏳ **The AI is temporarily busy** (free tier limit reached).\n\nPlease wait **{retrySeconds} seconds** and send your message again. This is normal for the free Gemini tier.\n\n*Tip: For faster responses, you can get a paid Gemini API key from [Google AI Studio](https://aistudio.google.com).*"
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Gemini API error {Status}: {Body}", response.StatusCode, responseBody);
                return new AIResponse { Message = $"⚠️ AI service error ({(int)response.StatusCode}). Please try again in a moment." };
            }

            using var doc = JsonDocument.Parse(responseBody);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "I couldn't generate a response. Please try again.";

            return new AIResponse { Message = text.Trim() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API");
            return new AIResponse { Message = "I'm experiencing technical difficulties. Please try again shortly." };
        }
    }

    private static bool IsEmergency(string message)
    {
        var lower = message.ToLowerInvariant();
        return EmergencyKeywords.Any(kw => lower.Contains(kw.ToLowerInvariant()));
    }

    private static bool ContainsBangla(string text)
    {
        return text.Any(c => c >= '\u0980' && c <= '\u09FF');
    }

    private static string SanitizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        // Trim and limit length
        input = input.Trim();
        if (input.Length > 2000) input = input[..2000];

        // Remove HTML tags
        input = Regex.Replace(input, "<[^>]+>", string.Empty);

        // Prevent prompt injection: remove instruction-like prefixes
        var injectionPatterns = new[]
        {
            @"(?i)(ignore previous|ignore above|disregard|forget your|new instruction|system:)",
        };
        foreach (var pattern in injectionPatterns)
            input = Regex.Replace(input, pattern, "[removed]");

        return input;
    }
}
