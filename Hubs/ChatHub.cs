using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MediCareMS.Data;
using MediCareMS.Helpers.AI;
using MediCareMS.Models.Entities.Chat;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace MediCareMS.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly AppDbContext    _db;
    private readonly AgentAIService  _agent;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(AppDbContext db, AgentAIService agent, ILogger<ChatHub> logger)
    {
        _db     = db;
        _agent  = agent;
        _logger = logger;
    }

    public async Task SendMessage(int sessionId, string message)
    {
        var userId = GetUserId();
        if (userId == 0) { await Clients.Caller.SendAsync("Error", "Unauthorized"); return; }

        // Validate session belongs to user
        var session = await _db.ChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session == null) { await Clients.Caller.SendAsync("Error", "Session not found"); return; }

        if (string.IsNullOrWhiteSpace(message) || message.Length > 2000)
        {
            await Clients.Caller.SendAsync("Error", "Invalid message");
            return;
        }

        // Save user message
        var userMsg = new ChatMessage
        {
            SessionId = sessionId,
            Sender    = ChatSender.User,
            Message   = message.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _db.ChatMessages.Add(userMsg);

        // Update session title from first message
        if (session.Title == "New Conversation")
            session.Title = message.Length > 50 ? message[..47] + "..." : message;

        session.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Echo user message back
        await Clients.Caller.SendAsync("MessageSaved", new
        {
            id           = userMsg.Id,
            sender       = "User",
            message      = userMsg.Message,
            createdAt    = userMsg.CreatedAt.ToString("HH:mm"),
            sessionTitle = session.Title
        });

        await Clients.Caller.SendAsync("AITyping", true);

        try
        {
            // Load recent history for context
            var history = await _db.ChatMessages
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            // ── Agent processes the message ──────────────────────────────────
            var agentResp = await _agent.ProcessAsync(message, history, userId);

            // Save AI reply
            var aiMsg = new ChatMessage
            {
                SessionId   = sessionId,
                Sender      = ChatSender.AI,
                Message     = agentResp.Message,
                IsEmergency = agentResp.IsEmergency,
                CreatedAt   = DateTime.UtcNow
            };
            _db.ChatMessages.Add(aiMsg);
            session.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await Clients.Caller.SendAsync("AITyping", false);

            // Send text reply
            await Clients.Caller.SendAsync("ReceiveAIMessage", new
            {
                id           = aiMsg.Id,
                sender       = "AI",
                message      = aiMsg.Message,
                isEmergency  = aiMsg.IsEmergency,
                isRateLimited= agentResp.IsRateLimited,
                createdAt    = aiMsg.CreatedAt.ToString("HH:mm")
            });

            // Send rich action card (if any)
            if (agentResp.ActionType != "none" && agentResp.ActionData != null)
            {
                await Clients.Caller.SendAsync("ReceiveAgentAction", new
                {
                    actionType = agentResp.ActionType,
                    actionData = agentResp.ActionData
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ChatHub for session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("AITyping", false);
            await Clients.Caller.SendAsync("Error", "Failed to get AI response. Please try again.");
        }
    }

    private int GetUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
