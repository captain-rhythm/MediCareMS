using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.ComponentModel.DataAnnotations;

namespace MediCareMS.Helpers.Email;

public class EmailService : IEmailService
{
    private readonly EmailOptions _opts;
    private readonly ILogger<EmailService> _logger;
    private const int MaxRetries = 3;
    private const int RetryDelayMs = 2000;

    public EmailService(IOptions<EmailOptions> opts, ILogger<EmailService> logger)
    {
        _opts = opts.Value;
        _logger = logger;
    }

    /// <summary>
    /// Validates email configuration before attempting to send.
    /// </summary>
    private void ValidateConfiguration()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(_opts.Host))
            errors.Add("Email Host is not configured.");

        if (_opts.Port <= 0 || _opts.Port > 65535)
            errors.Add($"Email Port ({_opts.Port}) is invalid. Must be between 1 and 65535.");

        if (string.IsNullOrWhiteSpace(_opts.UserName))
            errors.Add("Email UserName is not configured.");

        if (string.IsNullOrWhiteSpace(_opts.Password))
            errors.Add("Email Password is not configured.");

        if (string.IsNullOrWhiteSpace(_opts.From) || !IsValidEmail(_opts.From))
            errors.Add("Email From address is not configured or invalid.");

        if (errors.Any())
        {
            var errorMessage = string.Join(" ", errors);
            _logger.LogError("Email configuration validation failed: {Errors}", errorMessage);
            throw new InvalidOperationException($"Email service is not properly configured. {errorMessage}");
        }
    }

    /// <summary>
    /// Validates email address format.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates recipient email and link before sending.
    /// </summary>
    private void ValidateEmailInput(string toEmail, string inviteLink)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
            throw new ArgumentException("Recipient email address cannot be empty.", nameof(toEmail));

        if (!IsValidEmail(toEmail))
            throw new ArgumentException($"Recipient email address '{toEmail}' is invalid.", nameof(toEmail));

        if (string.IsNullOrWhiteSpace(inviteLink))
            throw new ArgumentException("Invite link cannot be empty.", nameof(inviteLink));

        if (!Uri.IsWellFormedUriString(inviteLink, UriKind.Absolute))
            throw new ArgumentException($"Invite link '{inviteLink}' is not a valid URI.", nameof(inviteLink));
    }

    public async Task SendInvitationAsync(string toEmail, string inviteLink)
    {
        _logger.LogInformation("Starting invitation email send to {Email}", toEmail);

        // Validate configuration
        ValidateConfiguration();

        // Validate input
        ValidateEmailInput(toEmail, inviteLink);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("MediCare HMS", _opts.From));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "You're Invited to Join MediCare HMS";

        var builder = new BodyBuilder
        {
            HtmlBody = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'/>
<style>
  body{{font-family:'Segoe UI',sans-serif;background:#f0f4f8;margin:0;padding:0}}
  .wrap{{max-width:560px;margin:40px auto;background:#fff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,.10)}}
  .hdr{{background:linear-gradient(135deg,#1a73e8,#0dcaf0);padding:36px;text-align:center}}
  .hdr h1{{color:#fff;margin:0;font-size:26px;letter-spacing:1px}}
  .hdr p{{color:rgba(255,255,255,.85);margin:6px 0 0;font-size:14px}}
  .body{{padding:36px;color:#333}}
  .body p{{line-height:1.7;font-size:15px;margin:0 0 16px}}
  .btn{{display:inline-block;margin:8px 0 24px;padding:14px 40px;background:#1a73e8;color:#fff!important;text-decoration:none;border-radius:8px;font-size:15px;font-weight:600;letter-spacing:.5px}}
  .note{{background:#fff8e1;border-left:4px solid #ffc107;padding:12px 16px;border-radius:4px;font-size:13px;color:#7a5c00;margin-top:4px}}
  .footer{{background:#f8f9fa;padding:16px 36px;font-size:12px;color:#999;text-align:center}}
</style>
</head>
<body>
<div class='wrap'>
  <div class='hdr'>
    <h1>?? MediCare HMS</h1>
    <p>Hospital Management System</p>
  </div>
  <div class='body'>
    <p>Hello,</p>
    <p>You have been personally invited to join <strong>MediCare HMS</strong>. Click the button below to complete your registration and set up your account.</p>
    <a href='{inviteLink}' class='btn'>Accept Invitation &amp; Register</a>
    <div class='note'>? This link expires in <strong>3 days</strong>. After registration, your account will be reviewed by an admin before activation.</div>
    <p style='margin-top:20px;font-size:13px;color:#666'>If you were not expecting this invitation, you can safely ignore this email.</p>
  </div>
  <div class='footer'>MediCare HMS &mdash; Do not reply to this email.</div>
</div>
</body>
</html>"
        };

        message.Body = builder.ToMessageBody();

        // Attempt to send with retry logic
        await SendWithRetryAsync(message, toEmail);

        _logger.LogInformation("Invitation email successfully sent to {Email}", toEmail);
    }

    /// <summary>
    /// Sends email with retry logic and detailed error logging.
    /// </summary>
    private async Task SendWithRetryAsync(MimeMessage message, string toEmail)
    {
        Exception? lastException = null;

        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                _logger.LogDebug("Attempting to send email (attempt {Attempt}/{MaxRetries})", attempt, MaxRetries);

                using var client = new SmtpClient();

                try
                {
                    await client.ConnectAsync(_opts.Host, _opts.Port, SecureSocketOptions.StartTls);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to connect to SMTP server {Host}:{Port}", _opts.Host, _opts.Port);
                    throw;
                }

                try
                {
                    await client.AuthenticateAsync(_opts.UserName, _opts.Password);
                }
                catch (MailKit.Security.AuthenticationException ex)
                {
                    _logger.LogError(ex, "SMTP authentication failed. Check username and password configuration.");
                    throw new InvalidOperationException("Email authentication failed. Verify your Gmail credentials and ensure you're using an App Password (not your regular Gmail password).", ex);
                }

                try
                {
                    await client.SendAsync(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email message");
                    throw;
                }

                try
                {
                    await client.DisconnectAsync(true);
                }
                catch
                {
                    // Disconnection errors shouldn't fail the operation
                    _logger.LogDebug("Non-critical error during SMTP disconnection");
                }

                // Success - return
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Email send attempt {Attempt} failed for {Email}", attempt, toEmail);

                // If this isn't the last attempt, wait before retrying
                if (attempt < MaxRetries)
                {
                    await Task.Delay(RetryDelayMs * attempt); // Exponential backoff
                }
            }
        }

        // All retries failed
        _logger.LogError(lastException, "Email send failed after {MaxRetries} attempts to {Email}", MaxRetries, toEmail);
        throw new InvalidOperationException(
            $"Failed to send invitation email to {toEmail} after {MaxRetries} attempts. " +
            $"Error: {lastException?.Message ?? "Unknown error"}",
            lastException);
    }
}
