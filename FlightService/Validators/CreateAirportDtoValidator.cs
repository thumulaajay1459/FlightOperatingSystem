using FluentValidation;
using FlightService.DTOs;

namespace FlightService.Validators
{
    public class CreateAirportDtoValidator : AbstractValidator<CreateAirportDto>
    {
        public CreateAirportDtoValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Airport code is required")
                .Length(3, 3).WithMessage("Airport code must be exactly 3 characters")
                .Matches("^[A-Z]+$").WithMessage("Airport code must contain only uppercase letters");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Airport name is required")
                .Length(3, 100).WithMessage("Airport name must be between 3 and 100 characters");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required")
                .Length(2, 50).WithMessage("City must be between 2 and 50 characters");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Country is required")
                .Length(2, 50).WithMessage("Country must be between 2 and 50 characters");
        }
    }
}
