using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightService.Models
{
    public class Flight
    {
        [Key]
        public int FlightId { get; set; }

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
        
        public string? Comments { get; set; }
        
        public int? DelayedMinutes { get; set; }
        
        public DateTime? ActualDepartureTime { get; set; }
        
        public DateTime? ActualArrivalTime { get; set; }

        // Pricing
        public decimal BasePrice { get; set; } = 0;
        public decimal EconomyPrice { get; set; } = 0;
        public decimal PremiumEconomyPrice { get; set; } = 0;
        public decimal BusinessPrice { get; set; } = 0;
        public decimal FirstClassPrice { get; set; } = 0;

        // Passenger Type Multipliers
        public decimal ChildDiscountPercent { get; set; } = 25; // 25% discount for children
        public decimal InfantDiscountPercent { get; set; } = 90; // 90% discount for infants

        [ForeignKey("AircraftId")]
        public Aircraft? Aircraft { get; set; }

        [ForeignKey("RouteId")]
        public FlightService.Models.Route? Route { get; set; }
    }
}
