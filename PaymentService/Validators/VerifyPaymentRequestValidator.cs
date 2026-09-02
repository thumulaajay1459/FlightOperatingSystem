using FluentValidation;
using PaymentService.DTOs;

namespace PaymentService.Validators
{
    public class VerifyPaymentRequestValidator : AbstractValidator<VerifyPaymentRequest>
    {
        public VerifyPaymentRequestValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty().WithMessage("Payment ID is required");

            RuleFor(x => x.RazorpayOrderId)
                .NotEmpty().WithMessage("Razorpay order ID is required");

            RuleFor(x => x.RazorpayPaymentId)
                .NotEmpty().WithMessage("Razorpay payment ID is required");

            RuleFor(x => x.RazorpaySignature)
                .NotEmpty().WithMessage("Razorpay signature is required");
        }
    }
}
