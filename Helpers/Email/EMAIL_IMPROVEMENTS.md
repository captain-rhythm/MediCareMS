# EmailService Improvements

## Overview
The `EmailService` has been enhanced with comprehensive validation and error handling to provide better debugging and user feedback.

## Key Features Added

### 1. **Configuration Validation**
- Validates that all required email settings are configured (Host, Port, UserName, Password, From)
- Port range validation (1-65535)
- Email format validation using `System.Net.Mail.MailAddress`
- Throws `InvalidOperationException` with detailed error messages if configuration is invalid

### 2. **Input Validation**
- Validates recipient email address format
- Validates invite link is a well-formed absolute URI
- Prevents null/empty values
- Throws `ArgumentException` with specific field errors

### 3. **Retry Logic with Exponential Backoff**
- Attempts to send email up to 3 times on failure
- Uses exponential backoff (2s, 4s, 6s delays)
- Handles transient network errors gracefully
- Logs each retry attempt

### 4. **Detailed Error Logging**
- Logs configuration validation failures
- Logs each send attempt (success/failure)
- Captures specific SMTP connection errors
- Identifies authentication issues with helpful guidance
- Uses structured logging with email addresses and attempt counts

### 5. **Specific Error Messages**
- **Authentication Failure**: Provides hint about using Gmail App Password instead of regular password
- **Connection Failure**: Shows which SMTP server failed to connect
- **Validation Failure**: Specifies which configuration field is missing/invalid
- **Recipient Error**: Tells user which email address is invalid

### 6. **Improved Exception Information**
- Preserves inner exceptions with full stack traces
- Includes attempt count in final error
- Distinguishes between configuration errors and runtime errors

## Configuration Requirements

### In `appsettings.json` or `appsettings.Development.json`:
```json
"Email": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "EnableSsl": true,
  "UserName": "your-email@gmail.com",
  "Password": "your-16-char-app-password",
  "From": "your-email@gmail.com",
  "BaseUrl": "http://localhost:5002/"
}
```

### Gmail Setup Instructions
1. Enable 2-Step Verification on your Google Account
2. Generate an App Password (16 characters) for Mail/Windows
3. Use the app password in `Password` field (not your regular Gmail password)
4. Use your full Gmail address for both `UserName` and `From`

## Error Handling Flow

```
SendInvitationAsync()
  ?
ValidateConfiguration()
  ?? Check Host, Port, UserName, Password, From
  ?? Throw InvalidOperationException if any missing/invalid
  ?
ValidateEmailInput()
  ?? Check toEmail format
  ?? Check inviteLink format
  ?? Throw ArgumentException if invalid
  ?
SendWithRetryAsync()
  ?? Attempt 1: Send email (2s delay if fails)
  ?? Attempt 2: Retry (4s delay if fails)
  ?? Attempt 3: Retry (6s delay if fails)
  ?? Success: Log and return
  ?? All failed: Throw InvalidOperationException with details
```

## Usage in Controller

```csharp
try
{
	await _email.SendInvitationAsync(email, link);
	TempData["Success"] = $"Invitation sent to {email}!";
}
catch (InvalidOperationException ex)
{
	// Configuration or SMTP service issue
	TempData["Error"] = ex.Message;
}
catch (ArgumentException ex)
{
	// Invalid input (email format, etc.)
	TempData["Error"] = $"Invalid email: {ex.Message}";
}
catch (Exception ex)
{
	// Unexpected error
	TempData["Error"] = $"Failed to send invitation: {ex.Message}";
}
```

## Logging Output Examples

### Configuration validation failure:
```
Error: Email configuration validation failed: Email Password is not configured. Email UserName is not configured.
```

### Authentication failure:
```
Error: SMTP authentication failed. Check username and password configuration.
Hint: Email authentication failed. Verify your Gmail credentials and ensure you're using an App Password (not your regular Gmail password).
```

### Retry success:
```
Debug: Attempting to send email (attempt 1/3)
Debug: Attempting to send email (attempt 2/3)
Warning: Email send attempt 2 failed for user@example.com
Information: Invitation email successfully sent to user@example.com
```

## Dependencies
- `Microsoft.Extensions.Logging` (for ILogger)
- `MailKit` (for SMTP operations)
- `System.ComponentModel.DataAnnotations` (already referenced)

No new NuGet packages required - uses existing dependencies.
