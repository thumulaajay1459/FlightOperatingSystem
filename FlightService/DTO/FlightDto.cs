using System.ComponentModel.DataAnnotations;

namespace FlightService.DTOs
{
    public class FlightDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public int AircraftId { get; set; }
        public int RouteId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Status { get; set; } = "Scheduled";
    }

    public class FlightSummaryDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public int DistanceKm { get; set; }
    }

    public class FlightDetailDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public AircraftDto? Aircraft { get; set; }
        public RouteDto? Route { get; set; }
    }

    public class CreateFlightDto
    {
        [Required]
        public string FlightNumber { get; set; } = string.Empty;

        [Required]
        public int AircraftId { get; set; }

        [Required]
        public int RouteId { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        public string Status { get; set; } = "Scheduled";

        public decimal BasePrice { get; set; } = 5000;
        public decimal EconomyPrice { get; set; } = 5000;
        public decimal PremiumEconomyPrice { get; set; } = 8000;
        public decimal BusinessPrice { get; set; } = 15000;
        public decimal FirstClassPrice { get; set; } = 25000;
        public decimal ChildDiscountPercent { get; set; } = 25;
        public decimal InfantDiscountPercent { get; set; } = 90;
    }

    public class UpdateFlightDto
    {
        [Required]
        public string FlightNumber { get; set; } = string.Empty;

        [Required]
        public int AircraftId { get; set; }

        [Required]
        public int RouteId { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;
        
        public string? Comments { get; set; }
        
        public int? DelayedMinutes { get; set; }
    }

    public class UpdateFlightPricingDto
    {
        public decimal BasePrice { get; set; }
        public decimal EconomyPrice { get; set; }
        public decimal PremiumEconomyPrice { get; set; }
        public decimal BusinessPrice { get; set; }
        public decimal FirstClassPrice { get; set; }
        public decimal ChildDiscountPercent { get; set; }
        public decimal InfantDiscountPercent { get; set; }
    }

    public class FlightSearchDto
    {
        public int FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string OriginCode { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string DestinationCode { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public double DurationHours { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Aircraft { get; set; } = string.Empty;
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public decimal EconomyPrice { get; set; }
        public decimal PremiumEconomyPrice { get; set; }
        public decimal BusinessPrice { get; set; }
        public decimal FirstClassPrice { get; set; }
        public decimal ChildDiscountPercent { get; set; }
        public decimal InfantDiscountPercent { get; set; }
    }
}
