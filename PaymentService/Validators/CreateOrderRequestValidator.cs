using FluentValidation;
using PaymentService.DTOs;

namespace PaymentService.Validators
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.BookingId)
                .NotEmpty().WithMessage("Booking ID is required");

            RuleFor(x => x.FlightId)
                .NotEmpty().WithMessage("Flight ID is required");

            RuleFor(x => x.FlightNumber)
                .NotEmpty().WithMessage("Flight number is required");

            RuleFor(x => x.Origin)
                .NotEmpty().WithMessage("Origin is required")
                .Length(2, 100).WithMessage("Origin must be between 2 and 100 characters");

            RuleFor(x => x.Destination)
                .NotEmpty().WithMessage("Destination is required")
                .Length(2, 100).WithMessage("Destination must be between 2 and 100 characters");

            RuleFor(x => x.TravelDate)
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Travel date must be today or in the future");

            RuleFor(x => x.PassengerId)
                .NotEmpty().WithMessage("Passenger ID is required");

            RuleFor(x => x.PassengerName)
                .NotEmpty().WithMessage("Passenger name is required")
                .Length(2, 100).WithMessage("Passenger name must be between 2 and 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required")
                .MinimumLength(10).WithMessage("Phone number must be at least 10 characters");

            RuleFor(x => x.SeatNumber)
                .NotEmpty().WithMessage("Seat number is required");

            RuleFor(x => x.CabinClass)
                .NotEmpty().WithMessage("Cabin class is required")
                .Must(c => c == "Economy" || c == "PremiumEconomy" || c == "Business" || c == "FirstClass")
                .WithMessage("Cabin class must be Economy, PremiumEconomy, Business, or FirstClass");

            RuleFor(x => x.BaseFare)
                .GreaterThan(0).WithMessage("Base fare must be greater than 0");

            RuleFor(x => x.TaxAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Tax amount cannot be negative");

            RuleFor(x => x.ServiceFee)
                .GreaterThanOrEqualTo(0).WithMessage("Service fee cannot be negative");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .Length(3, 3).WithMessage("Currency must be 3 characters")
                .Matches("^[A-Z]+$").WithMessage("Currency must be uppercase letters");
        }
    }
}
