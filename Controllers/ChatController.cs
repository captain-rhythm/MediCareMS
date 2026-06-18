using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCareMS.Data;
using MediCareMS.Models.Entities.Chat;
using System.Security.Claims;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MediCareMS.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<ChatController> _logger;

    public ChatController(AppDbContext db, ILogger<ChatController> logger)
    {
        _db = db;
        _logger = logger;
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : 0;
    }

    // GET: /Chat/Index (dedicated page, optional)
    public IActionResult Index() => View();

    // POST: /Chat/NewSession
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NewSession()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var session = new ChatSession
        {
            UserId = userId,
            Title = "New Conversation",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.ChatSessions.Add(session);
        await _db.SaveChangesAsync();

        return Json(new { success = true, sessionId = session.Id, title = session.Title });
    }

    // GET: /Chat/Sessions
    [HttpGet]
    public async Task<IActionResult> Sessions()
    {
        var userId = GetUserId();
        var sessions = await _db.ChatSessions
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.UpdatedAt)
            .Take(20)
            .Select(s => new { s.Id, s.Title, updatedAt = s.UpdatedAt.ToString("MMM dd, HH:mm") })
            .ToListAsync();

        return Json(sessions);
    }

    // GET: /Chat/History/{sessionId}
    [HttpGet]
    public async Task<IActionResult> History(int sessionId)
    {
        var userId = GetUserId();
        var session = await _db.ChatSessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
        if (session == null) return NotFound();

        var messages = await _db.ChatMessages
            .Where(m => m.SessionId == sessionId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new
            {
                m.Id,
                sender = m.Sender.ToString(),
                m.Message,
                m.IsEmergency,
                createdAt = m.CreatedAt.ToString("HH:mm")
            })
            .ToListAsync();

        return Json(new { sessionTitle = session.Title, messages });
    }

    // DELETE: /Chat/DeleteSession/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSession(int id)
    {
        var userId = GetUserId();
        var session = await _db.ChatSessions.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        if (session == null) return NotFound();

        session.IsActive = false; // Soft delete
        await _db.SaveChangesAsync();
        return Json(new { success = true });
    }

    // POST: /Chat/Rate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate([FromBody] RateRequest req)
    {
        // Store rating as a special AI message marker
        var userId = GetUserId();
        var session = await _db.ChatSessions.FirstOrDefaultAsync(s => s.Id == req.SessionId && s.UserId == userId);
        if (session == null) return NotFound();

        // Save as system message
        _db.ChatMessages.Add(new ChatMessage
        {
            SessionId = req.SessionId,
            Sender = ChatSender.AI,
            Message = $"[RATING:{req.Stars}]",
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return Json(new { success = true });
    }

    // GET: /Chat/ExportPdf/{sessionId}
    [HttpGet]
    public async Task<IActionResult> ExportPdf(int sessionId)
    {
        var userId = GetUserId();
        var session = await _db.ChatSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

        if (session == null) return NotFound();

        QuestPDF.Settings.License = LicenseType.Community;

        var messages = session.Messages
            .Where(m => !m.Message.StartsWith("[RATING:"))
            .OrderBy(m => m.CreatedAt)
            .ToList();

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("🏥 MediCare AI Consultation")
                            .FontSize(18).Bold().FontColor(Color.FromHex("#6366f1"));
                        row.ConstantItem(150).AlignRight()
                            .Text($"Exported: {DateTime.Now:dd MMM yyyy}")
                            .FontSize(9).FontColor(Color.FromHex("#6b7280"));
                    });
                    col.Item().Text($"Session: {session.Title}").FontSize(11).Italic().FontColor(Color.FromHex("#4b5563"));
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Color.FromHex("#e5e7eb"));
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Item().Background(Color.FromHex("#fef2f2")).Padding(6)
                        .Text("⚠️ Disclaimer: This conversation is for educational purposes only and is NOT a medical diagnosis.")
                        .FontSize(9).Italic().FontColor(Color.FromHex("#dc2626"));

                    col.Item().PaddingTop(10);

                    foreach (var msg in messages)
                    {
                        col.Item().PaddingBottom(8).Border(1).BorderColor(
                            msg.Sender == ChatSender.User ? Color.FromHex("#e0e7ff") : Color.FromHex("#dcfce7"))
                            .Background(msg.Sender == ChatSender.User ? Color.FromHex("#f0f4ff") : Color.FromHex("#f0fdf4"))
                            .Padding(8).Column(inner =>
                            {
                                inner.Item().Row(r =>
                                {
                                    r.RelativeItem().Text(msg.Sender == ChatSender.User ? "👤 You" : "🤖 MediCare AI")
                                        .Bold().FontSize(10)
                                        .FontColor(msg.Sender == ChatSender.User ? Color.FromHex("#4338ca") : Color.FromHex("#16a34a"));
                                    r.ConstantItem(80).AlignRight()
                                        .Text(msg.CreatedAt.ToString("HH:mm")).FontSize(9).FontColor(Color.FromHex("#9ca3af"));
                                });
                                inner.Item().PaddingTop(4).Text(msg.Message).FontSize(10);
                            });
                    }
                });

                page.Footer().AlignCenter()
                    .Text(t =>
                    {
                        t.Span("MediCare Management System — AI Assistant | Page ").FontSize(8).FontColor(Color.FromHex("#9ca3af"));
                        t.CurrentPageNumber().FontSize(8).FontColor(Color.FromHex("#9ca3af"));
                    });
            });
        });

        var bytes = pdf.GeneratePdf();
        var filename = $"MediCare_Chat_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
        return File(bytes, "application/pdf", filename);
    }

    public class RateRequest
    {
        public int SessionId { get; set; }
        public int Stars { get; set; }
    }
}
