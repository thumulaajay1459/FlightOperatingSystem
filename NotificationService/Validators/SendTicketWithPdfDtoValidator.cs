using FluentValidation;
using NotificationService.DTOs;

namespace NotificationService.Validators
{
    public class SendTicketWithPdfDtoValidator : AbstractValidator<SendTicketWithPdfDto>
    {
        public SendTicketWithPdfDtoValidator()
        {
            RuleFor(x => x.RecipientEmail)
                .NotEmpty().WithMessage("Recipient email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

            RuleFor(x => x.RecipientName)
                .NotEmpty().WithMessage("Recipient name is required")
                .Length(2, 100).WithMessage("Recipient name must be between 2 and 100 characters");

            RuleFor(x => x.BookingId)
                .NotEmpty().WithMessage("Booking ID is required");

            RuleFor(x => x.FlightNumber)
                .NotEmpty().WithMessage("Flight number is required")
                .Matches("^[A-Z0-9]+$").WithMessage("Flight number must contain only uppercase letters and numbers");

            RuleFor(x => x.PdfFile)
                .NotNull().WithMessage("PDF file is required")
                .Must(file => file.Length > 0).WithMessage("PDF file cannot be empty")
                .Must(file => file.ContentType == "application/pdf").WithMessage("File must be a PDF");
        }
    }
}
