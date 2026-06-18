namespace MediCareMS.Models.Entities.Chat;

public enum ChatSender
{
    User,
    AI
}

public class ChatMessage
{
    public int Id { get; set; }
    public int SessionId { get; set; }
    public ChatSender Sender { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsEmergency { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ChatSession Session { get; set; } = null!;
}
