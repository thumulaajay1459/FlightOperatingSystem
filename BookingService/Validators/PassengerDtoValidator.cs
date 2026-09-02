using FluentValidation;
using BookingService.DTOs;

namespace BookingService.Validators
{
    public class PassengerDtoValidator : AbstractValidator<PassengerDto>
    {
        public PassengerDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .Length(2, 100).WithMessage("Full name must be between 2 and 100 characters")
                .Matches("^[a-zA-Z ]+$").WithMessage("Full name can only contain letters and spaces");

            RuleFor(x => x.Age)
                .InclusiveBetween(0, 120).WithMessage("Age must be between 0 and 120");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required")
                .Must(g => g == "Male" || g == "Female" || g == "Other")
                .WithMessage("Gender must be Male, Female, or Other");

            RuleFor(x => x.PassengerType)
                .NotEmpty().WithMessage("Passenger type is required")
                .Must(pt => pt == "Adult" || pt == "Child" || pt == "Infant")
                .WithMessage("Passenger type must be Adult, Child, or Infant");

            RuleFor(x => x.PassportNumber)
                .NotEmpty().WithMessage("Passport number is required")
                .Length(6, 20).WithMessage("Passport number must be between 6 and 20 characters")
                .Matches("^[A-Z0-9]+$").WithMessage("Passport number must contain only uppercase letters and numbers");

            RuleFor(x => x.SeatNumber)
                .NotEmpty().WithMessage("Seat number is required")
                .Matches("^[A-Z0-9]+$").WithMessage("Seat number must be alphanumeric");
        }
    }
}
