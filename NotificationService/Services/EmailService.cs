using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace NotificationService.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            var smtpHost = _config["Smtp:Host"]!;
            var smtpPort = int.Parse(_config["Smtp:Port"]!);
            var smtpUser = _config["Smtp:Username"]!;
            var smtpPass = _config["Smtp:Password"]!;
            var fromEmail = _config["Smtp:FromEmail"]!;
            var fromName = _config["Smtp:FromName"]!;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Email} | Subject: {Subject}", toEmail, subject);
        }

        public async Task SendWithPdfAttachmentAsync(string toEmail, string toName, string subject, string htmlBody, byte[] pdfBytes, string fileName)
        {
            var smtpHost = _config["Smtp:Host"]!;
            var smtpPort = int.Parse(_config["Smtp:Port"]!);
            var smtpUser = _config["Smtp:Username"]!;
            var smtpPass = _config["Smtp:Password"]!;
            var fromEmail = _config["Smtp:FromEmail"]!;
            var fromName = _config["Smtp:FromName"]!;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            builder.Attachments.Add(fileName, pdfBytes, ContentType.Parse("application/pdf"));
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email with PDF sent to {Email} | Subject: {Subject}", toEmail, subject);
        }
    }
}
