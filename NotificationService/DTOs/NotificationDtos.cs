using System.ComponentModel.DataAnnotations;

namespace NotificationService.DTOs
{
    // Simple DTO for sending ticket with PDF
    public class SendTicketWithPdfDto
    {
        [Required] public string RecipientEmail { get; set; } = null!;
        [Required] public string RecipientName { get; set; } = null!;
        [Required] public string BookingId { get; set; } = null!;
        [Required] public string FlightNumber { get; set; } = null!;
        [Required] public IFormFile PdfFile { get; set; } = null!;
    }
}
