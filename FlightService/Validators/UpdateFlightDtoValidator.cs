using FluentValidation;
using FlightService.DTOs;

namespace FlightService.Validators
{
    public class UpdateFlightDtoValidator : AbstractValidator<UpdateFlightDto>
    {
        public UpdateFlightDtoValidator()
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
                .NotEmpty().WithMessage("Departure time is required");

            RuleFor(x => x.ArrivalTime)
                .GreaterThan(x => x.DepartureTime).WithMessage("Arrival time must be after departure time");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .Must(s => s == "Scheduled" || s == "Delayed" || s == "Cancelled" || s == "Completed")
                .WithMessage("Status must be Scheduled, Delayed, Cancelled, or Completed");
        }
    }
}
