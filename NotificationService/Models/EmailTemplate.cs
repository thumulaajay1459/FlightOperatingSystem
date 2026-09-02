namespace NotificationService.Models
{
    // Why store templates in DB instead of files?
    // So non-developers (admins) can update email content without redeploying the app.
    // For now we seed them from code, but later an admin panel can edit them.
    public class EmailTemplate
    {
        public int EmailTemplateId { get; set; }
        public string TemplateName { get; set; } = null!;   // e.g. "TicketConfirmation"
        public string Subject { get; set; } = null!;
        public string HtmlBody { get; set; } = null!;       // Contains {{placeholders}}
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
