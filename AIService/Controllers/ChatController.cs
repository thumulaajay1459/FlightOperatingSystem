using System.Security.Claims;
using AIService.Data;
using AIService.DTOs;
using AIService.Models;
using AIService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIService.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly AiDbContext _db;
        private readonly ChatService _chatService;

        public ChatController(AiDbContext db, ChatService chatService)
        {
            _db = db;
            _chatService = chatService;
        }

        // POST api/chat
        // Send a message — creates new session if SessionId is null
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            var userId = GetUserId();

            // For unauthenticated users, don't save to database
            if (userId == "test-user-1" || string.IsNullOrEmpty(userId))
            {
                var reply = await _chatService.ChatAsync(0, request.Message, userId);
                return Ok(new ChatResponse
                {
                    SessionId = 0,
                    Reply = reply,
                    Timestamp = DateTime.UtcNow
                });
            }

            // Get or create session for authenticated users
            ChatSession session;
            if (request.SessionId.HasValue)
            {
                session = await _db.ChatSessions
                    .FirstOrDefaultAsync(s => s.SessionId == request.SessionId.Value
                                           && s.UserId == userId
                                           && s.IsActive);

                if (session == null)
                    return NotFound(new { error = "Session not found." });
            }
            else
            {
                session = new ChatSession { UserId = userId };
                _db.ChatSessions.Add(session);
                await _db.SaveChangesAsync();
            }

            // Save user message
            _db.ChatMessages.Add(new ChatMessage
            {
                SessionId = session.SessionId,
                Role      = MessageRole.User,
                Content   = request.Message
            });
            await _db.SaveChangesAsync();

            // Get AI reply
            var aiReply = await _chatService.ChatAsync(session.SessionId, request.Message, userId);

            // Save assistant reply
            _db.ChatMessages.Add(new ChatMessage
            {
                SessionId = session.SessionId,
                Role      = MessageRole.Assistant,
                Content   = aiReply
            });

            session.LastMessageAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new ChatResponse
            {
                SessionId = session.SessionId,
                Reply     = aiReply,
                Timestamp = DateTime.UtcNow
            });
        }

        // GET api/chat/sessions
        // Get all sessions for the current user
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var userId = GetUserId();

            var query = _db.ChatSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.LastMessageAt);

            var totalCount = await query.CountAsync();
            var sessions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ChatSessionDto
                {
                    SessionId      = s.SessionId,
                    CreatedAt      = s.CreatedAt,
                    LastMessageAt  = s.LastMessageAt,
                    MessageCount   = s.Messages.Count
                })
                .ToListAsync();

            return Ok(new
            {
                data = sessions,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }

        // GET api/chat/sessions/{sessionId}/history
        // Get full message history for a session
        [HttpGet("sessions/{sessionId}/history")]
        public async Task<IActionResult> GetHistory(int sessionId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            var userId = GetUserId();

            var session = await _db.ChatSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

            if (session == null) return NotFound(new { error = "Session not found." });

            var query = _db.ChatMessages
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.CreatedAt);

            var totalCount = await query.CountAsync();
            var messages = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new ChatMessageDto
                {
                    Role      = m.Role.ToString(),
                    Content   = m.Content,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                sessionId = session.SessionId,
                data = messages,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }

        // DELETE api/chat/sessions/{sessionId}
        // End/close a session
        [HttpDelete("sessions/{sessionId}")]
        public async Task<IActionResult> EndSession(int sessionId)
        {
            var userId = GetUserId();

            var session = await _db.ChatSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.UserId == userId);

            if (session == null) return NotFound(new { error = "Session not found." });

            session.IsActive = false;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Session ended." });
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? "test-user-1";  // TODO: remove before production
    }
}
