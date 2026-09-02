using FluentValidation;
using PaymentService.DTOs;

namespace PaymentService.Validators
{
    public class RefundRequestValidator : AbstractValidator<RefundRequest>
    {
        public RefundRequestValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty().WithMessage("Payment ID is required");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Refund reason is required")
                .Length(10, 500).WithMessage("Reason must be between 10 and 500 characters");
        }
    }
}
