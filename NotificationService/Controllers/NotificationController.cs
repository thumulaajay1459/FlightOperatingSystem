using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Models;
using NotificationService.Services;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public partial class NotificationController : ControllerBase
    {
        private readonly NotificationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            NotificationDbContext context,
            IEmailService emailService,
            ILogger<NotificationController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // POST api/notifications/send-ticket-with-pdf
        [HttpPost("send-ticket-with-pdf")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> SendTicketWithPdf([FromForm] string recipientEmail, [FromForm] string recipientName, 
            [FromForm] string bookingId, [FromForm] string flightNumber, [FromForm] IFormFile pdfFile)
        {
            if (pdfFile == null || pdfFile.Length == 0)
                return BadRequest("PDF file is required");

            // Simple email body
            var subject = $"Flight Ticket - {flightNumber}";
            var body = $@"
                <h2>Your Flight Ticket</h2>
                <p>Dear {recipientName},</p>
                <p>Thank you for booking with us!</p>
                <p><strong>Booking Reference:</strong> {bookingId}</p>
                <p><strong>Flight Number:</strong> {flightNumber}</p>
                <p>Please find your ticket attached as PDF.</p>
                <br/>
                <p>Have a safe journey!</p>
                <p>Flight Operations Team</p>
            ";

            // Read PDF bytes
            byte[] pdfBytes;
            using (var ms = new MemoryStream())
            {
                await pdfFile.CopyToAsync(ms);
                pdfBytes = ms.ToArray();
            }

            // Save notification
            var notification = new Notification
            {
                RecipientEmail = recipientEmail,
                RecipientName = recipientName,
                Subject = subject,
                Body = body,
                Type = NotificationType.TicketConfirmation,
                Status = NotificationStatus.Pending,
                ReferenceId = bookingId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send email with PDF
            try
            {
                await _emailService.SendWithPdfAttachmentAsync(
                    recipientEmail,
                    recipientName,
                    subject,
                    body,
                    pdfBytes,
                    $"FlightTicket_{bookingId}.pdf");

                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTime.UtcNow;
                _logger.LogInformation("Email with PDF sent to {Email}", recipientEmail);
            }
            catch (Exception ex)
            {
                notification.Status = NotificationStatus.Failed;
                notification.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Failed to send email with PDF to {Email}", recipientEmail);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                notificationId = notification.NotificationId,
                status = notification.Status.ToString(),
                message = notification.Status == NotificationStatus.Sent
                    ? "Email with PDF sent successfully"
                    : "Email failed but saved for retry"
            });
        }

        // GET api/notifications/history
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var totalCount = await _context.Notifications.CountAsync();
            var notifications = await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new
                {
                    n.NotificationId,
                    n.RecipientEmail,
                    n.RecipientName,
                    n.Subject,
                    Type = n.Type.ToString(),
                    Status = n.Status.ToString(),
                    n.CreatedAt,
                    n.SentAt,
                    n.ReferenceId
                })
                .ToListAsync();

            return Ok(new
            {
                data = notifications,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }

        // POST api/notifications/retry/{id}
        [HttpPost("retry/{id}")]
        public async Task<IActionResult> Retry(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            if (notification.Status == NotificationStatus.Sent)
                return BadRequest("Notification already sent");

            return BadRequest("Retry not supported for PDF emails. Please resend from BookingService.");
        }

        // POST api/notifications/test-smtp
        [HttpPost("test-smtp")]
        public async Task<IActionResult> TestSmtp([FromBody] TestEmailRequest request)
        {
            try
            {
                await _emailService.SendAsync(
                    request.ToEmail,
                    "Test User",
                    "SMTP Test Email",
                    "<h2>✅ SMTP Configuration Test</h2><p>If you receive this email, your SMTP settings are working correctly!</p>");

                return Ok(new { success = true, message = "Test email sent successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP test failed");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        // GET api/notifications/user/{userId} - Get user notifications
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = _context.Notifications
                .Where(n => n.ReferenceId != null && n.ReferenceId.StartsWith(userId))
                .OrderByDescending(n => n.CreatedAt);

            var totalCount = await query.CountAsync();
            var notifications = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new
                {
                    n.NotificationId,
                    n.RecipientEmail,
                    n.RecipientName,
                    n.Subject,
                    Type = n.Type.ToString(),
                    Status = n.Status.ToString(),
                    n.CreatedAt,
                    n.SentAt,
                    n.ReferenceId
                })
                .ToListAsync();

            return Ok(new
            {
                data = notifications,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            });
        }
        
        // POST api/notifications/flight-status-change
        [HttpPost("flight-status-change")]
        public async Task<IActionResult> SendFlightStatusNotification([FromBody] FlightStatusChangeRequest request)
        {
            try
            {
                var bookingClient = new HttpClient { BaseAddress = new Uri("http://localhost:5004") };
                var bookingsResponse = await bookingClient.GetAsync($"/api/booking/by-flight/{request.FlightId}");
                
                if (!bookingsResponse.IsSuccessStatusCode)
                {
                    return Ok(new { message = "No bookings found" });
                }
                
                var bookings = await bookingsResponse.Content.ReadFromJsonAsync<List<BookingInfo>>();
                if (bookings == null || bookings.Count == 0) return Ok(new { message = "No bookings" });
                
                var flightClient = new HttpClient { BaseAddress = new Uri("http://localhost:5003") };
                var flightResponse = await flightClient.GetAsync($"/api/ScheduledFlights/{request.FlightId}");
                var flight = await flightResponse.Content.ReadFromJsonAsync<FlightInfo>();
                
                var userClient = new HttpClient { BaseAddress = new Uri("http://localhost:5001") };
                int emailsSent = 0;
                
                foreach (var booking in bookings)
                {
                    try
                    {
                        var userResponse = await userClient.GetAsync($"/api/User/{booking.UserId}");
                        if (!userResponse.IsSuccessStatusCode) continue;
                        
                        var user = await userResponse.Content.ReadFromJsonAsync<UserInfo>();
                        if (user == null || string.IsNullOrEmpty(user.Email)) continue;
                        
                        var subject = request.Status == "Cancelled" 
                            ? $"Flight Cancelled - {flight?.FlightNumber}" 
                            : $"Flight Delayed - {flight?.FlightNumber}";
                        
                        var delayInfo = request.DelayedMinutes.HasValue 
                            ? $"<p><strong>Delay Duration:</strong> {request.DelayedMinutes.Value} minutes</p>" 
                            : "";
                        
                        var body = $@"
                            <h2>Flight Status Update</h2>
                            <p>Dear {user.FirstName} {user.LastName},</p>
                            <p>We regret to inform you that your flight has been {request.Status.ToLower()}.</p>
                            <hr/>
                            <p><strong>Booking Reference:</strong> {booking.BookingReference}</p>
                            <p><strong>Flight Number:</strong> {flight?.FlightNumber}</p>
                            <p><strong>Status:</strong> {request.Status}</p>
                            {delayInfo}
                            {(!string.IsNullOrEmpty(request.Comments) ? $"<p><strong>Comments:</strong> {request.Comments}</p>" : "")}
                            <hr/>
                            <p>We apologize for any inconvenience.</p>
                            <p>Flight Operations Team</p>
                        ";
                        
                        await _emailService.SendAsync(user.Email, $"{user.FirstName} {user.LastName}", subject, body);
                        
                        var notification = new Notification
                        {
                            RecipientEmail = user.Email,
                            RecipientName = $"{user.FirstName} {user.LastName}",
                            Subject = subject,
                            Body = body,
                            Type = NotificationType.FlightUpdate,
                            Status = NotificationStatus.Sent,
                            ReferenceId = booking.BookingReference,
                            CreatedAt = DateTime.UtcNow,
                            SentAt = DateTime.UtcNow
                        };
                        
                        _context.Notifications.Add(notification);
                        emailsSent++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed for booking {booking.BookingReference}");
                    }
                }
                
                await _context.SaveChangesAsync();
                return Ok(new { message = $"Sent {emailsSent} notifications" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notifications");
                return StatusCode(500, new { error = "Failed" });
            }
        }
    }

    public class TestEmailRequest
    {
        public string ToEmail { get; set; } = string.Empty;
    }
    
    public class FlightStatusChangeRequest
    {
        public int FlightId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public int? DelayedMinutes { get; set; }
    }
    
    public class BookingInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string BookingReference { get; set; } = string.Empty;
    }
    
    public class UserInfo
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
    
    public class FlightInfo
    {
        public string FlightNumber { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
    }
}
