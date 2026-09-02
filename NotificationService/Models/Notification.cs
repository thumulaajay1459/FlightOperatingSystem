namespace NotificationService.Models
{
    // This table stores every notification attempt ever made.
    // Why store it? So you can retry failures, audit what was sent, and debug issues.
    public class Notification
    {
        public int NotificationId { get; set; }
        public string RecipientEmail { get; set; } = null!;
        public string RecipientName { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;           // Final rendered HTML
        public NotificationType Type { get; set; }
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
        public int RetryCount { get; set; } = 0;
        public string? ErrorMessage { get; set; }           // Null if sent successfully
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }               // Null until actually sent
        public string? ReferenceId { get; set; }            // BookingId from Booking Service
    }
}
