using FluentValidation;
using UserService.DTOs;

namespace UserService.Validators
{
    public class UserProfileDtoValidator : AbstractValidator<UserProfileDto>
    {
        public UserProfileDtoValidator()
        {
            RuleFor(x => x.PassportNumber)
                .NotEmpty().WithMessage("Passport number is required")
                .Length(6, 20).WithMessage("Passport number must be between 6 and 20 characters")
                .Matches("^[A-Z0-9]+$").WithMessage("Passport number must contain only uppercase letters and numbers");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required")
                .Must(g => g == "Male" || g == "Female" || g == "Other")
                .WithMessage("Gender must be Male, Female, or Other");

            RuleFor(x => x.Age)
                .InclusiveBetween(0, 120).WithMessage("Age must be between 0 and 120");

            RuleFor(x => x.Nationality)
                .NotEmpty().WithMessage("Nationality is required")
                .Length(2, 50).WithMessage("Nationality must be between 2 and 50 characters");

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Now).WithMessage("Date of birth must be in the past")
                .GreaterThan(DateTime.Now.AddYears(-120)).WithMessage("Date of birth cannot be more than 120 years ago")
                .When(x => x.DateOfBirth.HasValue);
        }
    }
}
