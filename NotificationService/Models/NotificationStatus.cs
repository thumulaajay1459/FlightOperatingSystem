namespace NotificationService.Models
{
    // Enum stored as int in DB — Pending=0, Sent=1, Failed=2
    // Using an enum means you can never accidentally store an invalid status string
    public enum NotificationStatus
    {
        Pending = 0,
        Sent = 1,
        Failed = 2
    }

    public enum NotificationType
    {
        TicketConfirmation = 0,
        BoardingPass = 1,
        FlightUpdate = 2
    }
}
