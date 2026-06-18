using MediCareMS.Models.Entities.Chat;

namespace MediCareMS.Helpers.AI;

public interface IAIService
{
    /// <summary>
    /// Sends a user message to the AI with conversation history and returns the AI response.
    /// </summary>
    Task<AIResponse> GetResponseAsync(string userMessage, List<ChatMessage> history, int userId);
}

public class AIResponse
{
    public string Message { get; set; } = string.Empty;
    public bool IsEmergency { get; set; } = false;
    public bool IsRateLimited { get; set; } = false;
}
