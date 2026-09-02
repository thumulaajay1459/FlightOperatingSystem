using FluentValidation;
using FlightService.DTOs;

namespace FlightService.Validators
{
    public class CreateRouteDtoValidator : AbstractValidator<CreateRouteDto>
    {
        public CreateRouteDtoValidator()
        {
            RuleFor(x => x.OriginAirportId)
                .GreaterThan(0).WithMessage("Origin airport ID must be greater than 0");

            RuleFor(x => x.DestinationAirportId)
                .GreaterThan(0).WithMessage("Destination airport ID must be greater than 0")
                .NotEqual(x => x.OriginAirportId).WithMessage("Destination airport must be different from origin airport");

            RuleFor(x => x.DistanceKm)
                .InclusiveBetween(1, 20000).WithMessage("Distance must be between 1 and 20000 km");
        }
    }
}
