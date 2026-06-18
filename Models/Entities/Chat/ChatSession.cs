using MediCareMS.Models.Entities.Auth;

namespace MediCareMS.Models.Entities.Chat;

public class ChatSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = "New Conversation";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ApplicationUser User { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
