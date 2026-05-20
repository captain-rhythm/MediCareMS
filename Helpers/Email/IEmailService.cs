namespace MediCareMS.Helpers.Email;

public interface IEmailService
{
    Task SendInvitationAsync(string toEmail, string inviteLink);
    Task SendPasswordResetAsync(string toEmail, string resetLink);
}
