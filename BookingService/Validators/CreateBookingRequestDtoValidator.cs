using FluentValidation;
using BookingService.DTOs;

namespace BookingService.Validators
{
    public class CreateBookingRequestDtoValidator : AbstractValidator<CreateBookingRequestDto>
    {
        public CreateBookingRequestDtoValidator()
        {
            RuleFor(x => x.FlightId)
                .GreaterThan(0).WithMessage("Flight ID must be greater than 0");

            RuleFor(x => x.ReturnFlightId)
                .GreaterThan(0).WithMessage("Return Flight ID must be greater than 0")
                .When(x => x.ReturnFlightId.HasValue);

            RuleFor(x => x.BookingType)
                .NotEmpty().WithMessage("Booking type is required")
                .Must(bt => bt == "OneWay" || bt == "RoundTrip" || bt == "MultiCity")
                .WithMessage("Booking type must be OneWay, RoundTrip, or MultiCity");

            RuleFor(x => x.TotalAmount)
                .GreaterThan(0).WithMessage("Total amount must be greater than 0");

            RuleFor(x => x.SeatNumber)
                .NotEmpty().WithMessage("Seat number is required when using profile as passenger")
                .When(x => x.UseProfileAsPassenger);

            RuleFor(x => x.Passengers)
                .NotEmpty().WithMessage("At least one passenger is required")
                .When(x => !x.UseProfileAsPassenger);

            RuleForEach(x => x.Passengers)
                .SetValidator(new PassengerDtoValidator())
                .When(x => !x.UseProfileAsPassenger);
        }
    }
}
