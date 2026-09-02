using FluentValidation;
using FlightService.DTOs;

namespace FlightService.Validators
{
    public class CreateFlightDtoValidator : AbstractValidator<CreateFlightDto>
    {
        public CreateFlightDtoValidator()
        {
            RuleFor(x => x.FlightNumber)
                .NotEmpty().WithMessage("Flight number is required")
                .Length(3, 10).WithMessage("Flight number must be between 3 and 10 characters")
                .Matches("^[A-Z0-9]+$").WithMessage("Flight number must contain only uppercase letters and numbers");

            RuleFor(x => x.AircraftId)
                .GreaterThan(0).WithMessage("Aircraft ID must be greater than 0");

            RuleFor(x => x.RouteId)
                .GreaterThan(0).WithMessage("Route ID must be greater than 0");

            RuleFor(x => x.DepartureTime)
                .GreaterThan(DateTime.Now).WithMessage("Departure time must be in the future");

            RuleFor(x => x.ArrivalTime)
                .GreaterThan(x => x.DepartureTime).WithMessage("Arrival time must be after departure time");

            RuleFor(x => x.Status)
                .Must(s => s == "Scheduled" || s == "Delayed" || s == "Cancelled" || s == "Completed")
                .WithMessage("Status must be Scheduled, Delayed, Cancelled, or Completed");

            RuleFor(x => x.BasePrice)
                .GreaterThan(0).WithMessage("Base price must be greater than 0");

            RuleFor(x => x.EconomyPrice)
                .GreaterThan(0).WithMessage("Economy price must be greater than 0");

            RuleFor(x => x.PremiumEconomyPrice)
                .GreaterThan(0).WithMessage("Premium economy price must be greater than 0");

            RuleFor(x => x.BusinessPrice)
                .GreaterThan(0).WithMessage("Business price must be greater than 0");

            RuleFor(x => x.FirstClassPrice)
                .GreaterThan(0).WithMessage("First class price must be greater than 0");

            RuleFor(x => x.ChildDiscountPercent)
                .InclusiveBetween(0, 100).WithMessage("Child discount must be between 0 and 100");

            RuleFor(x => x.InfantDiscountPercent)
                .InclusiveBetween(0, 100).WithMessage("Infant discount must be between 0 and 100");
        }
    }
}
