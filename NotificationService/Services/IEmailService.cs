namespace NotificationService.Services
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string toName, string subject, string htmlBody);
        Task SendWithPdfAttachmentAsync(string toEmail, string toName, string subject, string htmlBody, byte[] pdfBytes, string fileName);
    }
}
