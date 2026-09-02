using FluentValidation;
using FlightService.DTOs;

namespace FlightService.Validators
{
    public class CreateAircraftDtoValidator : AbstractValidator<CreateAircraftDto>
    {
        public CreateAircraftDtoValidator()
        {
            RuleFor(x => x.Manufacturer)
                .NotEmpty().WithMessage("Manufacturer is required")
                .Length(2, 50).WithMessage("Manufacturer must be between 2 and 50 characters");

            RuleFor(x => x.Model)
                .NotEmpty().WithMessage("Model is required")
                .Length(2, 50).WithMessage("Model must be between 2 and 50 characters");

            RuleFor(x => x.TotalSeats)
                .InclusiveBetween(1, 1000).WithMessage("Total seats must be between 1 and 1000");
        }
    }
}
