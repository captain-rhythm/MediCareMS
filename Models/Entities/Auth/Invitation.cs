namespace MediCareMS.Models.Entities.Auth;

public class Invitation
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(3);
    public bool IsUsed { get; set; } = false;

    // Pending | Registered | Accepted | Declined
    public string Status { get; set; } = "Pending";

    public string? RegisteredFullName { get; set; }
    public string? RegisteredPhone { get; set; }
    public string? RequestedRole { get; set; }

    public int InvitedByUserId { get; set; }
    public ApplicationUser? InvitedByUser { get; set; }
}
