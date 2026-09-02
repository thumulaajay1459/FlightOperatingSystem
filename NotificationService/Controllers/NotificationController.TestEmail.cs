using Microsoft.AspNetCore.Mvc;
using NotificationService.Services;

namespace NotificationService.Controllers
{
    public partial class NotificationController
    {
        // GET api/notifications/test-email?email=your@email.com
        [HttpGet("test-email")]
        public async Task<IActionResult> TestEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
                return BadRequest("Email parameter is required");

            try
            {
                await _emailService.SendAsync(
                    email,
                    "Test User",
                    "SMTP Test - Flight Booking System",
                    "<h2>Test Email</h2><p>If you receive this, SMTP is working!</p>"
                );

                _logger.LogInformation("Test email sent to {Email}", email);
                return Ok(new { message = "Test email sent successfully!", email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test email to {Email}", email);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
