using FluentValidation;
using PaymentService.DTOs;

namespace PaymentService.Validators;

public class FailPaymentRequestValidator : AbstractValidator<FailPaymentRequest>
{
    public FailPaymentRequestValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
